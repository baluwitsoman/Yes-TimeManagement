using System.Data;
using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.Hrms;

public interface IHrmsRepository
{
    Task<decimal> StartLogAsync(string trigger, DateTime from, DateTime to, decimal? userId, CancellationToken ct = default);
    Task FinishLogAsync(decimal id, string status, int employees, int attRows, int newMatches, string? message, CancellationToken ct = default);
    Task<IReadOnlyList<TM_SYNC_LOG>> GetRecentLogsAsync(int count, CancellationToken ct = default);

    Task UpsertEmployeesAsync(IReadOnlyList<HrmsEmployeeDto> employees, CancellationToken ct = default);
    /// <summary>Replaces every attendance row in [from, to] with the HRMS feed for that range.</summary>
    Task<int> ReplaceAttendanceAsync(DateTime from, DateTime to, IReadOnlyList<TM_ATTENDANCE> rows, CancellationToken ct = default);

    /// <summary>TM employees (code, name) — the ones to map.</summary>
    Task<IReadOnlyList<(string Code, string? Name)>> GetTmEmployeesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TM_HRMS_EMPLOYEE>> GetHrmsEmployeesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TM_EMP_CODE_MAP>> GetMapsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<EmpMapRow>> GetMapRowsAsync(CancellationToken ct = default);
    /// <summary>Stores auto-match results without touching rows an admin already confirmed or rejected. Returns rows written.</summary>
    Task<int> SaveSuggestionsAsync(IReadOnlyList<EmpMatch> matches, CancellationToken ct = default);
    Task SaveMapAsync(string tmCode, string? hrmsCode, string matchType, string status, decimal? userId, CancellationToken ct = default);
}

public sealed class HrmsRepository : IHrmsRepository
{
    private readonly IDbConnectionFactory _db;

    public HrmsRepository(IDbConnectionFactory db) => _db = db;

    public async Task<decimal> StartLogAsync(string trigger, DateTime from, DateTime to, decimal? userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var id = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition("SELECT TM_SYNC_LOG_SEQ.NEXTVAL FROM DUAL", cancellationToken: ct));
        await conn.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO TM_SYNC_LOG (SL_ID, SL_TRIGGER, SL_FROM_DATE, SL_TO_DATE, SL_START, SL_STATUS, SL_USER_ID)
            VALUES (:id, :trigger, :fromDate, :toDate, SYSDATE, 'RUNNING', :userId)",
            new { id, trigger, fromDate = from.Date, toDate = to.Date, userId }, cancellationToken: ct));
        return id;
    }

    public async Task FinishLogAsync(decimal id, string status, int employees, int attRows, int newMatches, string? message, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(@"
            UPDATE TM_SYNC_LOG SET SL_END = SYSDATE, SL_STATUS = :status, SL_EMPLOYEES = :employees,
                   SL_ATT_ROWS = :attRows, SL_NEW_MATCHES = :newMatches, SL_MESSAGE = :message
            WHERE SL_ID = :id",
            new { id, status, employees, attRows, newMatches, message = message is { Length: > 2000 } m ? m[..2000] : message },
            cancellationToken: ct));
    }

    public async Task<IReadOnlyList<TM_SYNC_LOG>> GetRecentLogsAsync(int count, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<TM_SYNC_LOG>(new CommandDefinition(@"
            SELECT SL_ID, SL_TRIGGER, SL_FROM_DATE, SL_TO_DATE, SL_START, SL_END, SL_STATUS,
                   SL_EMPLOYEES, SL_ATT_ROWS, SL_NEW_MATCHES, SL_MESSAGE, SL_USER_ID
            FROM   TM_SYNC_LOG ORDER BY SL_ID DESC FETCH FIRST :count ROWS ONLY", new { count }, cancellationToken: ct))).AsList();
    }

    public async Task UpsertEmployeesAsync(IReadOnlyList<HrmsEmployeeDto> employees, CancellationToken ct = default)
    {
        const string merge = @"
            MERGE INTO TM_HRMS_EMPLOYEE t
            USING (SELECT :EMP_CODE AS c FROM DUAL) s ON (t.HE_EMP_CODE = s.c)
            WHEN MATCHED THEN UPDATE SET
                 HE_EMPLOYMENT_NO = :EMPLOYMENT_NO, HE_EMP_NAME = :EMP_NAME, HE_ACTIVE = :ACTIVE,
                 HE_BRANCH_CODE = :BRANCH_CODE, HE_DEPT_NAME = :DEPT_NAME, HE_DESIG_NAME = :DESIG_NAME, HE_SYNC_DATE = SYSDATE
            WHEN NOT MATCHED THEN INSERT
                 (HE_EMP_CODE, HE_EMPLOYMENT_NO, HE_EMP_NAME, HE_ACTIVE, HE_BRANCH_CODE, HE_DEPT_NAME, HE_DESIG_NAME, HE_SYNC_DATE)
                 VALUES (:EMP_CODE, :EMPLOYMENT_NO, :EMP_NAME, :ACTIVE, :BRANCH_CODE, :DEPT_NAME, :DESIG_NAME, SYSDATE)";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(new CommandDefinition(merge,
            employees.Where(e => !string.IsNullOrWhiteSpace(e.EMP_CODE)), tx, cancellationToken: ct));
        tx.Commit();
    }

    public async Task<int> ReplaceAttendanceAsync(DateTime from, DateTime to, IReadOnlyList<TM_ATTENDANCE> rows, CancellationToken ct = default)
    {
        const string insert = @"
            INSERT INTO TM_ATTENDANCE
                (ATT_HRMS_EMP_CODE, ATT_DATE, ATT_DAY_STATUS, ATT_LEAVE_ID, ATT_LEAVE_NAME, ATT_HOLIDAY_NAME,
                 ATT_LEAVE_DAYS, ATT_HOLIDAY_DAYS, ATT_ABSENT_DAYS, ATT_SHIFT_HOURS, ATT_ACTUAL_HOURS, ATT_OT_HOURS,
                 ATT_FIRST_IN, ATT_LAST_OUT, ATT_SYNC_ID)
            VALUES
                (:ATT_HRMS_EMP_CODE, :ATT_DATE, :ATT_DAY_STATUS, :ATT_LEAVE_ID, :ATT_LEAVE_NAME, :ATT_HOLIDAY_NAME,
                 :ATT_LEAVE_DAYS, :ATT_HOLIDAY_DAYS, :ATT_ABSENT_DAYS, :ATT_SHIFT_HOURS, :ATT_ACTUAL_HOURS, :ATT_OT_HOURS,
                 :ATT_FIRST_IN, :ATT_LAST_OUT, :ATT_SYNC_ID)";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM TM_ATTENDANCE WHERE ATT_DATE >= :fromDate AND ATT_DATE < :toExcl",
            new { fromDate = from.Date, toExcl = to.Date.AddDays(1) }, tx, cancellationToken: ct));
        // HRMS can return the same employee-day twice (re-processed rows): keep the last one.
        var distinct = rows.GroupBy(r => (r.ATT_HRMS_EMP_CODE, r.ATT_DATE.Date)).Select(g => g.Last()).ToList();
        await conn.ExecuteAsync(new CommandDefinition(insert, distinct, tx, cancellationToken: ct));
        tx.Commit();
        return distinct.Count;
    }

    public async Task<IReadOnlyList<(string Code, string? Name)>> GetTmEmployeesAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<(string PEMP_EMP_CODE, string? PEMP_EMP_NAME)>(new CommandDefinition(@"
            SELECT PEMP_EMP_CODE, PEMP_EMP_NAME FROM PPM_EMPLOYEE_DETAILS WHERE PEMP_EMP_CODE IS NOT NULL", cancellationToken: ct));
        return rows.Select(r => (r.PEMP_EMP_CODE, r.PEMP_EMP_NAME)).ToList();
    }

    public async Task<IReadOnlyList<TM_HRMS_EMPLOYEE>> GetHrmsEmployeesAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<TM_HRMS_EMPLOYEE>(new CommandDefinition(@"
            SELECT HE_EMP_CODE, HE_EMPLOYMENT_NO, HE_EMP_NAME, HE_ACTIVE, HE_BRANCH_CODE, HE_DEPT_NAME, HE_DESIG_NAME, HE_SYNC_DATE
            FROM TM_HRMS_EMPLOYEE ORDER BY HE_EMP_NAME", cancellationToken: ct))).AsList();
    }

    public async Task<IReadOnlyList<TM_EMP_CODE_MAP>> GetMapsAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<TM_EMP_CODE_MAP>(new CommandDefinition(@"
            SELECT ECM_TM_EMP_CODE, ECM_HRMS_EMP_CODE, ECM_MATCH_TYPE, ECM_CONFIDENCE, ECM_STATUS, ECM_UPDATE_USER_ID, ECM_UPDATE_DATE
            FROM TM_EMP_CODE_MAP", cancellationToken: ct))).AsList();
    }

    public async Task<IReadOnlyList<EmpMapRow>> GetMapRowsAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<EmpMapRow>(new CommandDefinition(@"
            SELECT e.PEMP_EMP_CODE AS TM_EMP_CODE, e.PEMP_EMP_NAME AS TM_EMP_NAME,
                   NVL(b.n, 0) AS BOOKED_LINES,
                   m.ECM_HRMS_EMP_CODE, h.HE_EMP_NAME AS HRMS_EMP_NAME, h.HE_EMPLOYMENT_NO AS HRMS_EMPLOYMENT_NO,
                   m.ECM_MATCH_TYPE, m.ECM_CONFIDENCE, m.ECM_STATUS
            FROM   PPM_EMPLOYEE_DETAILS e
            LEFT JOIN (SELECT TL_TECH_CODE, COUNT(*) n FROM TM_TIME_LINE GROUP BY TL_TECH_CODE) b ON b.TL_TECH_CODE = e.PEMP_EMP_CODE
            LEFT JOIN TM_EMP_CODE_MAP m ON m.ECM_TM_EMP_CODE = e.PEMP_EMP_CODE
            LEFT JOIN TM_HRMS_EMPLOYEE h ON h.HE_EMP_CODE = m.ECM_HRMS_EMP_CODE
            WHERE  e.PEMP_EMP_CODE IS NOT NULL
            ORDER  BY CASE WHEN NVL(b.n,0) > 0 THEN 0 ELSE 1 END, e.PEMP_EMP_NAME", cancellationToken: ct))).AsList();
    }

    public async Task<int> SaveSuggestionsAsync(IReadOnlyList<EmpMatch> matches, CancellationToken ct = default)
    {
        // Only (re)write rows that are missing or still SUGGESTED — admin decisions are never overwritten.
        const string merge = @"
            MERGE INTO TM_EMP_CODE_MAP t
            USING (SELECT :TmCode AS c FROM DUAL) s ON (t.ECM_TM_EMP_CODE = s.c)
            WHEN MATCHED THEN UPDATE SET
                 ECM_HRMS_EMP_CODE = :HrmsCode, ECM_MATCH_TYPE = :MatchType, ECM_CONFIDENCE = :Confidence,
                 ECM_STATUS = :Status, ECM_UPDATE_DATE = SYSDATE
                 WHERE t.ECM_STATUS = 'SUGGESTED'
                   AND (NVL(t.ECM_HRMS_EMP_CODE,'~') <> NVL(:HrmsCode,'~') OR t.ECM_STATUS <> :Status)
            WHEN NOT MATCHED THEN INSERT
                 (ECM_TM_EMP_CODE, ECM_HRMS_EMP_CODE, ECM_MATCH_TYPE, ECM_CONFIDENCE, ECM_STATUS, ECM_UPDATE_DATE)
                 VALUES (:TmCode, :HrmsCode, :MatchType, :Confidence, :Status, SYSDATE)";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        var written = 0;
        foreach (var m in matches)
        {
            written += await conn.ExecuteAsync(new CommandDefinition(merge, new
            {
                m.TmCode, m.HrmsCode, m.MatchType, m.Confidence,
                Status = m.AutoConfirm ? MapStatus.Confirmed : MapStatus.Suggested
            }, tx, cancellationToken: ct));
        }
        tx.Commit();
        return written;
    }

    public async Task SaveMapAsync(string tmCode, string? hrmsCode, string matchType, string status, decimal? userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        using var tx = conn.BeginTransaction();
        // One HRMS employee belongs to one technician: release it from any other confirmed mapping.
        if (!string.IsNullOrWhiteSpace(hrmsCode) && status == MapStatus.Confirmed)
            await conn.ExecuteAsync(new CommandDefinition(@"
                UPDATE TM_EMP_CODE_MAP SET ECM_HRMS_EMP_CODE = NULL, ECM_MATCH_TYPE = 'NONE', ECM_CONFIDENCE = 0,
                       ECM_STATUS = 'SUGGESTED', ECM_UPDATE_USER_ID = :userId, ECM_UPDATE_DATE = SYSDATE
                WHERE  ECM_HRMS_EMP_CODE = :hrmsCode AND ECM_TM_EMP_CODE <> :tmCode",
                new { hrmsCode, tmCode, userId }, tx, cancellationToken: ct));
        await conn.ExecuteAsync(new CommandDefinition(@"
            MERGE INTO TM_EMP_CODE_MAP t
            USING (SELECT :tmCode AS c FROM DUAL) s ON (t.ECM_TM_EMP_CODE = s.c)
            WHEN MATCHED THEN UPDATE SET ECM_HRMS_EMP_CODE = :hrmsCode, ECM_MATCH_TYPE = :matchType,
                 ECM_CONFIDENCE = CASE WHEN :matchType = 'MANUAL' THEN 100 ELSE ECM_CONFIDENCE END,
                 ECM_STATUS = :status, ECM_UPDATE_USER_ID = :userId, ECM_UPDATE_DATE = SYSDATE
            WHEN NOT MATCHED THEN INSERT (ECM_TM_EMP_CODE, ECM_HRMS_EMP_CODE, ECM_MATCH_TYPE, ECM_CONFIDENCE, ECM_STATUS, ECM_UPDATE_USER_ID, ECM_UPDATE_DATE)
                 VALUES (:tmCode, :hrmsCode, :matchType, 100, :status, :userId, SYSDATE)",
            new { tmCode, hrmsCode = string.IsNullOrWhiteSpace(hrmsCode) ? null : hrmsCode, matchType, status, userId },
            tx, cancellationToken: ct));
        tx.Commit();
    }
}
