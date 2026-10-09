using System.Data;
using Dapper;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.FieldService;

public interface IFieldServiceRepository
{
    Task<TM_FIELD_REPORT?> GetReportAsync(decimal tsId, CancellationToken ct = default);
    Task<IReadOnlyList<TM_FIELD_REPORT_CREW>> GetCrewAsync(decimal tsId, CancellationToken ct = default);
    /// <summary>Sheet no of another ACTIVE sheet whose FSR already carries this report no, else null.</summary>
    Task<string?> FindReportNoOwnerAsync(string reportNo, decimal? excludeTsId, CancellationToken ct = default);
    /// <summary>Insert-or-replace the FSR and its crew inside the caller's sheet transaction.</summary>
    Task WriteAsync(IDbConnection conn, IDbTransaction tx, decimal tsId, TM_FIELD_REPORT report,
        IReadOnlyList<TM_FIELD_REPORT_CREW> crew, decimal? userId, CancellationToken ct);
}

public sealed class FieldServiceRepository : IFieldServiceRepository
{
    private readonly IDbConnectionFactory _db;

    public FieldServiceRepository(IDbConnectionFactory db) => _db = db;

    private const string ReportColumns = @"
        FR_TS_ID, FR_REPORT_NO, FR_REPORT_DATE, FR_JOB_SITE, FR_CONTACT_PERSON, FR_CONTACT_POSITION, FR_CONTACT_PHONE,
        FR_OEM, FR_MODEL, FR_SMU_HOURS, FR_UNDER_WARRANTY_YN, FR_TAI_REPORT, FR_JOB_TYPE, FR_FLEET_NO,
        FR_PLATE_NO, FR_DRIVER_NAME, FR_KM_START, FR_KM_END, FR_TOTAL_KM, FR_KM_RATE, FR_VEHICLE_COST,
        FR_COMPLAINT, FR_PROBLEMS_FOUND, FR_PROBLEM_CAUSES, FR_CORRECTION, FR_OTHER_ITEMS,
        FR_JOB_COMPLETED_YN, FR_TO_WORKSHOP_YN, FR_SERVICE_ENGINEER, FR_SERVICE_MANAGER, FR_SIGN_DATE,
        FR_CUST_COMMENTS, FR_CUST_REP, FR_CUST_REP_POSITION, FR_CUST_SIGN_DATE";

    public async Task<TM_FIELD_REPORT?> GetReportAsync(decimal tsId, CancellationToken ct = default)
    {
        var sql = $"SELECT {ReportColumns}, FR_CREATION_USER_ID, FR_CREATION_DATE, FR_UPDATE_USER_ID, FR_UPDATE_DATE " +
                  "FROM TM_FIELD_REPORT WHERE FR_TS_ID = :tsId";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<TM_FIELD_REPORT>(new CommandDefinition(sql, new { tsId }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<TM_FIELD_REPORT_CREW>> GetCrewAsync(decimal tsId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT FRC_TS_ID, FRC_SEQ, FRC_EMP_CODE, FRC_EMP_NAME, FRC_FA_CODE
            FROM   TM_FIELD_REPORT_CREW
            WHERE  FRC_TS_ID = :tsId
            ORDER  BY FRC_SEQ";
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<TM_FIELD_REPORT_CREW>(new CommandDefinition(sql, new { tsId }, cancellationToken: ct))).AsList();
    }

    public async Task<string?> FindReportNoOwnerAsync(string reportNo, decimal? excludeTsId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT s.TS_SHEET_NO
            FROM   TM_FIELD_REPORT r
            JOIN   TM_TIME_SHEET s ON s.TS_ID = r.FR_TS_ID
            WHERE  UPPER(TRIM(r.FR_REPORT_NO)) = UPPER(TRIM(:reportNo))
              AND  NVL(s.TS_ACTIVE_YN,'Y') = 'Y'
              AND  (:excludeTsId IS NULL OR s.TS_ID <> :excludeTsId)
            FETCH FIRST 1 ROWS ONLY";
        var p = new DynamicParameters();
        p.Add("reportNo", reportNo, DbType.String);
        p.Add("excludeTsId", excludeTsId, DbType.Decimal);
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<string?>(new CommandDefinition(sql, p, cancellationToken: ct));
    }

    public async Task WriteAsync(IDbConnection conn, IDbTransaction tx, decimal tsId, TM_FIELD_REPORT report,
        IReadOnlyList<TM_FIELD_REPORT_CREW> crew, decimal? userId, CancellationToken ct)
    {
        report.FR_TS_ID = tsId;
        report.FR_UPDATE_USER_ID = userId;
        report.FR_CREATION_USER_ID ??= userId;

        // Crew rows hang off the report (FK), so they go first; the report row is then upserted.
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM TM_FIELD_REPORT_CREW WHERE FRC_TS_ID = :tsId", new { tsId }, tx, cancellationToken: ct));

        const string update = @"
            UPDATE TM_FIELD_REPORT SET
                FR_REPORT_NO = :FR_REPORT_NO, FR_REPORT_DATE = :FR_REPORT_DATE, FR_JOB_SITE = :FR_JOB_SITE,
                FR_CONTACT_PERSON = :FR_CONTACT_PERSON, FR_CONTACT_POSITION = :FR_CONTACT_POSITION, FR_CONTACT_PHONE = :FR_CONTACT_PHONE,
                FR_OEM = :FR_OEM, FR_MODEL = :FR_MODEL, FR_SMU_HOURS = :FR_SMU_HOURS,
                FR_UNDER_WARRANTY_YN = :FR_UNDER_WARRANTY_YN, FR_TAI_REPORT = :FR_TAI_REPORT,
                FR_JOB_TYPE = :FR_JOB_TYPE, FR_FLEET_NO = :FR_FLEET_NO,
                FR_PLATE_NO = :FR_PLATE_NO, FR_DRIVER_NAME = :FR_DRIVER_NAME,
                FR_KM_START = :FR_KM_START, FR_KM_END = :FR_KM_END, FR_TOTAL_KM = :FR_TOTAL_KM,
                FR_KM_RATE = :FR_KM_RATE, FR_VEHICLE_COST = :FR_VEHICLE_COST,
                FR_COMPLAINT = :FR_COMPLAINT, FR_PROBLEMS_FOUND = :FR_PROBLEMS_FOUND,
                FR_PROBLEM_CAUSES = :FR_PROBLEM_CAUSES, FR_CORRECTION = :FR_CORRECTION, FR_OTHER_ITEMS = :FR_OTHER_ITEMS,
                FR_JOB_COMPLETED_YN = :FR_JOB_COMPLETED_YN, FR_TO_WORKSHOP_YN = :FR_TO_WORKSHOP_YN,
                FR_SERVICE_ENGINEER = :FR_SERVICE_ENGINEER, FR_SERVICE_MANAGER = :FR_SERVICE_MANAGER, FR_SIGN_DATE = :FR_SIGN_DATE,
                FR_CUST_COMMENTS = :FR_CUST_COMMENTS, FR_CUST_REP = :FR_CUST_REP,
                FR_CUST_REP_POSITION = :FR_CUST_REP_POSITION, FR_CUST_SIGN_DATE = :FR_CUST_SIGN_DATE,
                FR_UPDATE_USER_ID = :FR_UPDATE_USER_ID, FR_UPDATE_DATE = SYSDATE
            WHERE FR_TS_ID = :FR_TS_ID";
        var affected = await conn.ExecuteAsync(new CommandDefinition(update, report, tx, cancellationToken: ct));
        if (affected == 0)
        {
            var insert = $@"
                INSERT INTO TM_FIELD_REPORT ({ReportColumns}, FR_CREATION_USER_ID, FR_CREATION_DATE)
                VALUES (:FR_TS_ID, :FR_REPORT_NO, :FR_REPORT_DATE, :FR_JOB_SITE, :FR_CONTACT_PERSON, :FR_CONTACT_POSITION, :FR_CONTACT_PHONE,
                        :FR_OEM, :FR_MODEL, :FR_SMU_HOURS, :FR_UNDER_WARRANTY_YN, :FR_TAI_REPORT, :FR_JOB_TYPE, :FR_FLEET_NO,
                        :FR_PLATE_NO, :FR_DRIVER_NAME, :FR_KM_START, :FR_KM_END, :FR_TOTAL_KM, :FR_KM_RATE, :FR_VEHICLE_COST,
                        :FR_COMPLAINT, :FR_PROBLEMS_FOUND, :FR_PROBLEM_CAUSES, :FR_CORRECTION, :FR_OTHER_ITEMS,
                        :FR_JOB_COMPLETED_YN, :FR_TO_WORKSHOP_YN, :FR_SERVICE_ENGINEER, :FR_SERVICE_MANAGER, :FR_SIGN_DATE,
                        :FR_CUST_COMMENTS, :FR_CUST_REP, :FR_CUST_REP_POSITION, :FR_CUST_SIGN_DATE,
                        :FR_CREATION_USER_ID, SYSDATE)";
            await conn.ExecuteAsync(new CommandDefinition(insert, report, tx, cancellationToken: ct));
        }

        const string insCrew = @"
            INSERT INTO TM_FIELD_REPORT_CREW (FRC_TS_ID, FRC_SEQ, FRC_EMP_CODE, FRC_EMP_NAME, FRC_FA_CODE)
            VALUES (:FRC_TS_ID, :FRC_SEQ, :FRC_EMP_CODE, :FRC_EMP_NAME, :FRC_FA_CODE)";
        foreach (var c in crew)
        {
            c.FRC_TS_ID = tsId;
            await conn.ExecuteAsync(new CommandDefinition(insCrew, c, tx, cancellationToken: ct));
        }
    }
}
