using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Reports;

public class TaskTimePrintModel : PageModel
{
    private readonly IReportRepository _repo;
    private readonly ILogger<TaskTimePrintModel> _logger;

    public TaskTimePrintModel(IReportRepository repo, ILogger<TaskTimePrintModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)] public ReportFilterField FilterField { get; set; } = ReportFilterField.All;
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public ReportSortBy SortBy { get; set; } = ReportSortBy.Job;
    [BindProperty(SupportsGet = true)] public bool SortDesc { get; set; }

    public IReadOnlyList<TaskTimeReportRow> Rows { get; private set; } = [];
    public decimal GrandNet { get; private set; }
    public decimal GrandCost { get; private set; }
    public bool IsOffline { get; private set; }

    private bool IsAdmin => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);

    public string FilterSummary()
    {
        var parts = new List<string>();
        var f = FilterField == ReportFilterField.All ? "All fields" : FilterField.ToString();
        if (!string.IsNullOrWhiteSpace(Search)) parts.Add($"{f}: \"{Search}\"");
        if (DateFrom is { } df) parts.Add($"from {df:dd-MMM-yyyy}");
        if (DateTo is { } dt) parts.Add($"to {dt:dd-MMM-yyyy}");
        parts.Add($"sorted by {SortBy}{(SortDesc ? " (desc)" : "")}");
        return string.Join(" · ", parts);
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var filter = new ReportFilter
        {
            FilterField = FilterField, Search = Search, DateFrom = DateFrom, DateTo = DateTo,
            SortBy = SortBy, SortDesc = SortDesc,
        };
        var (uid, emp) = IsAdmin ? ((decimal?)null, (string?)null) : (User.UserId(), User.EmpCode());
        try
        {
            var result = await _repo.GetTaskTimeAsync(filter, uid, emp, paged: false, ct);
            Rows = result.Rows;
            GrandNet = result.GrandNet;
            GrandCost = result.GrandCost;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Task & Time print report unavailable (database offline)");
            IsOffline = true;
        }
    }
}
