using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Reports;

/// <summary>
/// Technician Utilisation &amp; Efficiency: for a period, each technician's availability (calendar →
/// scheduled → available days/hours from HRMS attendance) against what he booked (normal/OT, work,
/// travel, idle, earned standard), with every formula shown.
/// </summary>
public class UtilisationModel : PageModel
{
    private readonly IReportRepository _repo;
    private readonly IUtilisationService _util;
    private readonly ILogger<UtilisationModel> _logger;

    public UtilisationModel(IReportRepository repo, IUtilisationService util, ILogger<UtilisationModel> logger)
    {
        _repo = repo;
        _util = util;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public string? Technician { get; set; }

    public UtilisationReport? Report { get; private set; }
    public bool IsOffline { get; private set; }
    public bool IsAdmin => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);

    public IEnumerable<TechUtilisation> Rows => Report is null ? [] :
        string.IsNullOrWhiteSpace(Technician) ? Report.Technicians
        : Report.Technicians.Where(t => (t.TechCode + " " + t.TechName).Contains(Technician.Trim(), StringComparison.OrdinalIgnoreCase));

    public async Task OnGetAsync(CancellationToken ct)
    {
        try { Report = await BuildAsync(ct); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Utilisation report unavailable");
            IsOffline = true;
        }
    }

    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        var report = await BuildAsync(ct);
        var filtered = report with { Technicians = string.IsNullOrWhiteSpace(Technician) ? report.Technicians : Rows.ToList() };
        using var wb = new XLWorkbook();
        TaskTimeExcel.AddUtilisationSheet(wb, filtered, string.IsNullOrWhiteSpace(Technician) ? "All technicians" : $"Technician: \"{Technician}\"");
        TaskTimeExcel.AddDefinitionsSheet(wb, report.StdHoursPerDay);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"TechnicianUtilisation_{report.From:yyyyMMdd}_{report.To:yyyyMMdd}.xlsx");
    }

    private async Task<UtilisationReport> BuildAsync(CancellationToken ct)
    {
        // Default period: this month to date.
        DateFrom ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateTo ??= DateTime.Today;
        if (DateTo < DateFrom) (DateFrom, DateTo) = (DateTo, DateFrom);

        var filter = new ReportFilter { DateFrom = DateFrom, DateTo = DateTo, PageSize = int.MaxValue };
        var (uid, emp) = IsAdmin ? ((decimal?)null, (string?)null) : (User.UserId(), User.EmpCode());
        var rows = (await _repo.GetTaskTimeAsync(filter, uid, emp, paged: false, ct)).Rows;
        // A normal user only sees himself (rows on his sheets may include crew mates).
        if (!IsAdmin && emp is not null)
            rows = rows.Where(r => string.Equals(r.TL_TECH_CODE, emp, StringComparison.OrdinalIgnoreCase)).ToList();
        return await _util.BuildAsync(rows, DateFrom, DateTo, includeAllMapped: IsAdmin, ct);
    }
}
