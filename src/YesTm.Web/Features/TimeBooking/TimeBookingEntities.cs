namespace YesTm.Web.Features.TimeBooking;

/// <summary>New table TM_TIME_SHEET — Time Booking header (one per Work Order posting).</summary>
public sealed class TM_TIME_SHEET
{
    public decimal TS_ID { get; set; }
    public string? TS_SHEET_NO { get; set; }
    public DateTime TS_POSTING_DATE { get; set; }
    public string? TS_JOB_CODE { get; set; }
    public string? TS_CUSTOMER_CODE { get; set; }
    public string? TS_CUSTOMER_NAME { get; set; }
    public string? TS_EQUIPMENT { get; set; }
    public string? TS_INDUSTRY_CODE { get; set; }
    public string? TS_LOCATION { get; set; }
    public string? TS_WORK_TYPE_CODE { get; set; }
    public string? TS_JOB_STATUS { get; set; }
    public string? TS_BRAND { get; set; }
    public string? TS_EQUIPMENT_TYPE { get; set; }
    public string? TS_SERVICE_TYPE { get; set; }
    public string? TS_SERIAL_NO { get; set; }
    public DateTime? TS_JOB_OPENING_DATE { get; set; }
    public decimal TS_TOTAL_NET_HOURS { get; set; }
    public decimal TS_TOTAL_LABOUR_COST { get; set; }
    public decimal TS_NORMAL_HOURS { get; set; }
    public decimal TS_OT_HOURS { get; set; }
    public decimal TS_STD_HOURS { get; set; }
    public string? TS_REMARKS { get; set; }
    public string? TS_ACTIVE_YN { get; set; }
    public decimal? TS_CREATION_USER_ID { get; set; }
    public DateTime? TS_CREATION_DATE { get; set; }
}

/// <summary>New table TM_TIME_LINE — one bookable line per Task (key = Job No + Task).</summary>
public sealed class TM_TIME_LINE
{
    public decimal TL_ID { get; set; }
    public decimal TL_TS_ID { get; set; }
    public decimal TL_LINE_NO { get; set; }
    public string? TL_TASK_CODE { get; set; }
    public string? TL_TASK_NAME { get; set; }
    public decimal TL_STD_HOURS { get; set; }
    public string? TL_TECH_CODE { get; set; }
    public string? TL_TECH_NAME { get; set; }
    public string? TL_SKILL { get; set; }
    public DateTime TL_START_DT { get; set; }
    public DateTime TL_END_DT { get; set; }
    public decimal TL_LUNCH_HOURS { get; set; }
    public decimal TL_NET_HOURS { get; set; }
    public string? TL_TIME_TYPE { get; set; }
    public decimal TL_RATE { get; set; }
    public decimal TL_LABOUR_COST { get; set; }
    public string? TL_JOB_CODE { get; set; }
}

// -------- Lookup / view DTOs (read from existing ERP + TM masters) --------

public sealed class JobLookup
{
    public string MTJ_JOB_CODE { get; set; } = string.Empty;
    public string? MTJ_JOB_DESC { get; set; }
    public string? MTJ_PARTY_CODE { get; set; }
    public string? CUSTOMER_NAME { get; set; }
    public string? MTJ_BRAND { get; set; }
    public string? MTJ_JOB_LOCATION { get; set; }
    public string? INDUSTRY_CODE { get; set; }
    // Equipment details resolved from the job + ERP lookup masters.
    public string? BRAND_DESC { get; set; }
    public string? EQUIPMENT_TYPE_DESC { get; set; }
    public string? SERVICE_TYPE_DESC { get; set; }
    public string? MTJ_SERIAL_NO { get; set; }
    public DateTime? MTJ_JOB_OPENING_DATE { get; set; }
}

public sealed class TechnicianLookup
{
    public string PEMP_EMP_CODE { get; set; } = string.Empty;
    public string? PEMP_EMP_NAME { get; set; }
}

public sealed class TaskLookup
{
    public string TASK_CODE { get; set; } = string.Empty;
    public string? TASK_NAME { get; set; }
    public string? DEFAULT_SKILL { get; set; }
    public decimal STD_HOURS { get; set; }
}

public sealed class WorkTypeLookup
{
    public string WORK_TYPE_CODE { get; set; } = string.Empty;
    public string? WORK_TYPE_NAME { get; set; }
}

/// <summary>Duty-timing master row used to drive Net-hours / Normal-Overtime.</summary>
public sealed class DutyTimingDto
{
    public string DUTY_START { get; set; } = "08:00";
    public string DUTY_END { get; set; } = "17:00";
    public decimal DEFAULT_LUNCH_HOURS { get; set; } = 0.5m;
    public string WEEKEND_DAYS { get; set; } = "Friday,Saturday";
}

/// <summary>Header summary row for the Time Booking list page.</summary>
public sealed class TimeSheetListItem
{
    public decimal TS_ID { get; set; }
    public string? TS_SHEET_NO { get; set; }
    public DateTime TS_POSTING_DATE { get; set; }
    public string? TS_JOB_CODE { get; set; }
    public string? TS_CUSTOMER_NAME { get; set; }
    public string? TS_LOCATION { get; set; }
    public string? TS_JOB_STATUS { get; set; }
    public decimal TS_TOTAL_NET_HOURS { get; set; }
    public decimal TS_TOTAL_LABOUR_COST { get; set; }
}
