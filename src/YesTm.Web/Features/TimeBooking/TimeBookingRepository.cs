using System.Data;
using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.TimeBooking;

public sealed record LabourRateResult(decimal Rate, decimal OvertimeMultiplier, decimal FoodAllowance);

public interface ITimeBookingRepository
{
    Task<PagedResult<TimeSheetListItem>> GetTimeSheetsAsync(decimal? scopeUserId, string? scopeEmpCode, string? search,
        DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken ct = default);
    Task<TimeSheetEditDto?> GetTimeSheetForEditAsync(decimal tsId, CancellationToken ct = default);
    /// <summary>True if the user created the sheet or appears as a technician on any of its lines.</summary>
    Task<bool> CanEditTimeSheetAsync(decimal tsId, decimal userId, string? empCode, CancellationToken ct = default);
    /// <summary>The active sheet already posted for a job (one sheet per job), ignoring <paramref name="excludeTsId"/>.</summary>
    Task<ExistingSheetLookup?> FindActiveSheetByJobAsync(string jobCode, decimal? excludeTsId, CancellationToken ct = default);
    /// <summary>Job code → sheet no for every active sheet, to flag jobs that already have a sheet in the Job dropdown.</summary>
    Task<IReadOnlyDictionary<string, string>> GetActiveSheetNosByJobAsync(CancellationToken ct = default);
    Task<IReadOnlyList<JobLookup>> GetJobsAsync(string? location, CancellationToken ct = default);
    Task<JobLookup?> GetJobAsync(string jobCode, CancellationToken ct = default);
    Task<IReadOnlyList<TechnicianLookup>> GetTechniciansAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TaskLookup>> GetTasksAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WorkTypeLookup>> GetWorkTypesAsync(CancellationToken ct = default);
    Task<DutyTimingDto> GetDutyTimingAsync(CancellationToken ct = default);
    Task<decimal> GetTaskStdHoursAsync(string taskCode, CancellationToken ct = default);
    Task<LabourRateResult> GetLabourRateAsync(string? location, string? industryCode, CancellationToken ct = default);
    Task<string> SaveTimeSheetAsync(TM_TIME_SHEET header, IReadOnlyList<TM_TIME_LINE> lines, CancellationToken ct = default);
    Task UpdateTimeSheetAsync(TM_TIME_SHEET header, IReadOnlyList<TM_TIME_LINE> lines, decimal? userId, CancellationToken ct = default);
    Task VoidTimeSheetAsync(decimal tsId, decimal? userId, CancellationToken ct = default);
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

    public async Task<PagedResult<TimeSheetListItem>> GetTimeSheetsAsync(decimal? scopeUserId, string? scopeEmpCode, string? search,
        DateTime? dateFrom, DateTime? dateTo, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var offset = (page - 1) * pageSize;
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var empCode = string.IsNullOrWhiteSpace(scopeEmpCode) ? null : scopeEmpCode;
        // Exclusive upper bound = the day after dateTo at midnight, so the whole "to" day is
        // included even though TS_POSTING_DATE carries a time component. Computed here (not as
        // ":dateTo + 1" in SQL) because arithmetic on an untyped NULL bind trips ORA-00932.
        var dateToExcl = dateTo?.Date.AddDays(1);

        // COUNT(*) OVER() gives the full match count on every row so one round-trip does
        // both paging and the total needed for the pager.
        const string sql = @"
            SELECT TS_ID, TS_SHEET_NO, TS_POSTING_DATE, TS_JOB_CODE, TS_CUSTOMER_NAME,
                   TS_LOCATION, TS_JOB_STATUS, TS_TOTAL_NET_HOURS, TS_TOTAL_LABOUR_COST,
                   NVL(TS_TOTAL_COST, TS_TOTAL_LABOUR_COST) AS TS_TOTAL_COST,
                   COUNT(*) OVER() AS TOTAL_ROWS
            FROM   TM_TIME_SHEET
            WHERE  NVL(TS_ACTIVE_YN,'Y') = 'Y'
              AND  (:scopeUserId IS NULL
                    OR TS_CREATION_USER_ID = :scopeUserId
                    OR (:empCode IS NOT NULL AND EXISTS (
                          SELECT 1 FROM TM_TIME_LINE l
                          WHERE l.TL_TS_ID = TS_ID AND l.TL_TECH_CODE = :empCode)))
              AND  (:term IS NULL
                    OR UPPER(TS_SHEET_NO)      LIKE '%' || UPPER(:term) || '%'
                    OR UPPER(TS_JOB_CODE)      LIKE '%' || UPPER(:term) || '%'
                    OR UPPER(TS_CUSTOMER_NAME) LIKE '%' || UPPER(:term) || '%')
              AND  (:dateFrom   IS NULL OR TS_POSTING_DATE >= :dateFrom)
              AND  (:dateToExcl IS NULL OR TS_POSTING_DATE <  :dateToExcl)
            ORDER  BY TS_POSTING_DATE DESC, TS_ID DESC
            OFFSET :offset ROWS FETCH NEXT :pageSize ROWS ONLY";

        // Explicit DbType on the nullable date binds so Oracle sees a DATE-typed parameter even
        // when the value is NULL (an untyped NULL bind would otherwise cause ORA-00932).
        var p = new DynamicParameters();
        p.Add("scopeUserId", scopeUserId, DbType.Decimal);
        p.Add("empCode", empCode, DbType.String);
        p.Add("term", term, DbType.String);
        p.Add("dateFrom", dateFrom, DbType.Date);
        p.Add("dateToExcl", dateToExcl, DbType.Date);
        p.Add("offset", offset, DbType.Int32);
        p.Add("pageSize", pageSize, DbType.Int32);

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = (await conn.QueryAsync<TimeSheetListItem>(new CommandDefinition(
            sql, p, cancellationToken: ct))).AsList();
        var total = rows.Count > 0 ? rows[0].TOTAL_ROWS : 0;
        return new PagedResult<TimeSheetListItem>(rows, total);
    }

    public async Task<TimeSheetEditDto?> GetTimeSheetForEditAsync(decimal tsId, CancellationToken ct = default)
    {
        const string headerSql = @"
            SELECT TS_ID, TS_SHEET_NO, TS_CREATION_USER_ID, TS_POSTING_DATE, TS_JOB_CODE, TS_CUSTOMER_CODE, TS_CUSTOMER_NAME,
                   TS_INDUSTRY_CODE, TS_BRAND, TS_EQUIPMENT_TYPE, TS_SERVICE_TYPE, TS_SERIAL_NO,
                   TS_JOB_OPENING_DATE, TS_LOCATION, TS_WORK_TYPE_CODE, TS_JOB_STATUS, TS_REMARKS
            FROM   TM_TIME_SHEET
            WHERE  TS_ID = :tsId AND NVL(TS_ACTIVE_YN,'Y') = 'Y'";
        const string linesSql = @"
            SELECT TL_ID, TL_TS_ID, TL_LINE_NO, TL_TASK_CODE, TL_TASK_NAME, TL_STD_HOURS,
                   TL_TECH_CODE, TL_TECH_NAME, TL_SKILL, TL_START_DT, TL_END_DT, TL_LUNCH_HOURS,
                   TL_NET_HOURS, TL_TIME_TYPE, TL_RATE, TL_LABOUR_COST, TL_JOB_CODE, TL_LOCATION,
                   NVL(TL_WORK_DATE, TRUNC(TL_START_DT)) AS TL_WORK_DATE,
                   NVL(TL_NORMAL_HOURS,0) AS TL_NORMAL_HOURS, NVL(TL_OT_HOURS,0) AS TL_OT_HOURS,
                   NVL(TL_OT_RATE,0) AS TL_OT_RATE, NVL(TL_FOOD_ALLOWANCE,0) AS TL_FOOD_ALLOWANCE,
                   NVL(TL_TOTAL_COST, TL_LABOUR_COST) AS TL_TOTAL_COST,
                   TL_TRAVEL_SITE, TL_TRAVEL_START, TL_TRAVEL_END, NVL(TL_TRAVEL_HOURS,0) AS TL_TRAVEL_HOURS,
                   NVL(TL_OVERRIDE_YN,'N') AS TL_OVERRIDE_YN, TL_MERGED_FROM_TS_ID
            FROM   TM_TIME_LINE
            WHERE  TL_TS_ID = :tsId
            ORDER  BY TL_LINE_NO";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var header = await conn.QueryFirstOrDefaultAsync<TimeSheetEditDto>(
            new CommandDefinition(headerSql, new { tsId }, cancellationToken: ct));
        if (header is null) return null;
        header.Lines = (await conn.QueryAsync<TM_TIME_LINE>(
            new CommandDefinition(linesSql, new { tsId }, cancellationToken: ct))).AsList();
        return header;
    }

    public async Task<bool> CanEditTimeSheetAsync(decimal tsId, decimal userId, string? empCode, CancellationToken ct = default)
    {
        // Authoritative (server-side) involvement check: creator of the sheet, or a technician on any line.
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                       SELECT 1 FROM TM_TIME_SHEET s
                       WHERE  s.TS_ID = :tsId AND NVL(s.TS_ACTIVE_YN,'Y') = 'Y'
                         AND (s.TS_CREATION_USER_ID = :userId
                              OR (:empCode IS NOT NULL AND EXISTS (
                                    SELECT 1 FROM TM_TIME_LINE l
                                    WHERE l.TL_TS_ID = s.TS_ID AND l.TL_TECH_CODE = :empCode)))
                   ) THEN 1 ELSE 0 END
            FROM DUAL";
        var p = new DynamicParameters();
        p.Add("tsId", tsId, DbType.Decimal);
        p.Add("userId", userId, DbType.Decimal);
        p.Add("empCode", string.IsNullOrWhiteSpace(empCode) ? null : empCode, DbType.String);

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var flag = await conn.ExecuteScalarAsync<int>(new CommandDefinition(sql, p, cancellationToken: ct));
        return flag == 1;
    }

    public async Task<ExistingSheetLookup?> FindActiveSheetByJobAsync(string jobCode, decimal? excludeTsId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TS_ID, TS_SHEET_NO, TS_JOB_CODE
            FROM   TM_TIME_SHEET
            WHERE  TS_JOB_CODE = :jobCode
              AND  NVL(TS_ACTIVE_YN,'Y') = 'Y'
              AND  (:excludeTsId IS NULL OR TS_ID <> :excludeTsId)
            ORDER  BY TS_ID
            FETCH FIRST 1 ROWS ONLY";
        var p = new DynamicParameters();
        p.Add("jobCode", jobCode, DbType.String);
        p.Add("excludeTsId", excludeTsId, DbType.Decimal);

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<ExistingSheetLookup>(new CommandDefinition(sql, p, cancellationToken: ct));
    }

    public async Task<IReadOnlyDictionary<string, string>> GetActiveSheetNosByJobAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TS_JOB_CODE, MIN(TS_SHEET_NO) AS TS_SHEET_NO
            FROM   TM_TIME_SHEET
            WHERE  NVL(TS_ACTIVE_YN,'Y') = 'Y' AND TS_JOB_CODE IS NOT NULL
            GROUP  BY TS_JOB_CODE";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<(string TS_JOB_CODE, string TS_SHEET_NO)>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToDictionary(r => r.TS_JOB_CODE, r => r.TS_SHEET_NO, StringComparer.OrdinalIgnoreCase);
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

    public async Task<IReadOnlyList<JobLookup>> GetJobsAsync(string? location, CancellationToken ct = default)
    {
        var sql = JobSelectBase + @"
            WHERE NVL(j.MTJ_JOB_STATUS,'OPEN') <> 'CLOSED'
              AND (:location IS NULL OR j.MTJ_JOB_LOCATION = :location)
            ORDER BY j.MTJ_JOB_OPENING_DATE DESC NULLS LAST, j.MTJ_JOB_CODE";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<JobLookup>(new CommandDefinition(sql, new { location }, cancellationToken: ct));
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
            SELECT LR_RATE, NVL(LR_OT_MULTIPLIER, 1.5) AS LR_OT_MULTIPLIER, NVL(LR_FOOD_ALLOWANCE, 0) AS LR_FOOD_ALLOWANCE
            FROM   TM_LABOUR_RATE
            WHERE  UPPER(LR_LOCATION) = UPPER(:location)
              AND  (LR_INDUSTRY_CODE = :industryCode OR :industryCode IS NULL)
              AND  NVL(LR_ACTIVE_YN,'Y') = 'Y'
            ORDER  BY LR_EFFECTIVE_FROM DESC NULLS LAST
            FETCH FIRST 1 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var row = await conn.QueryFirstOrDefaultAsync<(decimal LR_RATE, decimal LR_OT_MULTIPLIER, decimal LR_FOOD_ALLOWANCE)>(
            new CommandDefinition(sql, new { location, industryCode }, cancellationToken: ct));
        return new LabourRateResult(row.LR_RATE, row.LR_OT_MULTIPLIER == 0 ? 1.5m : row.LR_OT_MULTIPLIER, row.LR_FOOD_ALLOWANCE);
    }

    // Shared by create (insert) and edit (delete + re-insert) — every column of a task line.
    private const string InsertLineSql = @"
        INSERT INTO TM_TIME_LINE
            (TL_ID, TL_TS_ID, TL_LINE_NO, TL_TASK_CODE, TL_TASK_NAME, TL_STD_HOURS,
             TL_TECH_CODE, TL_TECH_NAME, TL_SKILL, TL_START_DT, TL_END_DT, TL_LUNCH_HOURS,
             TL_NET_HOURS, TL_TIME_TYPE, TL_RATE, TL_LABOUR_COST, TL_JOB_CODE, TL_LOCATION,
             TL_WORK_DATE, TL_NORMAL_HOURS, TL_OT_HOURS, TL_OT_RATE, TL_FOOD_ALLOWANCE, TL_TOTAL_COST,
             TL_TRAVEL_SITE, TL_TRAVEL_START, TL_TRAVEL_END, TL_TRAVEL_HOURS, TL_OVERRIDE_YN, TL_MERGED_FROM_TS_ID,
             TL_CREATION_DATE)
        VALUES
            (TM_TIME_LINE_SEQ.NEXTVAL, :TL_TS_ID, :TL_LINE_NO, :TL_TASK_CODE, :TL_TASK_NAME, :TL_STD_HOURS,
             :TL_TECH_CODE, :TL_TECH_NAME, :TL_SKILL, :TL_START_DT, :TL_END_DT, :TL_LUNCH_HOURS,
             :TL_NET_HOURS, :TL_TIME_TYPE, :TL_RATE, :TL_LABOUR_COST, :TL_JOB_CODE, :TL_LOCATION,
             :TL_WORK_DATE, :TL_NORMAL_HOURS, :TL_OT_HOURS, :TL_OT_RATE, :TL_FOOD_ALLOWANCE, :TL_TOTAL_COST,
             :TL_TRAVEL_SITE, :TL_TRAVEL_START, :TL_TRAVEL_END, :TL_TRAVEL_HOURS, :TL_OVERRIDE_YN, :TL_MERGED_FROM_TS_ID,
             SYSDATE)";

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
                     TS_TOTAL_FOOD_ALLOWANCE, TS_TOTAL_TRAVEL_HOURS, TS_TOTAL_COST,
                     TS_REMARKS, TS_ACTIVE_YN, TS_CREATION_USER_ID, TS_CREATION_DATE)
                VALUES
                    (:TS_ID, :TS_SHEET_NO, :TS_POSTING_DATE, :TS_JOB_CODE, :TS_CUSTOMER_CODE, :TS_CUSTOMER_NAME,
                     :TS_EQUIPMENT, :TS_INDUSTRY_CODE, :TS_LOCATION, :TS_WORK_TYPE_CODE, :TS_JOB_STATUS,
                     :TS_BRAND, :TS_EQUIPMENT_TYPE, :TS_SERVICE_TYPE, :TS_SERIAL_NO, :TS_JOB_OPENING_DATE,
                     :TS_TOTAL_NET_HOURS, :TS_TOTAL_LABOUR_COST, :TS_NORMAL_HOURS, :TS_OT_HOURS, :TS_STD_HOURS,
                     :TS_TOTAL_FOOD_ALLOWANCE, :TS_TOTAL_TRAVEL_HOURS, :TS_TOTAL_COST,
                     :TS_REMARKS, 'Y', :TS_CREATION_USER_ID, SYSDATE)";
            await conn.ExecuteAsync(new CommandDefinition(insHeader, header, tx, cancellationToken: ct));


            foreach (var line in lines)
            {
                line.TL_TS_ID = newId;
                await conn.ExecuteAsync(new CommandDefinition(InsertLineSql, line, tx, cancellationToken: ct));
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

    public async Task UpdateTimeSheetAsync(TM_TIME_SHEET header, IReadOnlyList<TM_TIME_LINE> lines, decimal? userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        try
        {
            header.TS_UPDATE_USER_ID = userId;

            const string updHeader = @"
                UPDATE TM_TIME_SHEET SET
                    TS_POSTING_DATE = :TS_POSTING_DATE, TS_JOB_CODE = :TS_JOB_CODE,
                    TS_CUSTOMER_CODE = :TS_CUSTOMER_CODE, TS_CUSTOMER_NAME = :TS_CUSTOMER_NAME,
                    TS_EQUIPMENT = :TS_EQUIPMENT, TS_INDUSTRY_CODE = :TS_INDUSTRY_CODE,
                    TS_LOCATION = :TS_LOCATION, TS_WORK_TYPE_CODE = :TS_WORK_TYPE_CODE, TS_JOB_STATUS = :TS_JOB_STATUS,
                    TS_BRAND = :TS_BRAND, TS_EQUIPMENT_TYPE = :TS_EQUIPMENT_TYPE, TS_SERVICE_TYPE = :TS_SERVICE_TYPE,
                    TS_SERIAL_NO = :TS_SERIAL_NO, TS_JOB_OPENING_DATE = :TS_JOB_OPENING_DATE,
                    TS_TOTAL_NET_HOURS = :TS_TOTAL_NET_HOURS, TS_TOTAL_LABOUR_COST = :TS_TOTAL_LABOUR_COST,
                    TS_NORMAL_HOURS = :TS_NORMAL_HOURS, TS_OT_HOURS = :TS_OT_HOURS, TS_STD_HOURS = :TS_STD_HOURS,
                    TS_TOTAL_FOOD_ALLOWANCE = :TS_TOTAL_FOOD_ALLOWANCE, TS_TOTAL_TRAVEL_HOURS = :TS_TOTAL_TRAVEL_HOURS,
                    TS_TOTAL_COST = :TS_TOTAL_COST,
                    TS_REMARKS = :TS_REMARKS, TS_UPDATE_USER_ID = :TS_UPDATE_USER_ID, TS_UPDATE_DATE = SYSDATE
                WHERE TS_ID = :TS_ID";
            var affected = await conn.ExecuteAsync(new CommandDefinition(updHeader, header, tx, cancellationToken: ct));
            if (affected == 0)
                throw new InvalidOperationException($"Time sheet {header.TS_ID} not found for update.");

            await conn.ExecuteAsync(new CommandDefinition(
                "DELETE FROM TM_TIME_LINE WHERE TL_TS_ID = :id", new { id = header.TS_ID }, tx, cancellationToken: ct));

            foreach (var line in lines)
            {
                line.TL_TS_ID = header.TS_ID;
                await conn.ExecuteAsync(new CommandDefinition(InsertLineSql, line, tx, cancellationToken: ct));
            }

            tx.Commit();
            _logger.LogInformation("Updated time sheet {SheetNo} ({TsId}) — {LineCount} line(s), cost {Cost}",
                header.TS_SHEET_NO, header.TS_ID, lines.Count, header.TS_TOTAL_LABOUR_COST);
        }
        catch (Exception ex)
        {
            tx.Rollback();
            _logger.LogError(ex, "Failed to update time sheet {TsId}", header.TS_ID);
            throw;
        }
    }

    public async Task VoidTimeSheetAsync(decimal tsId, decimal? userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE TM_TIME_SHEET SET TS_ACTIVE_YN = 'N', TS_UPDATE_USER_ID = :userId, TS_UPDATE_DATE = SYSDATE
              WHERE TS_ID = :tsId AND NVL(TS_ACTIVE_YN,'Y') = 'Y'",
            new { tsId, userId }, cancellationToken: ct));
        _logger.LogInformation("Voided time sheet {TsId} by user {UserId}", tsId, userId);
    }
}
