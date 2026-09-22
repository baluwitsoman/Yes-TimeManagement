using ClosedXML.Excel;
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
    public decimal GrandNet { get; private set; }
    public decimal GrandCost { get; private set; }
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
            GrandNet = result.GrandNet;
            GrandCost = result.GrandCost;
            if (PageNo > TotalPages) PageNo = TotalPages;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Task & Time report unavailable (database offline)");
            IsOffline = true;
        }
    }

    public async Task<IActionResult> OnGetExportExcelAsync(CancellationToken ct)
    {
        var (uid, emp) = Scope();
        var result = await _repo.GetTaskTimeAsync(BuildFilter(paged: false), uid, emp, paged: false, ct);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Task & Time");

        string[] headers =
        [
            "Sheet No", "Posting Date", "Job No", "Customer", "Task", "Tech Code", "Technician",
            "Skill", "Location", "Start", "End", "Std Hrs", "Net Hrs", "Time Type", "Rate", "Labour Cost",
            "Work Type", "Job Status"
        ];
        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];
        ws.Row(1).Style.Font.Bold = true;

        var r = 2;
        foreach (var x in result.Rows)
        {
            ws.Cell(r, 1).Value = x.TS_SHEET_NO;
            ws.Cell(r, 2).Value = x.TS_POSTING_DATE;
            ws.Cell(r, 2).Style.DateFormat.Format = "dd-MMM-yyyy";
            ws.Cell(r, 3).Value = x.TL_JOB_CODE;
            ws.Cell(r, 4).Value = x.TS_CUSTOMER_NAME;
            ws.Cell(r, 5).Value = x.TL_TASK_NAME;
            ws.Cell(r, 6).Value = x.TL_TECH_CODE;
            ws.Cell(r, 7).Value = x.TL_TECH_NAME;
            ws.Cell(r, 8).Value = x.TL_SKILL;
            ws.Cell(r, 9).Value = x.TL_LOCATION;
            if (x.TL_START_DT is { } sdt) { ws.Cell(r, 10).Value = sdt; ws.Cell(r, 10).Style.DateFormat.Format = "dd-MMM-yyyy HH:mm"; }
            if (x.TL_END_DT is { } edt) { ws.Cell(r, 11).Value = edt; ws.Cell(r, 11).Style.DateFormat.Format = "dd-MMM-yyyy HH:mm"; }
            ws.Cell(r, 12).Value = x.TL_STD_HOURS;
            ws.Cell(r, 13).Value = x.TL_NET_HOURS;
            ws.Cell(r, 14).Value = x.TL_TIME_TYPE;
            ws.Cell(r, 15).Value = x.TL_RATE;
            ws.Cell(r, 16).Value = x.TL_LABOUR_COST;
            ws.Cell(r, 17).Value = x.WORK_TYPE_NAME;
            ws.Cell(r, 18).Value = x.TS_JOB_STATUS;
            r++;
        }

        // Totals row.
        ws.Cell(r, 11).Value = "Total";
        ws.Cell(r, 13).Value = result.GrandNet;
        ws.Cell(r, 16).Value = result.GrandCost;
        ws.Range(r, 1, r, 18).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var fileName = $"TaskTimeReport_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
