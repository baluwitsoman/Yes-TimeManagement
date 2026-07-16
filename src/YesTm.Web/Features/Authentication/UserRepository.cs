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
}

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
}
