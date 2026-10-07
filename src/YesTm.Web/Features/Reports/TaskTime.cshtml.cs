using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Reports;

public class TaskTimeModel : PageModel
{
    private readonly IReportRepository _repo;
    private readonly ILogger<TaskTimeModel> _logger;

    public TaskTimeModel(IReportRepository repo, ILogger<TaskTimeModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public const int PageSize = 25;

    [BindProperty(SupportsGet = true)] public ReportFilterField FilterField { get; set; } = ReportFilterField.All;
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public ReportSortBy SortBy { get; set; } = ReportSortBy.Job;
    [BindProperty(SupportsGet = true)] public bool SortDesc { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNo { get; set; } = 1;

    public IReadOnlyList<TaskTimeReportRow> Rows { get; private set; } = [];
    public int TotalCount { get; private set; }
    public ReportTotals Totals { get; private set; } = ReportTotals.Zero;
    public decimal GrandNet => Totals.Net;
    public decimal GrandCost => Totals.Cost;
    public bool IsOffline { get; private set; }
    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    private bool IsAdmin => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);

    private ReportFilter BuildFilter(bool paged) => new()
    {
        FilterField = FilterField,
        Search = Search,
        DateFrom = DateFrom,
        DateTo = DateTo,
        SortBy = SortBy,
        SortDesc = SortDesc,
        PageNo = PageNo < 1 ? 1 : PageNo,
        PageSize = PageSize,
    };

    // Non-admins are scoped to rows they are involved in; admins pass null scope (all rows).
    private (decimal? userId, string? empCode) Scope() =>
        IsAdmin ? (null, null) : (User.UserId(), User.EmpCode());

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (PageNo < 1) PageNo = 1;
        var (uid, emp) = Scope();
        try
        {
            var result = await _repo.GetTaskTimeAsync(BuildFilter(paged: true), uid, emp, paged: true, ct);
            Rows = result.Rows;
            TotalCount = result.TotalCount;
            Totals = result.Totals;
            if (PageNo > TotalPages) PageNo = TotalPages;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Task & Time report unavailable (database offline)");
            IsOffline = true;
        }
    }

    public string FilterSummary()
    {
        var parts = new List<string>();
        var f = FilterField == ReportFilterField.All ? "All fields" : FilterField.ToString();
        if (!string.IsNullOrWhiteSpace(Search)) parts.Add($"{f}: \"{Search}\"");
        if (DateFrom is { } df) parts.Add($"from {df:dd-MMM-yyyy}");
        if (DateTo is { } dt) parts.Add($"to {dt:dd-MMM-yyyy}");
        if (parts.Count == 0) parts.Add("All task lines");
        return string.Join(" · ", parts);
    }

    public async Task<IActionResult> OnGetExportExcelAsync(CancellationToken ct)
    {
        var (uid, emp) = Scope();
        var result = await _repo.GetTaskTimeAsync(BuildFilter(paged: false), uid, emp, paged: false, ct);
        var bytes = TaskTimeExcel.BuildFlat(result.Rows, result.Totals);
        return File(bytes, XlsxMime, $"TaskTimeReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    /// <summary>Technician-wise workbook: a Summary tab plus one tab per technician (lines grouped by Job → Sheet).</summary>
    public async Task<IActionResult> OnGetExportTechnicianAsync(CancellationToken ct)
    {
        var (uid, emp) = Scope();
        var result = await _repo.GetTaskTimeAsync(BuildFilter(paged: false), uid, emp, paged: false, ct);
        var bytes = TaskTimeExcel.BuildTechnicianWise(result.Rows, FilterSummary());
        return File(bytes, XlsxMime, $"TechnicianTaskReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    private const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
