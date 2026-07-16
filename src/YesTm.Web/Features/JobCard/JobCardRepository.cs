using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using YesTm.Web.Common.Data;
using YesTm.Web.Features.Masters;

namespace YesTm.Web.Features.JobCard;

public interface IJobCardRepository
{
    Task<IReadOnlyList<JobCardListItem>> GetListAsync(string? search, int take, CancellationToken ct = default);
    Task<MTL_TRANSACTION_JOB_OTHER_DTLS?> GetAsync(string jobCode, CancellationToken ct = default);
    Task<string> GenerateJobCodeAsync(CancellationToken ct = default);
    Task InsertAsync(MTL_TRANSACTION_JOB_OTHER_DTLS e, decimal userId, CancellationToken ct = default);
    Task UpdateAsync(MTL_TRANSACTION_JOB_OTHER_DTLS e, decimal userId, CancellationToken ct = default);
    Task<DeleteResult> DeleteAsync(string jobCode, CancellationToken ct = default);

    // Lookups
    Task<IReadOnlyList<CodeName>> GetCustomersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetBrandsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetEquipmentTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetServiceTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetJobStatusesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetBranchesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CodeName>> GetSalesmenAsync(CancellationToken ct = default);
}

public sealed class JobCardRepository : IJobCardRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<JobCardRepository> _logger;
    private readonly string _companyCode;

    public JobCardRepository(IDbConnectionFactory db, IConfiguration config, ILogger<JobCardRepository> logger)
    {
        _db = db;
        _logger = logger;
        _companyCode = config["TimeManagement:CompanyCode"] ?? "01";
    }

    public async Task<IReadOnlyList<JobCardListItem>> GetListAsync(string? search, int take, CancellationToken ct = default)
    {
        var sql = @"
            SELECT * FROM (
                SELECT j.MTJ_JOB_CODE, j.MTJ_JOB_DESC, p.MPD_BRANCH_NAME AS CUSTOMER_NAME,
                       b.MCMD_ENTITY_DESC AS BRAND_DESC, j.MTJ_JOB_LOCATION,
                       st.MCMD_ENTITY_DESC AS STATUS_DESC, j.MTJ_JOB_OPENING_DATE
                FROM   MTL_TRANSACTION_JOB_OTHER_DTLS j
                LEFT JOIN MMM_PARTY_DETAILS p
                       ON p.MPD_PARTY_CODE = j.MTJ_PARTY_CODE AND p.MPD_PARTY_TYPE = 'AR'
                LEFT JOIN MMM_COMMON_MASTERS_DETAIL b
                       ON UPPER(b.MCMD_ENTITY_GROUP) = 'AGENCY' AND b.MCMD_ENTITY_CODE = j.MTJ_BRAND
                LEFT JOIN MMM_COMMON_MASTERS_DETAIL st
                       ON UPPER(st.MCMD_ENTITY_GROUP) = 'JOB_CARD_STATUS' AND st.MCMD_ENTITY_CODE = j.MTJ_JOB_STATUS
                WHERE (:search IS NULL
                       OR UPPER(j.MTJ_JOB_CODE) LIKE '%' || UPPER(:search) || '%'
                       OR UPPER(j.MTJ_JOB_DESC) LIKE '%' || UPPER(:search) || '%'
                       OR UPPER(p.MPD_BRANCH_NAME) LIKE '%' || UPPER(:search) || '%')
                ORDER BY j.MTJ_JOB_OPENING_DATE DESC NULLS LAST, j.MTJ_JOB_CODE DESC
            ) WHERE ROWNUM <= :take";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<JobCardListItem>(
            new CommandDefinition(sql, new { search, take }, cancellationToken: ct));
        return rows.AsList();
    }

    public async Task<MTL_TRANSACTION_JOB_OTHER_DTLS?> GetAsync(string jobCode, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT MTJ_COMP_CODE, MTJ_PARTY_IND, MTJ_PARTY_CODE, MTJ_JOB_LOCATION, MTJ_BRAND,
                   MTJ_EQUIPMENT_TYPE_ID, MTJ_SERVICE_TYPE_ID, MTJ_SERIAL_NO, MTJ_JOB_CODE, MTJ_JOB_DESC,
                   MTJ_JOB_OPENING_DATE, MTJ_JOB_SERVICE_START_DATE, MTJ_JOB_CLOSING_DATE, MTJ_JOB_STATUS,
                   MTJ_CONTACT_PERSON, MTJ_CONTACT_PHONE, MTJ_SALES_EMPLOYEE_CODE, MTJ_SALES_EMPLOYEE_PHONE,
                   MTJ_BRANCH, MTJ_SECTOR, MTJ_REMARKS
            FROM   MTL_TRANSACTION_JOB_OTHER_DTLS
            WHERE  MTJ_JOB_CODE = :jobCode AND MTJ_COMP_CODE = :comp
            FETCH FIRST 1 ROWS ONLY";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await conn.QueryFirstOrDefaultAsync<MTL_TRANSACTION_JOB_OTHER_DTLS>(
            new CommandDefinition(sql, new { jobCode, comp = _companyCode }, cancellationToken: ct));
    }

    /// <summary>
    /// Generates the next Job Code using the ERP's own document-numbering procedure
    /// (PRO_GET_NEXT_DOCNO for the JCD document), so numbers stay consistent with the ERP.
    /// </summary>
    public async Task<string> GenerateJobCodeAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);

        var docSysId = await conn.ExecuteScalarAsync<decimal?>(new CommandDefinition(
            "SELECT MDCH_SYS_ID FROM MMM_DOCUMENT_CONTROL_HEADER WHERE MDCH_TRANS_CODE='JCD' AND MDCH_DOC_CODE='JCD'",
            cancellationToken: ct));
        if (docSysId is null)
            throw new InvalidOperationException("JCD document control (MMM_DOCUMENT_CONTROL_HEADER) is not configured.");

        using var cmd = ((OracleConnection)conn).CreateCommand();
        cmd.CommandText = "PRO_GET_NEXT_DOCNO";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("P_DOC_SYS_ID", OracleDbType.Int32) { Value = (int)docSysId.Value });
        cmd.Parameters.Add(new OracleParameter("P_TRN_DT", OracleDbType.Date) { Value = DateTime.Now });
        var outP = new OracleParameter("P_NUMBER", OracleDbType.Varchar2, 100) { Direction = ParameterDirection.Output };
        cmd.Parameters.Add(outP);

        await cmd.ExecuteNonQueryAsync(ct);

        return outP.Value is OracleString os
            ? (os.IsNull ? string.Empty : os.Value)
            : outP.Value?.ToString() ?? string.Empty;
    }

    public async Task InsertAsync(MTL_TRANSACTION_JOB_OTHER_DTLS e, decimal userId, CancellationToken ct = default)
    {
        e.MTJ_COMP_CODE = _companyCode;
        e.MTJ_PARTY_IND ??= "AR";
        e.MTJ_CREATION_USER_ID = userId;

        const string sql = @"
            INSERT INTO MTL_TRANSACTION_JOB_OTHER_DTLS
                (MTJ_COMP_CODE, MTJ_PARTY_IND, MTJ_PARTY_CODE, MTJ_JOB_LOCATION, MTJ_BRAND,
                 MTJ_EQUIPMENT_TYPE_ID, MTJ_SERVICE_TYPE_ID, MTJ_SERIAL_NO, MTJ_JOB_CODE, MTJ_JOB_DESC,
                 MTJ_JOB_OPENING_DATE, MTJ_JOB_SERVICE_START_DATE, MTJ_JOB_CLOSING_DATE, MTJ_JOB_STATUS,
                 MTJ_CONTACT_PERSON, MTJ_CONTACT_PHONE, MTJ_SALES_EMPLOYEE_CODE, MTJ_SALES_EMPLOYEE_PHONE,
                 MTJ_BRANCH, MTJ_SECTOR, MTJ_REMARKS, MTJ_CREATION_USER_ID, MTJ_CREATION_DATE)
            VALUES
                (:MTJ_COMP_CODE, :MTJ_PARTY_IND, :MTJ_PARTY_CODE, :MTJ_JOB_LOCATION, :MTJ_BRAND,
                 :MTJ_EQUIPMENT_TYPE_ID, :MTJ_SERVICE_TYPE_ID, :MTJ_SERIAL_NO, :MTJ_JOB_CODE, :MTJ_JOB_DESC,
                 :MTJ_JOB_OPENING_DATE, :MTJ_JOB_SERVICE_START_DATE, :MTJ_JOB_CLOSING_DATE, :MTJ_JOB_STATUS,
                 :MTJ_CONTACT_PERSON, :MTJ_CONTACT_PHONE, :MTJ_SALES_EMPLOYEE_CODE, :MTJ_SALES_EMPLOYEE_PHONE,
                 :MTJ_BRANCH, :MTJ_SECTOR, :MTJ_REMARKS, :MTJ_CREATION_USER_ID, SYSDATE)";

        using var conn = await _db.CreateOpenConnectionAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(sql, e, cancellationToken: ct));
        _logger.LogInformation("Job card {JobCode} created by {UserId}", e.MTJ_JOB_CODE, userId);
    }

    public async Task UpdateAsync(MTL_TRANSACTION_JOB_OTHER_DTLS e, decimal userId, CancellationToken ct = default)
    {
        e.MTJ_UPDATE_USER_ID = userId;

        const string sql = @"
            UPDATE MTL_TRANSACTION_JOB_OTHER_DTLS SET
                MTJ_PARTY_CODE = :MTJ_PARTY_CODE, MTJ_JOB_LOCATION = :MTJ_JOB_LOCATION, MTJ_BRAND = :MTJ_BRAND,
                MTJ_EQUIPMENT_TYPE_ID = :MTJ_EQUIPMENT_TYPE_ID, MTJ_SERVICE_TYPE_ID = :MTJ_SERVICE_TYPE_ID,
                MTJ_SERIAL_NO = :MTJ_SERIAL_NO, MTJ_JOB_DESC = :MTJ_JOB_DESC,
                MTJ_JOB_OPENING_DATE = :MTJ_JOB_OPENING_DATE, MTJ_JOB_SERVICE_START_DATE = :MTJ_JOB_SERVICE_START_DATE,
                MTJ_JOB_CLOSING_DATE = :MTJ_JOB_CLOSING_DATE, MTJ_JOB_STATUS = :MTJ_JOB_STATUS,
                MTJ_CONTACT_PERSON = :MTJ_CONTACT_PERSON, MTJ_CONTACT_PHONE = :MTJ_CONTACT_PHONE,
                MTJ_SALES_EMPLOYEE_CODE = :MTJ_SALES_EMPLOYEE_CODE, MTJ_SALES_EMPLOYEE_PHONE = :MTJ_SALES_EMPLOYEE_PHONE,
                MTJ_BRANCH = :MTJ_BRANCH, MTJ_SECTOR = :MTJ_SECTOR, MTJ_REMARKS = :MTJ_REMARKS,
                MTJ_UPDATE_USER_ID = :MTJ_UPDATE_USER_ID, MTJ_UPDATE_DATE = SYSDATE
            WHERE MTJ_JOB_CODE = :MTJ_JOB_CODE AND MTJ_COMP_CODE = :MTJ_COMP_CODE";

        e.MTJ_COMP_CODE = _companyCode;
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(sql, e, cancellationToken: ct));
        _logger.LogInformation("Job card {JobCode} updated by {UserId}", e.MTJ_JOB_CODE, userId);
    }

    public async Task<DeleteResult> DeleteAsync(string jobCode, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);

        // Guard: don't delete a job that already has labour bookings or ERP transactions.
        var timeSheets = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM TM_TIME_SHEET WHERE TS_JOB_CODE = :jobCode", new { jobCode }, cancellationToken: ct));
        var erpTxns = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM MTL_TRANSACTIONS_HEADER WHERE MTH_NARR5 = :jobCode", new { jobCode }, cancellationToken: ct));
        if (timeSheets > 0 || erpTxns > 0)
            return DeleteResult.HasDependencies;

        var rows = await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM MTL_TRANSACTION_JOB_OTHER_DTLS WHERE MTJ_JOB_CODE = :jobCode AND MTJ_COMP_CODE = :comp",
            new { jobCode, comp = _companyCode }, cancellationToken: ct));

        if (rows == 0) return DeleteResult.NotFound;
        _logger.LogInformation("Job card {JobCode} deleted", jobCode);
        return DeleteResult.Deleted;
    }

    // ---------------- Lookups ----------------
    public async Task<IReadOnlyList<CodeName>> GetCustomersAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT MPD_PARTY_CODE AS CODE, MPD_BRANCH_NAME AS NAME FROM MMM_PARTY_DETAILS
                            WHERE MPD_PARTY_TYPE='AR' AND MPD_PARTY_CODE IS NOT NULL
                            ORDER BY MPD_BRANCH_NAME FETCH FIRST 2000 ROWS ONLY", ct);

    public async Task<IReadOnlyList<CodeName>> GetBrandsAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT MCMD_ENTITY_CODE AS CODE, MCMD_ENTITY_DESC AS NAME FROM MMM_COMMON_MASTERS_DETAIL
                            WHERE UPPER(MCMD_ENTITY_GROUP)='AGENCY' ORDER BY MCMD_ENTITY_DESC", ct);

    public async Task<IReadOnlyList<CodeName>> GetEquipmentTypesAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT TO_CHAR(MET_EQUIPMENT_TYPE_ID) AS CODE, MET_EQUIPMENT_TYPE_DESC AS NAME
                            FROM MMM_EQUIPMENT_TYPE ORDER BY MET_SORT_ORDER NULLS LAST, MET_EQUIPMENT_TYPE_DESC", ct);

    public async Task<IReadOnlyList<CodeName>> GetServiceTypesAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT TO_CHAR(MST_SERVICE_TYPE_ID) AS CODE, MST_SERVICE_TYPE_DESC AS NAME
                            FROM MMM_SERVICE_TYPE ORDER BY MST_SORT_ORDER NULLS LAST, MST_SERVICE_TYPE_DESC", ct);

    public async Task<IReadOnlyList<CodeName>> GetJobStatusesAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT MCMD_ENTITY_CODE AS CODE, MCMD_ENTITY_DESC AS NAME FROM MMM_COMMON_MASTERS_DETAIL
                            WHERE UPPER(MCMD_ENTITY_GROUP)='JOB_CARD_STATUS' ORDER BY MCMD_ENTITY_DESC", ct);

    public async Task<IReadOnlyList<CodeName>> GetBranchesAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT PBM_BRANCH_CODE AS CODE, PBM_BRANCH_NAME AS NAME FROM PPM_BRANCH_MASTER
                            ORDER BY PBM_SORT_ORDER NULLS LAST, PBM_BRANCH_NAME", ct);

    public async Task<IReadOnlyList<CodeName>> GetSalesmenAsync(CancellationToken ct = default) =>
        await QueryLookup(@"SELECT PEMP_EMP_CODE AS CODE, PEMP_EMP_NAME AS NAME FROM V_EMPLOYEE_DETAILS
                            WHERE PEMP_EMP_ACTIVE='Y' ORDER BY PEMP_EMP_NAME FETCH FIRST 2000 ROWS ONLY", ct);

    private async Task<IReadOnlyList<CodeName>> QueryLookup(string sql, CancellationToken ct)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<CodeName>(new CommandDefinition(sql, cancellationToken: ct))).AsList();
    }
}
