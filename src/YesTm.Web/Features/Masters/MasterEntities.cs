namespace YesTm.Web.Features.Masters;

// Property names mirror Oracle column names 1:1 (project convention).

public sealed class TM_WORK_TYPE
{
    public string WORK_TYPE_CODE { get; set; } = string.Empty;
    public string? WORK_TYPE_NAME { get; set; }
    public string? CATEGORY { get; set; }
    public string? COST_TREATMENT { get; set; }
    public string? SUB_TYPE { get; set; }
    public string ACTIVE_YN { get; set; } = "Y";
}

public sealed class TM_INDUSTRY
{
    public string INDUSTRY_CODE { get; set; } = string.Empty;
    public string? INDUSTRY_NAME { get; set; }
    public string ACTIVE_YN { get; set; } = "Y";
}

public sealed class TM_JOB_LOCATION
{
    public string LOC_CODE { get; set; } = string.Empty;
    public string? LOC_NAME { get; set; }
    public decimal? SORT_ORDER { get; set; }
    public string ACTIVE_YN { get; set; } = "Y";
}

public sealed class TM_TASK
{
    public string TASK_CODE { get; set; } = string.Empty;
    public string? TASK_NAME { get; set; }
    public string? TASK_GROUP { get; set; }
    public decimal? SEQ_NO { get; set; }
    public string? DEFAULT_SKILL { get; set; }
    public decimal STD_HOURS { get; set; }
    public string CHARGEABLE_YN { get; set; } = "Y";
    public string ACTIVE_YN { get; set; } = "Y";
}

public sealed class TM_LABOUR_RATE
{
    public decimal LR_ID { get; set; }
    public string? LR_LOCATION { get; set; }
    public string? LR_INDUSTRY_CODE { get; set; }
    public string? LR_TIME_TYPE { get; set; }
    public decimal LR_RATE { get; set; }
    public decimal? LR_COST_RATE { get; set; }
    public decimal? LR_OT_MULTIPLIER { get; set; }
    public DateTime? LR_EFFECTIVE_FROM { get; set; }
    public string LR_ACTIVE_YN { get; set; } = "Y";
}

public sealed class TM_DUTY_TIMING
{
    public decimal DT_ID { get; set; }
    public string? DT_NAME { get; set; }
    public string? DT_START { get; set; }
    public string? DT_END { get; set; }
    public decimal DT_LUNCH_HOURS { get; set; }
    public string? DT_WEEKEND_DAYS { get; set; }
    public decimal? DT_MONTHLY_BENCHMARK { get; set; }
    public string ACTIVE_YN { get; set; } = "Y";
}

public sealed class TM_CUST_INDUSTRY_MAP
{
    public decimal CIM_ID { get; set; }
    public string? CIM_CUSTOMER_CODE { get; set; }
    public string? CIM_INDUSTRY_CODE { get; set; }
    public string? CIM_DEFAULT_LOCATION { get; set; }
    public DateTime? CIM_EFFECTIVE_FROM { get; set; }
    public string CIM_ACTIVE_YN { get; set; } = "Y";
    // joined for display
    public string? CUSTOMER_NAME { get; set; }
    public string? INDUSTRY_NAME { get; set; }
}

public sealed class TM_TASK_STD
{
    public decimal TST_ID { get; set; }
    public string? TST_MANUFACTURER { get; set; }
    public string? TST_MODEL { get; set; }
    public string? TST_SERIES { get; set; }
    public string? TST_COMPONENT { get; set; }
    public string? TST_TASK_CODE { get; set; }
    public decimal TST_STD_HOURS { get; set; }
    public string? TST_ACCEPT_BENCHMARK { get; set; }
    public string ACTIVE_YN { get; set; } = "Y";
    // joined for display
    public string? TASK_NAME { get; set; }
}
