using Dapper;
using YesTm.Web.Common.Data;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Administration;

public sealed class UserRoleListItem
{
    public decimal USER_ID { get; set; }
    public string? USER_CODE { get; set; }
    public string? USER_NAME { get; set; }
    public string? USER_EMAIL_ID { get; set; }
    public string? ROLE_CODE { get; set; }
}

public interface IAdministrationRepository
{
    Task<IReadOnlyList<UserRoleListItem>> GetUsersWithRolesAsync(string? search, CancellationToken ct = default);
    Task SetUserRoleAsync(decimal userId, string roleCode, decimal actingUserId, CancellationToken ct = default);
}

public sealed class AdministrationRepository : IAdministrationRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<AdministrationRepository> _logger;

    public AdministrationRepository(IDbConnectionFactory db, ILogger<AdministrationRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserRoleListItem>> GetUsersWithRolesAsync(string? search, CancellationToken ct = default)
    {
        // Each user's current (highest active) role, defaulting to USER when none assigned.
        const string sql = @"
            SELECT u.USER_ID, u.USER_CODE, u.USER_NAME, u.USER_EMAIL_ID,
                   NVL((SELECT r.TUR_ROLE_CODE
                          FROM TM_USER_ROLE r
                         WHERE r.TUR_USER_ID = u.USER_ID AND NVL(r.TUR_ACTIVE_YN,'Y') = 'Y'
                         ORDER BY CASE r.TUR_ROLE_CODE WHEN 'SITEADMIN' THEN 1 WHEN 'ADMIN' THEN 2 ELSE 3 END
                         FETCH FIRST 1 ROWS ONLY), 'USER') AS ROLE_CODE
            FROM   AMM_USER_DETAILS u
            WHERE  NVL(u.USER_ACTIVE_YN,'Y') = 'Y'
              AND  (:search IS NULL
                    OR UPPER(u.USER_NAME) LIKE '%' || UPPER(:search) || '%'
                    OR UPPER(u.USER_CODE) LIKE '%' || UPPER(:search) || '%')
            ORDER  BY u.USER_NAME
            FETCH FIRST 200 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<UserRoleListItem>(
            new CommandDefinition(sql, new { search }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task SetUserRoleAsync(decimal userId, string roleCode, decimal actingUserId, CancellationToken ct = default)
    {
        if (!Roles.IsKnown(roleCode))
            throw new ArgumentException($"Unknown role '{roleCode}'.", nameof(roleCode));

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            // Retire any existing active assignments, then add the new one (keeps history).
            await conn.ExecuteAsync(new CommandDefinition(
                @"UPDATE TM_USER_ROLE
                     SET TUR_ACTIVE_YN = 'N', TUR_UPDATE_USER_ID = :actingUserId, TUR_UPDATE_DATE = SYSDATE
                   WHERE TUR_USER_ID = :userId AND NVL(TUR_ACTIVE_YN,'Y') = 'Y'",
                new { actingUserId, userId }, tx, cancellationToken: ct));

            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO TM_USER_ROLE
                      (TUR_ID, TUR_USER_ID, TUR_ROLE_CODE, TUR_ACTIVE_YN, TUR_CREATION_USER_ID, TUR_CREATION_DATE)
                   VALUES
                      (TM_USER_ROLE_SEQ.NEXTVAL, :userId, :roleCode, 'Y', :actingUserId, SYSDATE)",
                new { userId, roleCode, actingUserId }, tx, cancellationToken: ct));

            tx.Commit();
            _logger.LogInformation("User {UserId} role set to {Role} by {ActingUser}", userId, roleCode, actingUserId);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Failed to set role {Role} for user {UserId}", roleCode, userId);
            throw;
        }
    }
}
