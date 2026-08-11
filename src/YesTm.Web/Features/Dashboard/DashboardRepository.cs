using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.Dashboard;

public sealed class DashboardSummary
{
    public decimal SHEET_COUNT { get; set; }
    public decimal TOTAL_NET_HOURS { get; set; }
    public decimal TOTAL_LABOUR_COST { get; set; }
    public decimal NORMAL_HOURS { get; set; }
    public decimal OT_HOURS { get; set; }
    public decimal STD_HOURS { get; set; }
    public decimal TECH_COUNT { get; set; }
}

public interface IDashboardRepository
{
    Task<DashboardSummary> GetMonthSummaryAsync(decimal? scopeUserId, CancellationToken ct = default);
}

public sealed class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _db;

    public DashboardRepository(IDbConnectionFactory db, ILogger<DashboardRepository> logger)
    {
        _db = db;
        Logger = logger;
    }

    public ILogger<DashboardRepository> Logger { get; }

    public async Task<DashboardSummary> GetMonthSummaryAsync(decimal? scopeUserId, CancellationToken ct = default)
    {
        const string sql = @"
           SELECT
    COUNT(DISTINCT s.TS_ID)           AS SHEET_COUNT,
    NVL(SUM(s.TS_TOTAL_NET_HOURS),0)  AS TOTAL_NET_HOURS,
    NVL(SUM(s.TS_TOTAL_LABOUR_COST),0) AS TOTAL_LABOUR_COST,
    NVL(SUM(s.TS_NORMAL_HOURS),0)     AS NORMAL_HOURS,
    NVL(SUM(s.TS_OT_HOURS),0)         AS OT_HOURS,
    NVL(SUM(s.TS_STD_HOURS),0)        AS STD_HOURS,
    min(t.TECH_COUNT) TECH_COUNT
FROM TM_TIME_SHEET s
CROSS JOIN
(
    SELECT COUNT(DISTINCT l.TL_TECH_CODE) AS TECH_COUNT
    FROM TM_TIME_LINE l
    JOIN TM_TIME_SHEET h
      ON h.TS_ID = l.TL_TS_ID
    WHERE h.TS_POSTING_DATE >= TRUNC(SYSDATE,'MM')
      AND NVL(h.TS_ACTIVE_YN,'Y')='Y'
      AND (:scopeUserId IS NULL OR h.TS_CREATION_USER_ID = :scopeUserId)
) t
WHERE s.TS_POSTING_DATE >= TRUNC(SYSDATE,'MM')
  AND NVL(s.TS_ACTIVE_YN,'Y')='Y'
  AND (:scopeUserId IS NULL OR s.TS_CREATION_USER_ID = :scopeUserId)";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        Logger.LogError($"DBoard:: {sql} scopeUserId::{scopeUserId}");
        var summary = await conn.QueryFirstOrDefaultAsync<DashboardSummary>(
            new CommandDefinition(sql, new { scopeUserId }, cancellationToken: ct));
        return summary ?? new DashboardSummary();
    }
}
