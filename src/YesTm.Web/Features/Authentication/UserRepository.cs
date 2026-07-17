using Dapper;
using YesTm.Web.Common.Data;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Authentication;

public interface IUserRepository
{
    /// <summary>Validates credentials against AMM_USER_DETAILS. Returns the user or null.</summary>
    Task<AMM_USER_DETAILS?> ValidateCredentialsAsync(string userName, string password, CancellationToken ct = default);

    /// <summary>Resolves the effective application role for a user (defaults to USER).</summary>
    Task<string> GetEffectiveRoleAsync(decimal userId, CancellationToken ct = default);

    /// <summary>
    /// Atomically redeems a one-time auto-login token: marks it used only if it is
    /// currently unused and unexpired. Returns the user id + ERP role name it carried,
    /// or null when the token is unknown, already used, or expired.
    /// </summary>
    Task<AutoLoginTicket?> ConsumeAutoLoginTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Loads a user by USER_ID to build the auth claims (null if not found/inactive).</summary>
    Task<AMM_USER_DETAILS?> GetUserByIdAsync(decimal userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a TM_USER_ROLE row for the user from the mapped role ONLY when the user has
    /// no active assignment yet (auto-provisioning). An existing active role is left untouched.
    /// </summary>
    Task EnsureRoleAsync(decimal userId, string tmRoleCode, CancellationToken ct = default);
}

/// <summary>Result of redeeming an auto-login token.</summary>
public readonly record struct AutoLoginTicket(decimal UserId, string ErpRoleName);

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbConnectionFactory db, ILogger<UserRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AMM_USER_DETAILS?> ValidateCredentialsAsync(string userName, string password, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT USER_ID, USER_CODE, USER_NAME, USER_SHORT_NAME, USER_PASSWORD,
                   USER_EMP_CODE, USER_EMAIL_ID, USER_DEFAULT_LOCATION, USER_ACTIVE_YN,
                   USER_ACTIVE_FROM_DATE, USER_ACTIVE_TO_DATE
            FROM   AMM_USER_DETAILS
            WHERE  UPPER(USER_NAME) = UPPER(:userName)
              AND  USER_PASSWORD = :password
              AND  NVL(USER_ACTIVE_YN, 'Y') = 'Y'";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var user = await conn.QueryFirstOrDefaultAsync<AMM_USER_DETAILS>(
            new CommandDefinition(sql, new { userName, password }, cancellationToken: ct));

        if (user is null)
            _logger.LogInformation("Failed login attempt for {UserName}", userName);

        return user;
    }

    public async Task<string> GetEffectiveRoleAsync(decimal userId, CancellationToken ct = default)
    {
        // SITEADMIN outranks ADMIN outranks USER; pick the highest active assignment.
        const string sql = @"
            SELECT TUR_ROLE_CODE
            FROM   TM_USER_ROLE
            WHERE  TUR_USER_ID = :userId
              AND  NVL(TUR_ACTIVE_YN, 'Y') = 'Y'
            ORDER  BY CASE TUR_ROLE_CODE
                        WHEN 'SITEADMIN' THEN 1
                        WHEN 'ADMIN'     THEN 2
                        ELSE 3
                      END
            FETCH FIRST 1 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var role = await conn.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { userId }, cancellationToken: ct));

        return Roles.IsKnown(role) ? role! : Roles.User;
    }

    public async Task<AutoLoginTicket?> ConsumeAutoLoginTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            // Redeem atomically: only one caller can flip N -> Y for an unexpired token.
            var affected = await conn.ExecuteAsync(new CommandDefinition(
                @"UPDATE TM_AUTO_LOGIN_TOKEN
                     SET TAT_USED_YN = 'Y'
                   WHERE TAT_TOKEN = :token
                     AND TAT_USED_YN = 'N'
                     AND TAT_EXPIRY_DATE > SYSDATE",
                new { token }, tx, cancellationToken: ct));

            if (affected != 1)
            {
                tx.Rollback();
                _logger.LogInformation("Auto-login token rejected (unknown, used, or expired)");
                return null;
            }

            var ticket = await conn.QueryFirstOrDefaultAsync<AutoLoginTicket>(new CommandDefinition(
                @"SELECT TAT_USER_ID AS UserId, TAT_ERP_ROLE_NAME AS ErpRoleName
                    FROM TM_AUTO_LOGIN_TOKEN
                   WHERE TAT_TOKEN = :token",
                new { token }, tx, cancellationToken: ct));

            tx.Commit();
            return ticket;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Failed to consume auto-login token");
            throw;
        }
    }

    public async Task<AMM_USER_DETAILS?> GetUserByIdAsync(decimal userId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT USER_ID, USER_CODE, USER_NAME, USER_SHORT_NAME, USER_PASSWORD,
                   USER_EMP_CODE, USER_EMAIL_ID, USER_DEFAULT_LOCATION, USER_ACTIVE_YN,
                   USER_ACTIVE_FROM_DATE, USER_ACTIVE_TO_DATE
            FROM   AMM_USER_DETAILS
            WHERE  USER_ID = :userId
              AND  NVL(USER_ACTIVE_YN, 'Y') = 'Y'";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<AMM_USER_DETAILS>(
            new CommandDefinition(sql, new { userId }, cancellationToken: ct));
    }

    public async Task EnsureRoleAsync(decimal userId, string tmRoleCode, CancellationToken ct = default)
    {
        if (!Roles.IsKnown(tmRoleCode))
            throw new ArgumentException($"Unknown role '{tmRoleCode}'.", nameof(tmRoleCode));

        using var conn = await _db.CreateOpenConnectionAsync(ct);

        var hasActive = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            @"SELECT COUNT(*) FROM TM_USER_ROLE
               WHERE TUR_USER_ID = :userId AND NVL(TUR_ACTIVE_YN,'Y') = 'Y'",
            new { userId }, cancellationToken: ct));

        if (hasActive > 0)
            return; // respect an existing (e.g. SITEADMIN-configured) assignment.

        await conn.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO TM_USER_ROLE
                  (TUR_ID, TUR_USER_ID, TUR_ROLE_CODE, TUR_ACTIVE_YN, TUR_CREATION_USER_ID, TUR_CREATION_DATE)
               VALUES
                  (TM_USER_ROLE_SEQ.NEXTVAL, :userId, :tmRoleCode, 'Y', :userId, SYSDATE)",
            new { userId, tmRoleCode }, cancellationToken: ct));

        _logger.LogInformation("Auto-provisioned TM role {Role} for user {UserId}", tmRoleCode, userId);
    }
}
