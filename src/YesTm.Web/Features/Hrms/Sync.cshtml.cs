using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Hrms;

/// <summary>
/// HRMS Sync &amp; Employee Mapping (admin): pull attendance from HRMS on demand, see the sync history,
/// and confirm which HRMS employee each TM technician is (auto-suggested, editable, Excel round-trip).
/// </summary>
public class SyncModel : PageModel
{
    private readonly HrmsSyncService _sync;
    private readonly IHrmsRepository _repo;
    private readonly ILogger<SyncModel> _logger;

    public SyncModel(HrmsSyncService sync, IHrmsRepository repo, ILogger<SyncModel> logger)
    {
        _sync = sync;
        _repo = repo;
        _logger = logger;
    }

    public HrmsOptions Options => _sync.Options;
    public bool IsConfigured => _sync.IsConfigured;
    public bool IsOffline { get; private set; }
    public IReadOnlyList<TM_SYNC_LOG> Logs { get; private set; } = [];
    public IReadOnlyList<EmpMapRow> Rows { get; private set; } = [];
    public IReadOnlyList<TM_HRMS_EMPLOYEE> HrmsEmployees { get; private set; } = [];

    [BindProperty(SupportsGet = true)] public string Show { get; set; } = "booked";   // booked | unmapped | suggested | all
    [BindProperty] public DateTime SyncFrom { get; set; } = DateTime.Today.AddDays(-35);
    [BindProperty] public DateTime SyncTo { get; set; } = DateTime.Today;

    public int CountConfirmed => Rows.Count(r => r.ECM_STATUS == MapStatus.Confirmed && r.ECM_HRMS_EMP_CODE is not null);
    public int CountSuggested => Rows.Count(r => r.ECM_STATUS == MapStatus.Suggested && r.ECM_HRMS_EMP_CODE is not null);
    public int CountBookedUnmapped => Rows.Count(r => r.BOOKED_LINES > 0 && !(r.ECM_STATUS == MapStatus.Confirmed && r.ECM_HRMS_EMP_CODE is not null));

    public IEnumerable<EmpMapRow> Visible => Show switch
    {
        "all" => Rows,
        "unmapped" => Rows.Where(r => r.ECM_HRMS_EMP_CODE is null || r.ECM_STATUS != MapStatus.Confirmed),
        "suggested" => Rows.Where(r => r.ECM_STATUS == MapStatus.Suggested && r.ECM_HRMS_EMP_CODE is not null),
        _ => Rows.Where(r => r.BOOKED_LINES > 0)
    };

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSyncAsync(CancellationToken ct)
    {
        if ((SyncTo - SyncFrom).TotalDays > 400)
        {
            TempData["Error"] = "Sync at most 400 days at a time.";
            return RedirectToPage(new { Show });
        }
        var res = await _sync.SyncAsync(SyncFrom, SyncTo, "MANUAL", User.UserId(), ct);
        TempData[res.Ok ? "Success" : "Error"] = res.Message;
        return RedirectToPage(new { Show });
    }

    /// <summary>Confirm / change / reject one technician's mapping.</summary>
    public async Task<IActionResult> OnPostSaveMapAsync(string tmCode, string? hrmsCode, string decision, string? originalHrms, CancellationToken ct)
    {
        try
        {
            if (decision == "reject")
                await _repo.SaveMapAsync(tmCode, null, MatchTypes.None, MapStatus.Rejected, User.UserId(), ct);
            else
            {
                var changed = !string.Equals(hrmsCode ?? "", originalHrms ?? "", StringComparison.OrdinalIgnoreCase);
                var current = (await _repo.GetMapsAsync(ct)).FirstOrDefault(m => m.ECM_TM_EMP_CODE == tmCode);
                var type = changed || current?.ECM_MATCH_TYPE is null or MatchTypes.None ? MatchTypes.Manual : current.ECM_MATCH_TYPE!;
                await _repo.SaveMapAsync(tmCode, hrmsCode, string.IsNullOrWhiteSpace(hrmsCode) ? MatchTypes.None : type,
                    string.IsNullOrWhiteSpace(hrmsCode) ? MapStatus.Rejected : MapStatus.Confirmed, User.UserId(), ct);
            }
            TempData["Success"] = $"Mapping for {tmCode} saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saving mapping for {TmCode} failed", tmCode);
            TempData["Error"] = "Unable to save the mapping.";
        }
        return RedirectToPage(new { Show });
    }

    /// <summary>Confirm every suggestion at or above the given confidence (name matches are the usual case).</summary>
    public async Task<IActionResult> OnPostConfirmAllAsync(decimal minConfidence, CancellationToken ct)
    {
        var maps = await _repo.GetMapsAsync(ct);
        var n = 0;
        foreach (var m in maps.Where(m => m.ECM_STATUS == MapStatus.Suggested && m.ECM_HRMS_EMP_CODE is not null && (m.ECM_CONFIDENCE ?? 0) >= minConfidence))
        {
            await _repo.SaveMapAsync(m.ECM_TM_EMP_CODE, m.ECM_HRMS_EMP_CODE, m.ECM_MATCH_TYPE ?? MatchTypes.Name, MapStatus.Confirmed, User.UserId(), ct);
            n++;
        }
        TempData["Success"] = $"{n} suggestion(s) with confidence ≥ {minConfidence:0} confirmed.";
        return RedirectToPage(new { Show });
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Mapping");
        string[] h = ["TM Code", "TM Name", "Booked Lines", "HRMS Code", "HRMS Name", "HRMS Employment No", "Match Type", "Confidence", "Status"];
        for (var c = 0; c < h.Length; c++) ws.Cell(1, c + 1).Value = h[c];
        ws.Range(1, 1, 1, h.Length).Style.Font.Bold = true;
        ws.Range(1, 1, 1, h.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE3EF");
        var r = 2;
        foreach (var x in Rows)
        {
            ws.Cell(r, 1).Value = x.TM_EMP_CODE; ws.Cell(r, 2).Value = x.TM_EMP_NAME; ws.Cell(r, 3).Value = x.BOOKED_LINES;
            ws.Cell(r, 4).Value = x.ECM_HRMS_EMP_CODE; ws.Cell(r, 5).Value = x.HRMS_EMP_NAME; ws.Cell(r, 6).Value = x.HRMS_EMPLOYMENT_NO;
            ws.Cell(r, 7).Value = x.ECM_MATCH_TYPE; ws.Cell(r, 8).Value = x.ECM_CONFIDENCE; ws.Cell(r, 9).Value = x.ECM_STATUS;
            r++;
        }
        ws.Range(2, 4, Math.Max(2, r - 1), 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF8E1");
        var hr = wb.AddWorksheet("HRMS Employees");
        string[] hh = ["HRMS Code", "Name", "Employment No", "Active", "Branch", "Department", "Designation"];
        for (var c = 0; c < hh.Length; c++) hr.Cell(1, c + 1).Value = hh[c];
        hr.Range(1, 1, 1, hh.Length).Style.Font.Bold = true;
        r = 2;
        foreach (var e in HrmsEmployees)
        {
            hr.Cell(r, 1).Value = e.HE_EMP_CODE; hr.Cell(r, 2).Value = e.HE_EMP_NAME; hr.Cell(r, 3).Value = e.HE_EMPLOYMENT_NO;
            hr.Cell(r, 4).Value = e.HE_ACTIVE; hr.Cell(r, 5).Value = e.HE_BRANCH_CODE; hr.Cell(r, 6).Value = e.HE_DEPT_NAME;
            hr.Cell(r, 7).Value = e.HE_DESIG_NAME;
            r++;
        }
        var readme = wb.AddWorksheet("How to import");
        readme.Cell(1, 1).Value = "Fill or correct the HRMS Code column (yellow) on the Mapping tab, then use Import on the HRMS Sync page.";
        readme.Cell(2, 1).Value = "Only TM Code + HRMS Code are read. Every imported row becomes CONFIRMED (MANUAL). A blank HRMS Code marks the technician as 'not in HRMS'.";
        readme.Cell(3, 1).Value = "Look up codes on the 'HRMS Employees' tab.";
        foreach (var s in wb.Worksheets) s.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"TM_HRMS_EmployeeMapping_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    public async Task<IActionResult> OnPostImportAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Choose the mapping Excel file to import.";
            return RedirectToPage(new { Show });
        }
        try
        {
            var hrms = (await _repo.GetHrmsEmployeesAsync(ct)).Select(e => e.HE_EMP_CODE).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var tm = (await _repo.GetTmEmployeesAsync(ct)).Select(e => e.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            using var stream = file.OpenReadStream();
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.FirstOrDefault(w => w.Name == "Mapping") ?? wb.Worksheet(1);
            var header = ws.Row(1).CellsUsed().ToDictionary(c => c.GetString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
            if (!header.TryGetValue("TM Code", out var cTm) || !header.TryGetValue("HRMS Code", out var cHr))
            {
                TempData["Error"] = "The sheet needs 'TM Code' and 'HRMS Code' columns (use Export to get the template).";
                return RedirectToPage(new { Show });
            }
            int ok = 0, skipped = 0;
            var problems = new List<string>();
            var seen = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in ws.RowsUsed().Skip(1))
            {
                var tmCode = row.Cell(cTm).GetString().Trim();
                var hrCode = row.Cell(cHr).GetString().Trim();
                if (tmCode.Length == 0) continue;
                if (!tm.Contains(tmCode)) { skipped++; problems.Add($"{tmCode}: not a TM employee"); continue; }
                if (hrCode.Length > 0 && !hrms.Contains(hrCode)) { skipped++; problems.Add($"{tmCode}: HRMS code {hrCode} not found (sync first?)"); continue; }
                if (hrCode.Length > 0 && seen.TryGetValue(hrCode, out var other)) { skipped++; problems.Add($"{tmCode}: HRMS {hrCode} already given to {other}"); continue; }
                if (hrCode.Length > 0) seen[hrCode] = tmCode;
                await _repo.SaveMapAsync(tmCode, hrCode.Length == 0 ? null : hrCode, hrCode.Length == 0 ? MatchTypes.None : MatchTypes.Manual,
                    hrCode.Length == 0 ? MapStatus.Rejected : MapStatus.Confirmed, User.UserId(), ct);
                ok++;
            }
            TempData[skipped == 0 ? "Success" : "Error"] = $"Imported {ok} mapping(s)" +
                (skipped == 0 ? "." : $", skipped {skipped}: " + string.Join("; ", problems.Take(8)) + (problems.Count > 8 ? " …" : ""));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mapping import failed");
            TempData["Error"] = "Could not read the Excel file: " + ex.Message;
        }
        return RedirectToPage(new { Show });
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            Logs = await _repo.GetRecentLogsAsync(10, ct);
            Rows = await _repo.GetMapRowsAsync(ct);
            HrmsEmployees = await _repo.GetHrmsEmployeesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HRMS sync page data unavailable (run db/10_hrms_attendance.sql?)");
            IsOffline = true;
        }
    }
}
