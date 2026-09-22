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

    // Window aggregates — identical on every row of a given filtered set.
    public int TOTAL_ROWS { get; set; }
    public decimal GRAND_NET { get; set; }
    public decimal GRAND_COST { get; set; }
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
public sealed record PagedReport(IReadOnlyList<TaskTimeReportRow> Rows, int TotalCount, decimal GrandNet, decimal GrandCost);
