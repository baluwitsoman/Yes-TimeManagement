using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using YesTm.Web.Common.Data;

namespace YesTm.Web.Features.JobCostPosting;

/// <summary>
/// Job Cost Posting: keeps each time sheet's GL journal vouchers (FINT_TRANSACTION_HEADER/DETAIL in the
/// Yes ERP schema) equal to what the sheet's hours cost. Called inside the sheet's own save / void
/// transaction, so the sheet and its JVs always commit — or roll back — together.
/// <para>
/// Writes follow the ERP's own posting path (PRO_POST_MATERIAL_ISSUE_TO_FIN): header inserted unposted,
/// details inserted, then FITH_STATUS flipped to 'P' (the header trigger runs the GL post). A changed
/// sheet re-uses the same voucher: unpost → replace the lines → post again, as the ERP unpost screen does.
/// </para>
/// </summary>
public interface IJobCostPostingService
{
    /// <summary>Messages from the last sync in this request (uncosted hours, vouchers created/updated) for the screen.</summary>
    IReadOnlyList<string> LastMessages { get; }

    /// <summary>Bring the sheet's vouchers in line with its lines. Throws <see cref="JobCostPostingException"/> when that is not allowed.</summary>
    Task SyncSheetAsync(IDbConnection conn, IDbTransaction tx, decimal tsId, decimal? userId, CancellationToken ct);
    /// <summary>Delete every voucher the sheet owns (sheet voided).</summary>
    Task RemoveSheetAsync(IDbConnection conn, IDbTransaction tx, decimal tsId, decimal? userId, CancellationToken ct);

    Task<SheetJvView> GetSheetJvAsync(decimal tsId, CancellationToken ct = default);

    // ---- setup ----
    Task<bool> IsSchemaReadyAsync(CancellationToken ct = default);
    Task<TM_JCP_SETUP> GetSetupAsync(CancellationToken ct = default);
    Task SaveSetupAsync(TM_JCP_SETUP setup, decimal userId, CancellationToken ct = default);
    Task<IReadOnlyList<TM_JCP_COST_CODE>> GetCostCodesAsync(CancellationToken ct = default);
    Task SaveCostCodeAsync(TM_JCP_COST_CODE code, decimal userId, CancellationToken ct = default);
    Task SetCostCodeActiveAsync(decimal id, bool active, decimal userId, CancellationToken ct = default);
    Task<IReadOnlyList<GlAccountLookup>> GetAccountsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetJournalTypesAsync(CancellationToken ct = default);
}

public sealed class JobCostPostingService : IJobCostPostingService
{
    private const string Orgn = "GL";
    private const string SourceTag = "TM-JCP";   // FITH_AUTHORIZED_BY5 marks vouchers this app owns

    private readonly IDbConnectionFactory _db;
    private readonly ILogger<JobCostPostingService> _logger;
    private readonly List<string> _messages = [];

    public JobCostPostingService(IDbConnectionFactory db, ILogger<JobCostPostingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public IReadOnlyList<string> LastMessages => _messages;

    // =====================================================================================
    // Sync
    // =====================================================================================

    public async Task SyncSheetAsync(IDbConnection conn, IDbTransaction tx, decimal tsId, decimal? userId, CancellationToken ct)
    {
        _messages.Clear();
        if (!await SchemaInfo.HasColumnAsync(conn, "TM_JCP_SETUP", "JCS_ID", ct)) return;   // db/12 not run
        var setup = await LoadSetupAsync(conn, tx, ct);
        if (!setup.Enabled) return;

        var sheet = await conn.QueryFirstOrDefaultAsync<SheetRow>(new CommandDefinition(@"
            SELECT TS_ID, TS_SHEET_NO, TS_JOB_CODE, TS_LOCATION, NVL(TS_SHEET_TYPE,'STANDARD') AS TS_SHEET_TYPE,
                   NVL(TS_ACTIVE_YN,'Y') AS TS_ACTIVE_YN
            FROM   TM_TIME_SHEET WHERE TS_ID = :tsId", new { tsId }, tx, cancellationToken: ct));
        if (sheet is null) return;

        var calc = await CalculateAsync(conn, tx, sheet, setup, ct);
        await ApplyAsync(conn, tx, sheet, setup, calc.Vouchers, userId, ct);
        _messages.AddRange(calc.Warnings);
    }

    public async Task RemoveSheetAsync(IDbConnection conn, IDbTransaction tx, decimal tsId, decimal? userId, CancellationToken ct)
    {
        _messages.Clear();
        if (!await SchemaInfo.HasColumnAsync(conn, "TM_JCP_POSTING", "JCP_TS_ID", ct)) return;
        var setup = await LoadSetupAsync(conn, tx, ct);
        var sheet = await conn.QueryFirstOrDefaultAsync<SheetRow>(new CommandDefinition(
            "SELECT TS_ID, TS_SHEET_NO, TS_JOB_CODE FROM TM_TIME_SHEET WHERE TS_ID = :tsId", new { tsId }, tx, cancellationToken: ct))
            ?? new SheetRow { TS_ID = tsId };
        // Removal runs even when posting is switched off — a voided sheet must not leave cost on the job.
        await ApplyAsync(conn, tx, sheet, setup, [], userId, ct);
    }

    private static async Task<JobCostCalcResult> CalculateAsync(IDbConnection conn, IDbTransaction? tx, SheetRow sheet,
        TM_JCP_SETUP setup, CancellationToken ct)
    {
        if (sheet.TS_SHEET_TYPE == "NONJOB" || sheet.TS_ACTIVE_YN != "Y" || string.IsNullOrWhiteSpace(sheet.TS_JOB_CODE))
            return new JobCostCalcResult();

        var lines = (await conn.QueryAsync<CostLineRow>(new CommandDefinition(@"
            SELECT TL_TECH_CODE, TL_TECH_NAME, NVL(TL_WORK_DATE, TRUNC(TL_START_DT)) AS TL_WORK_DATE, TL_LOCATION,
                   NVL(TL_SEGMENT_TYPE,'WORK') AS TL_SEGMENT_TYPE, NVL(TL_NET_HOURS,0) AS TL_NET_HOURS,
                   NVL(TL_IDLE_HOURS,0) AS TL_IDLE_HOURS, NVL(TL_TRAVEL_HOURS,0) AS TL_TRAVEL_HOURS
            FROM   TM_TIME_LINE WHERE TL_TS_ID = :tsId ORDER BY TL_LINE_NO", new { tsId = sheet.TS_ID }, tx, cancellationToken: ct)))
            .Select(r => new CostLineInput(r.TL_TECH_CODE, r.TL_TECH_NAME, r.TL_WORK_DATE, r.TL_LOCATION,
                r.TL_SEGMENT_TYPE, r.TL_NET_HOURS, r.TL_IDLE_HOURS, r.TL_TRAVEL_HOURS)).ToList();

        CostKmInput? km = null;
        if (sheet.TS_SHEET_TYPE == "FIELD")
        {
            var fr = await conn.QueryFirstOrDefaultAsync<(decimal? FR_TOTAL_KM, DateTime? FR_REPORT_DATE, string? FR_REPORT_NO, string? FR_PLATE_NO)>(
                new CommandDefinition("SELECT FR_TOTAL_KM, FR_REPORT_DATE, FR_REPORT_NO, FR_PLATE_NO FROM TM_FIELD_REPORT WHERE FR_TS_ID = :tsId",
                    new { tsId = sheet.TS_ID }, tx, cancellationToken: ct));
            if (fr.FR_TOTAL_KM is > 0) km = new CostKmInput(fr.FR_TOTAL_KM.Value, fr.FR_REPORT_DATE, fr.FR_REPORT_NO, fr.FR_PLATE_NO);
        }

        var codes = (await conn.QueryAsync<TM_JCP_COST_CODE>(new CommandDefinition(
            "SELECT JCC_ID, JCC_TYPE, JCC_LOCATION, JCC_DESC, JCC_RATE, JCC_DR_VEL_CODE, JCC_CR_VEL_CODE, JCC_EFFECTIVE_FROM, NVL(ACTIVE_YN,'Y') AS ACTIVE_YN FROM TM_JCP_COST_CODE", transaction: tx, cancellationToken: ct))).AsList();
        return JobCostCalculator.Calculate(sheet.TS_SHEET_NO ?? sheet.TS_ID.ToString(), sheet.TS_JOB_CODE!, sheet.TS_LOCATION,
            lines, km, codes, setup.JCS_START_DATE);
    }

    private async Task ApplyAsync(IDbConnection conn, IDbTransaction tx, SheetRow sheet, TM_JCP_SETUP setup,
        IReadOnlyList<JvDraft> drafts, decimal? userId, CancellationToken ct)
    {
        var existing = (await conn.QueryAsync<TM_JCP_POSTING>(new CommandDefinition(
            "SELECT * FROM TM_JCP_POSTING WHERE JCP_TS_ID = :tsId", new { tsId = sheet.TS_ID }, tx, cancellationToken: ct))).AsList();
        var user = userId ?? 0m;
        var keep = new HashSet<decimal>();

        foreach (var draft in drafts)
        {
            var period = await PeriodForAsync(conn, tx, setup.JCS_COMP_CODE, draft.JvDate, ct)
                ?? throw new JobCostPostingException($"No finance period covers {draft.JvDate:dd-MMM-yyyy} (ERP calendar).");
            keep.Add(period.FCD_PERIOD_SYS_ID);
            var post = existing.FirstOrDefault(p => p.JCP_PERIOD_SYS_ID == period.FCD_PERIOD_SYS_ID);
            var header = post is null ? null : await VoucherHeaderAsync(conn, tx, post.JCP_FITH_SYS_ID, ct);

            if (header is not null && header.FITH_DT.Date == draft.JvDate
                && SameLines(await VoucherLinesAsync(conn, tx, header.FITH_SYS_ID, ct), draft.Lines))
                continue;   // nothing changed — leave the posted voucher alone

            RequireOpen(period, draft.JvDate);
            var narration = $"Job cost (TM) {sheet.TS_SHEET_NO} · job {sheet.TS_JOB_CODE} · {draft.Month:MMM-yyyy}";
            if (header is not null)
            {
                await RewriteVoucherAsync(conn, tx, header, draft, period, narration, sheet.TS_SHEET_NO, user, ct);
                await conn.ExecuteAsync(new CommandDefinition(@"
                    UPDATE TM_JCP_POSTING SET JCP_FITH_DT = :dt, JCP_AMOUNT = :amt, UPDATE_USER_ID = :u, UPDATE_DATE = SYSDATE
                    WHERE JCP_TS_ID = :ts AND JCP_PERIOD_SYS_ID = :pid",
                    new { dt = draft.JvDate, amt = draft.Total, u = user, ts = sheet.TS_ID, pid = period.FCD_PERIOD_SYS_ID }, tx, cancellationToken: ct));
                _messages.Add($"Finance JV {header.FITH_SRL_NO} updated ({draft.Total:N3} OMR).");
            }
            else
            {
                var (sysId, srlNo) = await CreateVoucherAsync(conn, tx, setup, draft, period, narration, sheet.TS_SHEET_NO, user, ct);
                if (post is not null)   // finance deleted the old voucher in the ERP — point at the new one
                    await conn.ExecuteAsync(new CommandDefinition(
                        "DELETE FROM TM_JCP_POSTING WHERE JCP_TS_ID = :ts AND JCP_PERIOD_SYS_ID = :pid",
                        new { ts = sheet.TS_ID, pid = period.FCD_PERIOD_SYS_ID }, tx, cancellationToken: ct));
                await conn.ExecuteAsync(new CommandDefinition(@"
                    INSERT INTO TM_JCP_POSTING (JCP_TS_ID, JCP_PERIOD_SYS_ID, JCP_FITH_SYS_ID, JCP_FITH_SRL_NO, JCP_FITH_DT, JCP_AMOUNT,
                                                CREATION_USER_ID, CREATION_DATE)
                    VALUES (:ts, :pid, :sys, :srl, :dt, :amt, :u, SYSDATE)",
                    new { ts = sheet.TS_ID, pid = period.FCD_PERIOD_SYS_ID, sys = sysId, srl = srlNo, dt = draft.JvDate, amt = draft.Total, u = user },
                    tx, cancellationToken: ct));
                _messages.Add($"Finance JV {srlNo} posted ({draft.Total:N3} OMR).");
            }
        }

        // Months that no longer have any cost on this sheet: their vouchers go.
        foreach (var post in existing.Where(p => !keep.Contains(p.JCP_PERIOD_SYS_ID)))
        {
            var header = await VoucherHeaderAsync(conn, tx, post.JCP_FITH_SYS_ID, ct);
            if (header is not null)
            {
                var period = await conn.QueryFirstOrDefaultAsync<PeriodRow>(new CommandDefinition(
                    "SELECT FCD_PERIOD_SYS_ID, TO_NUMBER(FCD_YEAR_ID) AS FCD_YEAR_ID, FCD_GL_STATUS FROM FINM_CALENDAR_DETAILS WHERE FCD_PERIOD_SYS_ID = :pid",
                    new { pid = post.JCP_PERIOD_SYS_ID }, tx, cancellationToken: ct));
                if (period is not null) RequireOpen(period, header.FITH_DT);
                await DeleteVoucherAsync(conn, tx, header, user, ct);
                _messages.Add($"Finance JV {header.FITH_SRL_NO} deleted.");
            }
            await conn.ExecuteAsync(new CommandDefinition(
                "DELETE FROM TM_JCP_POSTING WHERE JCP_TS_ID = :ts AND JCP_PERIOD_SYS_ID = :pid",
                new { ts = post.JCP_TS_ID, pid = post.JCP_PERIOD_SYS_ID }, tx, cancellationToken: ct));
        }
    }

    private static void RequireOpen(PeriodRow period, DateTime date)
    {
        if (!string.Equals(period.FCD_GL_STATUS, "O", StringComparison.OrdinalIgnoreCase))
            throw new JobCostPostingException(
                $"The GL period for {date:MMM-yyyy} is closed in finance, so its job cost voucher cannot change. " +
                "Ask finance to reopen the period, or leave that month's hours as they are.");
    }

    private static bool SameLines(IReadOnlyList<SheetJvLine> current, IReadOnlyList<JvLine> wanted) =>
        current.Count == wanted.Count && current.OrderBy(l => l.FITD_SRL_NO).Zip(wanted).All(p =>
            p.First.FITD_VEL_CODE == p.Second.VelCode && p.First.FITD_BC_AMOUNT == p.Second.Amount
            && string.Equals(p.First.FITD_TA_CODE1 ?? "", Ta(p.Second.JobCode) ?? "", StringComparison.Ordinal)
            && string.Equals(p.First.FITD_JRNL_LINE_NARRATION ?? "", Narr(p.Second.Narration), StringComparison.Ordinal));

    // ---------------------------------------------------------------- ERP voucher writes

    private async Task<(decimal SysId, string SrlNo)> CreateVoucherAsync(IDbConnection conn, IDbTransaction tx, TM_JCP_SETUP setup,
        JvDraft draft, PeriodRow period, string narration, string? reference, decimal user, CancellationToken ct)
    {
        var ora = (OracleConnection)conn;
        // Serialise numbering with other users of this series (the ERP procedure reads then bumps FJDC_NEXT_NO).
        var ctrl = await conn.ExecuteScalarAsync<int>(new CommandDefinition(@"
            SELECT COUNT(*) FROM (SELECT 1 FROM FINM_JOURNAL_DOCUMENT_CTRL
                                  WHERE FJDC_JRNL_ORGN = :o AND FJDC_JRNL_TYPE = :t AND FJDC_BRANCH_CODE = :b
                                    AND FJDC_PERIOD_SYS_ID = :pid FOR UPDATE)",
            new { o = Orgn, t = setup.JCS_JRNL_TYPE, b = setup.JCS_OFF_CODE, pid = period.FCD_PERIOD_SYS_ID }, tx, cancellationToken: ct));
        if (ctrl == 0)
            throw new JobCostPostingException(
                $"Voucher numbering for {Orgn}/{setup.JCS_JRNL_TYPE}, branch {setup.JCS_OFF_CODE}, {draft.JvDate:MMM-yyyy} is not set up in the ERP (Journal Document Control).");

        string srlNo;
        using (var cmd = ora.CreateCommand())
        {
            cmd.Transaction = (OracleTransaction)tx;
            cmd.CommandText = "PRO_FIN_GEN_DOC_NO";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;
            cmd.Parameters.Add(new OracleParameter("P_JRNL_ORGN", OracleDbType.Varchar2) { Value = Orgn });
            cmd.Parameters.Add(new OracleParameter("P_JRNL_TYPE", OracleDbType.Varchar2) { Value = setup.JCS_JRNL_TYPE });
            cmd.Parameters.Add(new OracleParameter("P_BRANCH_CODE", OracleDbType.Varchar2) { Value = setup.JCS_OFF_CODE });
            cmd.Parameters.Add(new OracleParameter("P_YEAR", OracleDbType.Decimal) { Value = period.FCD_YEAR_ID });
            cmd.Parameters.Add(new OracleParameter("P_UPDATE_YN", OracleDbType.Varchar2) { Value = "Y" });
            var outP = new OracleParameter("P_DOC_NO", OracleDbType.Varchar2, 30) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outP);
            cmd.Parameters.Add(new OracleParameter("P_PERIOD_ID", OracleDbType.Decimal) { Value = period.FCD_PERIOD_SYS_ID });
            await cmd.ExecuteNonQueryAsync(ct);
            srlNo = outP.Value is OracleString os && !os.IsNull ? os.Value : outP.Value?.ToString() ?? string.Empty;
        }
        if (string.IsNullOrWhiteSpace(srlNo))
            throw new JobCostPostingException("The ERP did not return a voucher number (PRO_FIN_GEN_DOC_NO).");

        var sysId = await conn.ExecuteScalarAsync<decimal>(new CommandDefinition(
            "SELECT SEQ_GL_SYS_ID.NEXTVAL FROM DUAL", transaction: tx, cancellationToken: ct));
        await conn.ExecuteAsync(new CommandDefinition(@"
            INSERT INTO FINT_TRANSACTION_HEADER
                (FITH_COMP_CODE, FITH_OFF_CODE, FITH_JRNL_ORGN, FITH_JRNL_TYPE, FITH_SRL_NO, FITH_DT, FITH_SYS_ID,
                 FITH_PERIOD_SYS_ID, FITH_NARRATION, FITH_REFERENCE, FITH_STATUS, FITH_SOURCE_JRNL_ORGN,
                 FITH_AUTHORIZED_BY5, FITH_CREATION_USER_ID, FITH_CREATION_DATE)
            VALUES (:comp, :off, :orgn, :jtype, :srl, :dt, :sys, :pid, :narr, :refNo, 'U', 'TM', :tag, :u, SYSDATE)",
            new
            {
                comp = setup.JCS_COMP_CODE, off = setup.JCS_OFF_CODE, orgn = Orgn, jtype = setup.JCS_JRNL_TYPE, srl = srlNo,
                dt = draft.JvDate, sys = sysId, pid = period.FCD_PERIOD_SYS_ID, narr = Narr(narration), refNo = reference,
                tag = SourceTag, u = user
            }, tx, cancellationToken: ct));
        await InsertLinesAsync(conn, tx, sysId, setup.JCS_COMP_CODE, draft.Lines, user, ct);
        await SetStatusAsync(conn, tx, sysId, "P", user, ct);
        _logger.LogInformation("Job cost JV {SrlNo} ({SysId}) created for {Ref}, {Amount}", srlNo, sysId, reference, draft.Total);
        return (sysId, srlNo);
    }

    private async Task RewriteVoucherAsync(IDbConnection conn, IDbTransaction tx, VoucherHeaderRow header, JvDraft draft,
        PeriodRow period, string narration, string? reference, decimal user, CancellationToken ct)
    {
        // Unpost → replace lines → post again (same voucher no).
        if (header.FITH_STATUS == "P") await SetStatusAsync(conn, tx, header.FITH_SYS_ID, "U", user, ct);
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM FINT_TRANSACTION_DETAIL WHERE FITD_TH_SYS_ID = :sys", new { sys = header.FITH_SYS_ID }, tx, cancellationToken: ct));
        await conn.ExecuteAsync(new CommandDefinition(@"
            UPDATE FINT_TRANSACTION_HEADER
               SET FITH_DT = :dt, FITH_PERIOD_SYS_ID = :pid, FITH_NARRATION = :narr, FITH_REFERENCE = :refNo,
                   FITH_UPDATE_USER_ID = :u, FITH_UPDATE_DATE = SYSDATE
             WHERE FITH_SYS_ID = :sys",
            new { dt = draft.JvDate, pid = period.FCD_PERIOD_SYS_ID, narr = Narr(narration), refNo = reference, u = user, sys = header.FITH_SYS_ID },
            tx, cancellationToken: ct));
        await InsertLinesAsync(conn, tx, header.FITH_SYS_ID, header.FITH_COMP_CODE ?? "01", draft.Lines, user, ct);
        await SetStatusAsync(conn, tx, header.FITH_SYS_ID, "P", user, ct);
        _logger.LogInformation("Job cost JV {SrlNo} ({SysId}) rewritten for {Ref}, {Amount}", header.FITH_SRL_NO, header.FITH_SYS_ID, reference, draft.Total);
    }

    private async Task DeleteVoucherAsync(IDbConnection conn, IDbTransaction tx, VoucherHeaderRow header, decimal user, CancellationToken ct)
    {
        if (header.FITH_STATUS == "P") await SetStatusAsync(conn, tx, header.FITH_SYS_ID, "U", user, ct);
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM FINT_TRANSACTION_DETAIL WHERE FITD_TH_SYS_ID = :sys", new { sys = header.FITH_SYS_ID }, tx, cancellationToken: ct));
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM FINT_TRANSACTION_HEADER WHERE FITH_SYS_ID = :sys", new { sys = header.FITH_SYS_ID }, tx, cancellationToken: ct));
        _logger.LogInformation("Job cost JV {SrlNo} ({SysId}) deleted", header.FITH_SRL_NO, header.FITH_SYS_ID);
    }

    private static async Task InsertLinesAsync(IDbConnection conn, IDbTransaction tx, decimal sysId, string comp,
        IReadOnlyList<JvLine> lines, decimal user, CancellationToken ct)
    {
        var curr = await conn.ExecuteScalarAsync<string?>(new CommandDefinition(
            "SELECT ACD_FUNCTIONAL_CURR_CODE FROM AMM_COMPANY_DETAILS WHERE ACD_COMP_CODE = :comp", new { comp }, tx, cancellationToken: ct)) ?? "OMR";
        var srl = 0;
        foreach (var l in lines)
        {
            srl++;
            await conn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO FINT_TRANSACTION_DETAIL
                    (FITD_TH_SYS_ID, FITD_SRL_NO, FITD_VEL_CODE, FITD_CURR_CODE, FITD_EXCHG_FLAG, FITD_FC_AMOUNT, FITD_BC_AMOUNT,
                     FITD_JRNL_LINE_NARRATION, FITD_CURR_RATE, FITD_CREATION_USER_ID, FITD_CREATION_DATE, FITD_TA_CODE1)
                VALUES (:sys, :srl, :vel, :curr, 'N', :amt, :amt, :narr, 1, :u, SYSDATE, :ta)",
                new { sys = sysId, srl, vel = l.VelCode, curr, amt = l.Amount, narr = Narr(l.Narration), u = user, ta = Ta(l.JobCode) },
                tx, cancellationToken: ct));
        }
    }

    private static Task SetStatusAsync(IDbConnection conn, IDbTransaction tx, decimal sysId, string status, decimal user, CancellationToken ct) =>
        conn.ExecuteAsync(new CommandDefinition(@"
            UPDATE FINT_TRANSACTION_HEADER SET FITH_STATUS = :st, FITH_UPDATE_USER_ID = :u, FITH_UPDATE_DATE = SYSDATE
            WHERE FITH_SYS_ID = :sys", new { st = status, u = user, sys = sysId }, tx, cancellationToken: ct));

    private static string Narr(string s) => s.Length > 2000 ? s[..2000] : s;
    private static string? Ta(string? job) => job is { Length: > 20 } ? job[..20] : job;

    // ---------------------------------------------------------------- ERP reads

    private static Task<PeriodRow?> PeriodForAsync(IDbConnection conn, IDbTransaction? tx, string comp, DateTime date, CancellationToken ct) =>
        conn.QueryFirstOrDefaultAsync<PeriodRow>(new CommandDefinition(@"
            SELECT FCD_PERIOD_SYS_ID, TO_NUMBER(FCD_YEAR_ID) AS FCD_YEAR_ID, FCD_GL_STATUS
            FROM   FINM_CALENDAR_DETAILS
            WHERE  :dt BETWEEN FCD_PERIOD_FROM AND FCD_PERIOD_TO
            ORDER  BY FCD_PERIOD_SYS_ID FETCH FIRST 1 ROWS ONLY", new { dt = date.Date }, tx, cancellationToken: ct));

    private static Task<VoucherHeaderRow?> VoucherHeaderAsync(IDbConnection conn, IDbTransaction? tx, decimal sysId, CancellationToken ct) =>
        conn.QueryFirstOrDefaultAsync<VoucherHeaderRow>(new CommandDefinition(@"
            SELECT FITH_SYS_ID, FITH_SRL_NO, FITH_DT, FITH_STATUS, FITH_COMP_CODE, FITH_JRNL_TYPE, FITH_NARRATION
            FROM   FINT_TRANSACTION_HEADER WHERE FITH_SYS_ID = :sysId", new { sysId }, tx, cancellationToken: ct));

    private static async Task<IReadOnlyList<SheetJvLine>> VoucherLinesAsync(IDbConnection conn, IDbTransaction? tx, decimal sysId, CancellationToken ct) =>
        (await conn.QueryAsync<SheetJvLine>(new CommandDefinition(@"
            SELECT d.FITD_SRL_NO, d.FITD_VEL_CODE, a.FNAM_FNAM_CODE, a.FNAM_DESC, d.FITD_TA_CODE1,
                   d.FITD_JRNL_LINE_NARRATION, d.FITD_BC_AMOUNT
            FROM   FINT_TRANSACTION_DETAIL d
            LEFT JOIN FINM_ACCOUNT_MASTER a ON a.FNAM_VEL_CODE = d.FITD_VEL_CODE
            WHERE  d.FITD_TH_SYS_ID = :sysId ORDER BY d.FITD_SRL_NO", new { sysId }, tx, cancellationToken: ct))).AsList();

    private static async Task<TM_JCP_SETUP> LoadSetupAsync(IDbConnection conn, IDbTransaction? tx, CancellationToken ct) =>
        await conn.QueryFirstOrDefaultAsync<TM_JCP_SETUP>(new CommandDefinition(
            "SELECT * FROM TM_JCP_SETUP WHERE JCS_ID = 1", transaction: tx, cancellationToken: ct)) ?? new TM_JCP_SETUP();

    // =====================================================================================
    // View
    // =====================================================================================

    public async Task<SheetJvView> GetSheetJvAsync(decimal tsId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var view = new SheetJvView { SchemaReady = await SchemaInfo.HasColumnAsync(conn, "TM_JCP_POSTING", "JCP_TS_ID", ct) };
        var sheet = await conn.QueryFirstOrDefaultAsync<SheetRow>(new CommandDefinition(@"
            SELECT TS_ID, TS_SHEET_NO, TS_JOB_CODE, TS_LOCATION, NVL(TS_SHEET_TYPE,'STANDARD') AS TS_SHEET_TYPE,
                   NVL(TS_ACTIVE_YN,'Y') AS TS_ACTIVE_YN
            FROM   TM_TIME_SHEET WHERE TS_ID = :tsId", new { tsId }, cancellationToken: ct));
        view.SheetNo = sheet?.TS_SHEET_NO;
        view.JobCode = sheet?.TS_JOB_CODE;
        if (!view.SchemaReady || sheet is null) return view;

        var setup = await LoadSetupAsync(conn, null, ct);
        view.PostingEnabled = setup.Enabled;
        var posts = (await conn.QueryAsync<TM_JCP_POSTING>(new CommandDefinition(
            "SELECT * FROM TM_JCP_POSTING WHERE JCP_TS_ID = :tsId ORDER BY JCP_FITH_DT", new { tsId }, cancellationToken: ct))).AsList();
        foreach (var p in posts)
        {
            var h = await VoucherHeaderAsync(conn, null, p.JCP_FITH_SYS_ID, ct);
            var v = new SheetJvVoucher
            {
                FITH_SYS_ID = p.JCP_FITH_SYS_ID, FITH_SRL_NO = h?.FITH_SRL_NO ?? p.JCP_FITH_SRL_NO, FITH_DT = h?.FITH_DT ?? p.JCP_FITH_DT,
                FITH_JRNL_TYPE = h?.FITH_JRNL_TYPE, FITH_STATUS = h?.FITH_STATUS, FITH_NARRATION = h?.FITH_NARRATION, Missing = h is null
            };
            v.PERIOD_STATUS = await conn.ExecuteScalarAsync<string?>(new CommandDefinition(
                "SELECT FCD_GL_STATUS FROM FINM_CALENDAR_DETAILS WHERE FCD_PERIOD_SYS_ID = :pid", new { pid = p.JCP_PERIOD_SYS_ID }, cancellationToken: ct));
            if (h is not null) v.Lines = (await VoucherLinesAsync(conn, null, h.FITH_SYS_ID, ct)).ToList();
            view.Vouchers.Add(v);
        }
        // What the sheet costs by today's rules — the explanation behind each Dr line, and what posts on the next save.
        view.Preview = await CalculateAsync(conn, null, sheet, setup, ct);
        return view;
    }

    // =====================================================================================
    // Setup
    // =====================================================================================

    public async Task<bool> IsSchemaReadyAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await SchemaInfo.HasColumnAsync(conn, "TM_JCP_SETUP", "JCS_ID", ct);
    }

    public async Task<TM_JCP_SETUP> GetSetupAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return await LoadSetupAsync(conn, null, ct);
    }

    public async Task SaveSetupAsync(TM_JCP_SETUP s, decimal userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var n = await conn.ExecuteAsync(new CommandDefinition(@"
            UPDATE TM_JCP_SETUP SET JCS_ENABLED_YN = :en, JCS_COMP_CODE = :comp, JCS_OFF_CODE = :off, JCS_JRNL_TYPE = :jt,
                   JCS_START_DATE = :sd, UPDATE_USER_ID = :u, UPDATE_DATE = SYSDATE
            WHERE  JCS_ID = 1",
            new { en = s.Enabled ? "Y" : "N", comp = s.JCS_COMP_CODE, off = s.JCS_OFF_CODE, jt = s.JCS_JRNL_TYPE, sd = s.JCS_START_DATE, u = userId },
            cancellationToken: ct));
        if (n == 0)
            await conn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO TM_JCP_SETUP (JCS_ID, JCS_ENABLED_YN, JCS_COMP_CODE, JCS_OFF_CODE, JCS_JRNL_TYPE, JCS_START_DATE, UPDATE_USER_ID, UPDATE_DATE)
                VALUES (1, :en, :comp, :off, :jt, :sd, :u, SYSDATE)",
                new { en = s.Enabled ? "Y" : "N", comp = s.JCS_COMP_CODE, off = s.JCS_OFF_CODE, jt = s.JCS_JRNL_TYPE, sd = s.JCS_START_DATE, u = userId },
                cancellationToken: ct));
        _logger.LogInformation("Job cost posting setup saved by {UserId}: enabled={Enabled}, {Off}/{Type}, from {Start}",
            userId, s.Enabled, s.JCS_OFF_CODE, s.JCS_JRNL_TYPE, s.JCS_START_DATE);
    }

    public async Task<IReadOnlyList<TM_JCP_COST_CODE>> GetCostCodesAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<TM_JCP_COST_CODE>(new CommandDefinition(@"
            SELECT * FROM TM_JCP_COST_CODE
            ORDER  BY DECODE(JCC_TYPE,'LABOUR',1,'TRAVEL_HR',2,3), JCC_LOCATION, JCC_EFFECTIVE_FROM DESC", cancellationToken: ct))).AsList();
    }

    public async Task SaveCostCodeAsync(TM_JCP_COST_CODE c, decimal userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        var p = new
        {
            id = c.JCC_ID, type = c.JCC_TYPE, loc = c.JCC_TYPE == CostTypes.Labour ? c.JCC_LOCATION : null, descr = c.JCC_DESC.Trim(),
            rate = c.JCC_RATE, dr = c.JCC_DR_VEL_CODE, cr = c.JCC_CR_VEL_CODE, eff = c.JCC_EFFECTIVE_FROM.Date,
            act = c.Active ? "Y" : "N", u = userId
        };
        if (c.JCC_ID > 0)
            await conn.ExecuteAsync(new CommandDefinition(@"
                UPDATE TM_JCP_COST_CODE SET JCC_TYPE = :type, JCC_LOCATION = :loc, JCC_DESC = :descr, JCC_RATE = :rate,
                       JCC_DR_VEL_CODE = :dr, JCC_CR_VEL_CODE = :cr, JCC_EFFECTIVE_FROM = :eff, ACTIVE_YN = :act,
                       UPDATE_USER_ID = :u, UPDATE_DATE = SYSDATE
                WHERE  JCC_ID = :id", p, cancellationToken: ct));
        else
            await conn.ExecuteAsync(new CommandDefinition(@"
                INSERT INTO TM_JCP_COST_CODE (JCC_ID, JCC_TYPE, JCC_LOCATION, JCC_DESC, JCC_RATE, JCC_DR_VEL_CODE, JCC_CR_VEL_CODE,
                                              JCC_EFFECTIVE_FROM, ACTIVE_YN, CREATION_USER_ID, CREATION_DATE)
                VALUES (TM_JCP_COST_CODE_SEQ.NEXTVAL, :type, :loc, :descr, :rate, :dr, :cr, :eff, :act, :u, SYSDATE)", p, cancellationToken: ct));
    }

    public async Task SetCostCodeActiveAsync(decimal id, bool active, decimal userId, CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        await conn.ExecuteAsync(new CommandDefinition(
            "UPDATE TM_JCP_COST_CODE SET ACTIVE_YN = :act, UPDATE_USER_ID = :u, UPDATE_DATE = SYSDATE WHERE JCC_ID = :id",
            new { act = active ? "Y" : "N", u = userId, id }, cancellationToken: ct));
    }

    public async Task<IReadOnlyList<GlAccountLookup>> GetAccountsAsync(CancellationToken ct = default)
    {
        // P&L accounts only, and never a party control account (the ERP refuses those in GL vouchers).
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<GlAccountLookup>(new CommandDefinition(@"
            SELECT a.FNAM_VEL_CODE, a.FNAM_FNAM_CODE, a.FNAM_DESC, a.FNAM_FNAM_TYPE, a.FNAM_FNAM_GROUP
            FROM   FINM_ACCOUNT_MASTER a
            WHERE  a.FNAM_FNAM_TYPE IN ('E','I')
              AND  NOT EXISTS (SELECT 1 FROM MMM_PARTY_DETAILS p WHERE p.MPD_CTRL_VEL_CODE = a.FNAM_VEL_CODE)
            ORDER  BY a.FNAM_FNAM_TYPE, a.FNAM_DESC", cancellationToken: ct))).AsList();
    }

    public async Task<IReadOnlyList<string>> GetJournalTypesAsync(CancellationToken ct = default)
    {
        using var conn = await _db.CreateOpenConnectionAsync(ct);
        return (await conn.QueryAsync<string>(new CommandDefinition(
            "SELECT FJTD_JRNL_TYPE FROM FINM_JOURNAL_TYPES_DEFN WHERE FJTD_JRNL_ORGN = 'GL' ORDER BY FJTD_JRNL_TYPE",
            cancellationToken: ct))).AsList();
    }

    // ---------------------------------------------------------------- row types

    private sealed class SheetRow
    {
        public decimal TS_ID { get; set; }
        public string? TS_SHEET_NO { get; set; }
        public string? TS_JOB_CODE { get; set; }
        public string? TS_LOCATION { get; set; }
        public string TS_SHEET_TYPE { get; set; } = "STANDARD";
        public string TS_ACTIVE_YN { get; set; } = "Y";
    }

    private sealed class CostLineRow
    {
        public string? TL_TECH_CODE { get; set; }
        public string? TL_TECH_NAME { get; set; }
        public DateTime TL_WORK_DATE { get; set; }
        public string? TL_LOCATION { get; set; }
        public string? TL_SEGMENT_TYPE { get; set; }
        public decimal TL_NET_HOURS { get; set; }
        public decimal TL_IDLE_HOURS { get; set; }
        public decimal TL_TRAVEL_HOURS { get; set; }
    }

    private sealed class PeriodRow
    {
        public decimal FCD_PERIOD_SYS_ID { get; set; }
        public decimal FCD_YEAR_ID { get; set; }
        public string? FCD_GL_STATUS { get; set; }
    }

    private sealed class VoucherHeaderRow
    {
        public decimal FITH_SYS_ID { get; set; }
        public string? FITH_SRL_NO { get; set; }
        public DateTime FITH_DT { get; set; }
        public string? FITH_STATUS { get; set; }
        public string? FITH_COMP_CODE { get; set; }
        public string? FITH_JRNL_TYPE { get; set; }
        public string? FITH_NARRATION { get; set; }
    }
}
