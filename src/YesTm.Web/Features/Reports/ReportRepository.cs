using System.Data;
using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.Reports;

public interface IReportRepository
{
    /// <summary>
    /// Flat Task &amp; Time rows (one per booked task line). When <paramref name="paged"/> is true the result
    /// is one page (OFFSET/FETCH); when false the whole filtered set is returned (for export/print).
    /// Scope: admins pass a null userId (see all); a normal employee passes his userId + emp code and only
    /// sees lines on sheets he created or where he is the technician.
    /// </summary>
    Task<PagedReport> GetTaskTimeAsync(ReportFilter f, decimal? scopeUserId, string? scopeEmpCode, bool paged, CancellationToken ct = default);
}

public sealed class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _db;

    public ReportRepository(IDbConnectionFactory db) => _db = db;

    // Whitelist: user-chosen sort maps to a fixed column (raw text is never placed in the ORDER BY).
    private static string SortColumn(ReportSortBy by) => by switch
    {
        ReportSortBy.Employee => "l.TL_TECH_NAME",
        ReportSortBy.Customer => "s.TS_CUSTOMER_NAME",
        ReportSortBy.Date     => "s.TS_POSTING_DATE",
        ReportSortBy.Task     => "l.TL_TASK_NAME",
        ReportSortBy.Cost     => "l.TL_LABOUR_COST",
        _                     => "l.TL_JOB_CODE",
    };

    public async Task<PagedReport> GetTaskTimeAsync(ReportFilter f, decimal? scopeUserId, string? scopeEmpCode, bool paged, CancellationToken ct = default)
    {
        var page = f.PageNo < 1 ? 1 : f.PageNo;
        var pageSize = f.PageSize < 1 ? 25 : f.PageSize;
        var offset = (page - 1) * pageSize;
        var term = string.IsNullOrWhiteSpace(f.Search) ? null : f.Search.Trim();
        var empCode = string.IsNullOrWhiteSpace(scopeEmpCode) ? null : scopeEmpCode;
        var dateToExcl = f.DateTo?.Date.AddDays(1);   // inclusive "to" day; computed here to avoid ORA-00932

        // Per-field filter: only the chosen field's predicate is active (the others compare :field to fixed tokens).
        var field = f.FilterField;
        var filterSql = field switch
        {
            ReportFilterField.Job        => "UPPER(l.TL_JOB_CODE) LIKE '%' || UPPER(:term) || '%'",
            ReportFilterField.Customer   => "UPPER(s.TS_CUSTOMER_NAME) LIKE '%' || UPPER(:term) || '%'",
            ReportFilterField.Technician => "(UPPER(l.TL_TECH_NAME) LIKE '%' || UPPER(:term) || '%' OR UPPER(l.TL_TECH_CODE) LIKE '%' || UPPER(:term) || '%')",
            _                            => @"(UPPER(l.TL_JOB_CODE)     LIKE '%' || UPPER(:term) || '%'
                                            OR UPPER(s.TS_CUSTOMER_NAME) LIKE '%' || UPPER(:term) || '%'
                                            OR UPPER(l.TL_TECH_NAME)     LIKE '%' || UPPER(:term) || '%'
                                            OR UPPER(l.TL_TASK_NAME)     LIKE '%' || UPPER(:term) || '%'
                                            OR UPPER(s.TS_SHEET_NO)      LIKE '%' || UPPER(:term) || '%')",
        };

        var orderBy = SortColumn(f.SortBy) + (f.SortDesc ? " DESC" : " ASC")
                      + " NULLS LAST, s.TS_POSTING_DATE DESC, s.TS_SHEET_NO, l.TL_LINE_NO";

        var sql = $@"
            SELECT s.TS_SHEET_NO, s.TS_POSTING_DATE, l.TL_JOB_CODE, s.TS_CUSTOMER_NAME,
                   l.TL_TASK_NAME, l.TL_TECH_CODE, l.TL_TECH_NAME, l.TL_SKILL, l.TL_LOCATION,
                   l.TL_START_DT, l.TL_END_DT, l.TL_STD_HOURS, l.TL_NET_HOURS, l.TL_TIME_TYPE,
                   l.TL_RATE, l.TL_LABOUR_COST, w.WORK_TYPE_NAME, s.TS_JOB_STATUS,
                   NVL(l.TL_WORK_DATE, TRUNC(l.TL_START_DT)) AS TL_WORK_DATE,
                   NVL(l.TL_LUNCH_HOURS,0)    AS TL_LUNCH_HOURS,
                   NVL(l.TL_NORMAL_HOURS,0)   AS TL_NORMAL_HOURS,
                   NVL(l.TL_OT_HOURS,0)       AS TL_OT_HOURS,
                   NVL(l.TL_OT_RATE,0)        AS TL_OT_RATE,
                   NVL(l.TL_FOOD_ALLOWANCE,0) AS TL_FOOD_ALLOWANCE,
                   NVL(l.TL_TOTAL_COST, l.TL_LABOUR_COST) AS TL_TOTAL_COST,
                   l.TL_TRAVEL_SITE, l.TL_TRAVEL_START, l.TL_TRAVEL_END,
                   NVL(l.TL_TRAVEL_HOURS,0)   AS TL_TRAVEL_HOURS,
                   NVL(l.TL_OVERRIDE_YN,'N')  AS TL_OVERRIDE_YN,
                   COUNT(*) OVER()                    AS TOTAL_ROWS,
                   SUM(l.TL_NET_HOURS)   OVER()       AS GRAND_NET,
                   SUM(l.TL_LABOUR_COST) OVER()       AS GRAND_COST,
                   SUM(NVL(l.TL_NORMAL_HOURS,0))   OVER() AS GRAND_NORMAL,
                   SUM(NVL(l.TL_OT_HOURS,0))       OVER() AS GRAND_OT,
                   SUM(NVL(l.TL_FOOD_ALLOWANCE,0)) OVER() AS GRAND_FOOD,
                   SUM(NVL(l.TL_TOTAL_COST, l.TL_LABOUR_COST)) OVER() AS GRAND_TOTAL,
                   SUM(NVL(l.TL_TRAVEL_HOURS,0))   OVER() AS GRAND_TRAVEL
            FROM   TM_TIME_LINE l
            JOIN   TM_TIME_SHEET s ON s.TS_ID = l.TL_TS_ID
            LEFT JOIN TM_WORK_TYPE w ON w.WORK_TYPE_CODE = s.TS_WORK_TYPE_CODE
            WHERE  NVL(s.TS_ACTIVE_YN,'Y') = 'Y'
              AND  (:scopeUserId IS NULL
                    OR s.TS_CREATION_USER_ID = :scopeUserId
                    OR (:empCode IS NOT NULL AND l.TL_TECH_CODE = :empCode))
              AND  (:term IS NULL OR {filterSql})
              AND  (:dateFrom   IS NULL OR s.TS_POSTING_DATE >= :dateFrom)
              AND  (:dateToExcl IS NULL OR s.TS_POSTING_DATE <  :dateToExcl)
            ORDER  BY {orderBy}"
            + (paged ? "\n            OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY" : "");

        var p = new DynamicParameters();
        p.Add("scopeUserId", scopeUserId, DbType.Decimal);
        p.Add("empCode", empCode, DbType.String);
        p.Add("term", term, DbType.String);
        p.Add("dateFrom", f.DateFrom, DbType.Date);
        p.Add("dateToExcl", dateToExcl, DbType.Date);
        if (paged)
        {
            p.Add("offset", offset, DbType.Int32);
            p.Add("pageSize", pageSize, DbType.Int32);
        }

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = (await conn.QueryAsync<TaskTimeReportRow>(new CommandDefinition(sql, p, cancellationToken: ct))).AsList();
        var total = rows.Count > 0 ? rows[0].TOTAL_ROWS : 0;
        var totals = rows.Count > 0
            ? new ReportTotals(rows[0].GRAND_NET, rows[0].GRAND_COST, rows[0].GRAND_NORMAL, rows[0].GRAND_OT,
                               rows[0].GRAND_FOOD, rows[0].GRAND_TOTAL, rows[0].GRAND_TRAVEL)
            : ReportTotals.Zero;
        return new PagedReport(rows, total, totals);
    }
}
