using System.Data;
using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.TimeBooking;

public sealed record LabourRateResult(decimal Rate, decimal OvertimeMultiplier);

public interface ITimeBookingRepository
{
    Task<IReadOnlyList<TimeSheetListItem>> GetRecentTimeSheetsAsync(decimal? scopeUserId, int take, CancellationToken ct = default);
    Task<IReadOnlyList<JobLookup>> GetJobsAsync(CancellationToken ct = default);
    Task<JobLookup?> GetJobAsync(string jobCode, CancellationToken ct = default);
    Task<IReadOnlyList<TechnicianLookup>> GetTechniciansAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskLookup>> GetTasksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WorkTypeLookup>> GetWorkTypesAsync(CancellationToken ct = default);
    Task<DutyTimingDto> GetDutyTimingAsync(CancellationToken ct = default);
    Task<decimal> GetTaskStdHoursAsync(string taskCode, CancellationToken ct = default);
    Task<LabourRateResult> GetLabourRateAsync(string? location, string? industryCode, CancellationToken ct = default);
    Task<string> SaveTimeSheetAsync(TM_TIME_SHEET header, IReadOnlyList<TM_TIME_LINE> lines, CancellationToken ct = default);
}

public sealed class TimeBookingRepository : ITimeBookingRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<TimeBookingRepository> _logger;

    public TimeBookingRepository(IDbConnectionFactory db, ILogger<TimeBookingRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TimeSheetListItem>> GetRecentTimeSheetsAsync(decimal? scopeUserId, int take, CancellationToken ct = default)
    {
        var sql = @"
            SELECT * FROM (
                SELECT TS_ID, TS_SHEET_NO, TS_POSTING_DATE, TS_JOB_CODE, TS_CUSTOMER_NAME,
                       TS_LOCATION, TS_JOB_STATUS, TS_TOTAL_NET_HOURS, TS_TOTAL_LABOUR_COST
                FROM   TM_TIME_SHEET
                WHERE  NVL(TS_ACTIVE_YN,'Y') = 'Y'
                  AND  (:scopeUserId IS NULL OR TS_CREATION_USER_ID = :scopeUserId)
                ORDER  BY TS_POSTING_DATE DESC, TS_ID DESC
            ) WHERE ROWNUM <= :take";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<TimeSheetListItem>(
            new CommandDefinition(sql, new { scopeUserId, take }, cancellationToken: ct));
        return rows.AsList();
    }

    // Job header + customer + mapped industry + equipment details (brand, type,
    // service type, serial, opening date) resolved via the ERP lookup masters.
    // Left joins so a job always appears even when a lookup is missing.
    private const string JobSelectBase = @"
        SELECT j.MTJ_JOB_CODE, j.MTJ_JOB_DESC, j.MTJ_PARTY_CODE,
               p.MPD_BRANCH_NAME AS CUSTOMER_NAME, j.MTJ_BRAND, j.MTJ_JOB_LOCATION,
               m.CIM_INDUSTRY_CODE AS INDUSTRY_CODE,
               b.MCMD_ENTITY_DESC AS BRAND_DESC,
               e.MET_EQUIPMENT_TYPE_DESC AS EQUIPMENT_TYPE_DESC,
               s.MST_SERVICE_TYPE_DESC AS SERVICE_TYPE_DESC,
               j.MTJ_SERIAL_NO, j.MTJ_JOB_OPENING_DATE
        FROM   MTL_TRANSACTION_JOB_OTHER_DTLS j
        LEFT JOIN MMM_PARTY_DETAILS p
               ON p.MPD_PARTY_CODE = j.MTJ_PARTY_CODE AND p.MPD_PARTY_TYPE = 'AR'
        LEFT JOIN TM_CUST_INDUSTRY_MAP m
               ON m.CIM_CUSTOMER_CODE = j.MTJ_PARTY_CODE AND NVL(m.CIM_ACTIVE_YN,'Y') = 'Y'
        LEFT JOIN MMM_EQUIPMENT_TYPE e
               ON e.MET_EQUIPMENT_TYPE_ID = j.MTJ_EQUIPMENT_TYPE_ID
        LEFT JOIN MMM_SERVICE_TYPE s
               ON s.MST_SERVICE_TYPE_ID = j.MTJ_SERVICE_TYPE_ID
        LEFT JOIN MMM_COMMON_MASTERS_DETAIL b
               ON UPPER(b.MCMD_ENTITY_GROUP) = 'AGENCY' AND b.MCMD_ENTITY_CODE = j.MTJ_BRAND";

    public async Task<IReadOnlyList<JobLookup>> GetJobsAsync(CancellationToken ct = default)
    {
        var sql = JobSelectBase + @"
            WHERE NVL(j.MTJ_JOB_STATUS,'OPEN') <> 'CLOSED'
            ORDER BY j.MTJ_JOB_OPENING_DATE DESC NULLS LAST, j.MTJ_JOB_CODE";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<JobLookup>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<JobLookup?> GetJobAsync(string jobCode, CancellationToken ct = default)
    {
        var sql = JobSelectBase + @"
            WHERE j.MTJ_JOB_CODE = :jobCode
            FETCH FIRST 1 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<JobLookup>(
            new CommandDefinition(sql, new { jobCode }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<TechnicianLookup>> GetTechniciansAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT PEMP_EMP_CODE, PEMP_EMP_NAME
            FROM   PPM_EMPLOYEE_DETAILS
            WHERE  PEMP_EMP_CODE IS NOT NULL
            ORDER  BY PEMP_EMP_NAME";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<TechnicianLookup>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<TaskLookup>> GetTasksAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TASK_CODE, TASK_NAME, DEFAULT_SKILL, STD_HOURS
            FROM   TM_TASK
            WHERE  NVL(ACTIVE_YN,'Y') = 'Y'
            ORDER  BY SEQ_NO NULLS LAST, TASK_NAME";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<TaskLookup>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<IReadOnlyList<WorkTypeLookup>> GetWorkTypesAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT WORK_TYPE_CODE, WORK_TYPE_NAME
            FROM   TM_WORK_TYPE
            WHERE  NVL(ACTIVE_YN,'Y') = 'Y'
            ORDER  BY WORK_TYPE_NAME";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<WorkTypeLookup>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<DutyTimingDto> GetDutyTimingAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT DT_START AS DUTY_START, DT_END AS DUTY_END,
                   DT_LUNCH_HOURS AS DEFAULT_LUNCH_HOURS, DT_WEEKEND_DAYS AS WEEKEND_DAYS
            FROM   TM_DUTY_TIMING
            WHERE  NVL(ACTIVE_YN,'Y') = 'Y'
            ORDER  BY DT_ID
            FETCH FIRST 1 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var row = await conn.QueryFirstOrDefaultAsync<DutyTimingDto>(new CommandDefinition(sql, cancellationToken: ct));
        return row ?? new DutyTimingDto();
    }

    public async Task<decimal> GetTaskStdHoursAsync(string taskCode, CancellationToken ct = default)
    {
        const string sql = @"SELECT NVL(STD_HOURS, 0) FROM TM_TASK WHERE TASK_CODE = :taskCode";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<decimal?>(
            new CommandDefinition(sql, new { taskCode }, cancellationToken: ct)) ?? 0m;
    }

    public async Task<LabourRateResult> GetLabourRateAsync(string? location, string? industryCode, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT LR_RATE, NVL(LR_OT_MULTIPLIER, 1.5) AS LR_OT_MULTIPLIER
            FROM   TM_LABOUR_RATE
            WHERE  UPPER(LR_LOCATION) = UPPER(:location)
              AND  (LR_INDUSTRY_CODE = :industryCode OR :industryCode IS NULL)
              AND  NVL(LR_ACTIVE_YN,'Y') = 'Y'
            ORDER  BY LR_EFFECTIVE_FROM DESC NULLS LAST
            FETCH FIRST 1 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var row = await conn.QueryFirstOrDefaultAsync<(decimal LR_RATE, decimal LR_OT_MULTIPLIER)>(
            new CommandDefinition(sql, new { location, industryCode }, cancellationToken: ct));
        return new LabourRateResult(row.LR_RATE, row.LR_OT_MULTIPLIER == 0 ? 1.5m : row.LR_OT_MULTIPLIER);
    }

    public async Task<string> SaveTimeSheetAsync(TM_TIME_SHEET header, IReadOnlyList<TM_TIME_LINE> lines, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            // Generate sheet no + id from sequence.
            var newId = await conn.ExecuteScalarAsync<decimal>(
                new CommandDefinition("SELECT TM_TIME_SHEET_SEQ.NEXTVAL FROM DUAL", transaction: tx, cancellationToken: ct));
            header.TS_ID = newId;
            header.TS_SHEET_NO = $"TS-{DateTime.Now:yyyy}-{newId:000000}";

            const string insHeader = @"
                INSERT INTO TM_TIME_SHEET
                    (TS_ID, TS_SHEET_NO, TS_POSTING_DATE, TS_JOB_CODE, TS_CUSTOMER_CODE, TS_CUSTOMER_NAME,
                     TS_EQUIPMENT, TS_INDUSTRY_CODE, TS_LOCATION, TS_WORK_TYPE_CODE, TS_JOB_STATUS,
                     TS_BRAND, TS_EQUIPMENT_TYPE, TS_SERVICE_TYPE, TS_SERIAL_NO, TS_JOB_OPENING_DATE,
                     TS_TOTAL_NET_HOURS, TS_TOTAL_LABOUR_COST, TS_NORMAL_HOURS, TS_OT_HOURS, TS_STD_HOURS,
                     TS_REMARKS, TS_ACTIVE_YN, TS_CREATION_USER_ID, TS_CREATION_DATE)
                VALUES
                    (:TS_ID, :TS_SHEET_NO, :TS_POSTING_DATE, :TS_JOB_CODE, :TS_CUSTOMER_CODE, :TS_CUSTOMER_NAME,
                     :TS_EQUIPMENT, :TS_INDUSTRY_CODE, :TS_LOCATION, :TS_WORK_TYPE_CODE, :TS_JOB_STATUS,
                     :TS_BRAND, :TS_EQUIPMENT_TYPE, :TS_SERVICE_TYPE, :TS_SERIAL_NO, :TS_JOB_OPENING_DATE,
                     :TS_TOTAL_NET_HOURS, :TS_TOTAL_LABOUR_COST, :TS_NORMAL_HOURS, :TS_OT_HOURS, :TS_STD_HOURS,
                     :TS_REMARKS, 'Y', :TS_CREATION_USER_ID, SYSDATE)";
            await conn.ExecuteAsync(new CommandDefinition(insHeader, header, tx, cancellationToken: ct));

            const string insLine = @"
                INSERT INTO TM_TIME_LINE
                    (TL_ID, TL_TS_ID, TL_LINE_NO, TL_TASK_CODE, TL_TASK_NAME, TL_STD_HOURS,
                     TL_TECH_CODE, TL_TECH_NAME, TL_SKILL, TL_START_DT, TL_END_DT, TL_LUNCH_HOURS,
                     TL_NET_HOURS, TL_TIME_TYPE, TL_RATE, TL_LABOUR_COST, TL_JOB_CODE, TL_CREATION_DATE)
                VALUES
                    (TM_TIME_LINE_SEQ.NEXTVAL, :TL_TS_ID, :TL_LINE_NO, :TL_TASK_CODE, :TL_TASK_NAME, :TL_STD_HOURS,
                     :TL_TECH_CODE, :TL_TECH_NAME, :TL_SKILL, :TL_START_DT, :TL_END_DT, :TL_LUNCH_HOURS,
                     :TL_NET_HOURS, :TL_TIME_TYPE, :TL_RATE, :TL_LABOUR_COST, :TL_JOB_CODE, SYSDATE)";

            foreach (var line in lines)
            {
                line.TL_TS_ID = newId;
                await conn.ExecuteAsync(new CommandDefinition(insLine, line, tx, cancellationToken: ct));
            }

            tx.Commit();
            _logger.LogInformation("Posted time sheet {SheetNo} with {LineCount} line(s), cost {Cost}",
                header.TS_SHEET_NO, lines.Count, header.TS_TOTAL_LABOUR_COST);
            return header.TS_SHEET_NO!;
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Failed to post time sheet for job {JobCode}", header.TS_JOB_CODE);
            throw;
        }
    }
}
