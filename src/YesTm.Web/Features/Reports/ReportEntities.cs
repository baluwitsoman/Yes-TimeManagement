namespace YesTm.Web.Features.Reports;

/// <summary>Which column the free-text search box applies to.</summary>
public enum ReportFilterField { All, Job, Customer, Technician }

/// <summary>User-chosen sort column (mapped to a whitelisted SQL column — never raw text).</summary>
public enum ReportSortBy { Job, Employee, Customer, Date, Task, Cost }

/// <summary>One flat row of the Task &amp; Time report — a single booked task line with its sheet context.</summary>
public sealed class TaskTimeReportRow
{
    public string? TS_SHEET_NO { get; set; }
    public DateTime TS_POSTING_DATE { get; set; }
    public string? TL_JOB_CODE { get; set; }
    public string? TS_CUSTOMER_NAME { get; set; }
    public string? TL_TASK_NAME { get; set; }
    public string? TL_TECH_CODE { get; set; }
    public string? TL_TECH_NAME { get; set; }
    public string? TL_SKILL { get; set; }
    public string? TL_LOCATION { get; set; }
    public DateTime? TL_START_DT { get; set; }
    public DateTime? TL_END_DT { get; set; }
    public decimal TL_STD_HOURS { get; set; }
    public decimal TL_NET_HOURS { get; set; }
    public string? TL_TIME_TYPE { get; set; }
    public decimal TL_RATE { get; set; }
    public decimal TL_LABOUR_COST { get; set; }
    public string? WORK_TYPE_NAME { get; set; }
    public string? TS_JOB_STATUS { get; set; }
    // ---- Phase 2 line columns ----
    public DateTime? TL_WORK_DATE { get; set; }
    public decimal TL_LUNCH_HOURS { get; set; }
    public decimal TL_NORMAL_HOURS { get; set; }
    public decimal TL_OT_HOURS { get; set; }
    public decimal TL_OT_RATE { get; set; }
    public decimal TL_FOOD_ALLOWANCE { get; set; }
    public decimal TL_TOTAL_COST { get; set; }
    public string? TL_TRAVEL_SITE { get; set; }
    public DateTime? TL_TRAVEL_START { get; set; }
    public DateTime? TL_TRAVEL_END { get; set; }
    public decimal TL_TRAVEL_HOURS { get; set; }
    public string? TL_OVERRIDE_YN { get; set; }

    // Window aggregates — identical on every row of a given filtered set.
    public int TOTAL_ROWS { get; set; }
    public decimal GRAND_NET { get; set; }
    public decimal GRAND_COST { get; set; }
    public decimal GRAND_NORMAL { get; set; }
    public decimal GRAND_OT { get; set; }
    public decimal GRAND_FOOD { get; set; }
    public decimal GRAND_TOTAL { get; set; }
    public decimal GRAND_TRAVEL { get; set; }
}

/// <summary>Grand totals across the whole filtered set of the Task &amp; Time report.</summary>
public sealed record ReportTotals(decimal Net, decimal Cost, decimal Normal, decimal Ot, decimal Food, decimal Total, decimal Travel)
{
    public static readonly ReportTotals Zero = new(0, 0, 0, 0, 0, 0, 0);
}

/// <summary>Bound filter/sort/paging state for the Task &amp; Time report.</summary>
public sealed class ReportFilter
{
    public ReportFilterField FilterField { get; set; } = ReportFilterField.All;
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public ReportSortBy SortBy { get; set; } = ReportSortBy.Job;
    public bool SortDesc { get; set; }
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>A page of report rows plus the total count and grand totals across the whole filtered set.</summary>
public sealed record PagedReport(IReadOnlyList<TaskTimeReportRow> Rows, int TotalCount, ReportTotals Totals)
{
    public decimal GrandNet => Totals.Net;
    public decimal GrandCost => Totals.Cost;
}
