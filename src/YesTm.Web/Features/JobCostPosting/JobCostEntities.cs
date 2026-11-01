namespace YesTm.Web.Features.JobCostPosting;

/// <summary>New table TM_JCP_SETUP (db/12) — single row (JCS_ID = 1) of posting parameters.</summary>
public sealed class TM_JCP_SETUP
{
    public decimal JCS_ID { get; set; } = 1;
    public string JCS_ENABLED_YN { get; set; } = "N";
    public string JCS_COMP_CODE { get; set; } = "01";
    /// <summary>FITH_OFF_CODE — branch the vouchers are numbered under.</summary>
    public string JCS_OFF_CODE { get; set; } = "01";
    /// <summary>GL journal type (FINM_JOURNAL_TYPES_DEFN, origin GL), normally JRV.</summary>
    public string JCS_JRNL_TYPE { get; set; } = "JRV";
    /// <summary>Cut-over: work dates before this are never posted.</summary>
    public DateTime? JCS_START_DATE { get; set; }
    public decimal? UPDATE_USER_ID { get; set; }
    public DateTime? UPDATE_DATE { get; set; }

    public bool Enabled => string.Equals(JCS_ENABLED_YN, "Y", StringComparison.OrdinalIgnoreCase);
}

/// <summary>New table TM_JCP_COST_CODE (db/12) — what one hour / km costs and where it is posted.</summary>
public sealed class TM_JCP_COST_CODE
{
    public decimal JCC_ID { get; set; }
    /// <summary><see cref="CostTypes"/>.</summary>
    public string JCC_TYPE { get; set; } = CostTypes.Labour;
    /// <summary>LABOUR only: the job location (TM_JOB_LOCATION.LOC_CODE) whose hours this rate costs.</summary>
    public string? JCC_LOCATION { get; set; }
    public string JCC_DESC { get; set; } = string.Empty;
    public decimal JCC_RATE { get; set; }
    /// <summary>Debit: job cost account (FINM_ACCOUNT_MASTER.FNAM_VEL_CODE), tagged with the job code.</summary>
    public decimal? JCC_DR_VEL_CODE { get; set; }
    /// <summary>Credit: absorbed / recovery contra account, no job code.</summary>
    public decimal? JCC_CR_VEL_CODE { get; set; }
    public DateTime JCC_EFFECTIVE_FROM { get; set; } = new(2000, 1, 1);
    public string ACTIVE_YN { get; set; } = "Y";

    public bool Active => string.Equals(ACTIVE_YN, "Y", StringComparison.OrdinalIgnoreCase);
    public string Uom => JCC_TYPE == CostTypes.TravelKm ? "km" : "h";
}

/// <summary>TM_JCP_COST_CODE.JCC_TYPE values.</summary>
public static class CostTypes
{
    public const string Labour = "LABOUR";
    public const string TravelHours = "TRAVEL_HR";
    public const string TravelKm = "TRAVEL_KM";

    public static string Label(string? type) => type switch
    {
        Labour => "Labour hours",
        TravelHours => "Travel hours",
        TravelKm => "Vehicle km",
        _ => type ?? string.Empty
    };
}

/// <summary>New table TM_JCP_POSTING (db/12) — the ERP voucher a sheet owns for one GL period.</summary>
public sealed class TM_JCP_POSTING
{
    public decimal JCP_TS_ID { get; set; }
    public decimal JCP_PERIOD_SYS_ID { get; set; }
    public decimal JCP_FITH_SYS_ID { get; set; }
    public string JCP_FITH_SRL_NO { get; set; } = string.Empty;
    public DateTime JCP_FITH_DT { get; set; }
    public decimal JCP_AMOUNT { get; set; }
}

/// <summary>A GL account offered in the setup dropdowns (FINM_ACCOUNT_MASTER).</summary>
public sealed class GlAccountLookup
{
    public decimal FNAM_VEL_CODE { get; set; }
    public string? FNAM_FNAM_CODE { get; set; }
    public string? FNAM_DESC { get; set; }
    public string? FNAM_FNAM_TYPE { get; set; }
    public string? FNAM_FNAM_GROUP { get; set; }

    public string Display => $"{FNAM_FNAM_CODE} — {FNAM_DESC?.Replace("YES/", "").Replace("/GEN/GEN/GEN", "").Trim()}";
}

// ------------------------------------------------------------------ calculation in / out

/// <summary>One time line as the cost calculation needs it.</summary>
public sealed record CostLineInput(string? TechCode, string? TechName, DateTime WorkDate, string? Location,
    string? SegmentType, decimal NetHours, decimal IdleHours, decimal TravelHours);

/// <summary>Field Service Report vehicle km (one per FIELD sheet).</summary>
public sealed record CostKmInput(decimal TotalKm, DateTime? ReportDate, string? ReportNo, string? PlateNo);

/// <summary>One voucher line (debit &gt; 0, credit &lt; 0) in ERP sign convention.</summary>
public sealed record JvLine(decimal VelCode, decimal Amount, string? JobCode, string Narration);

/// <summary>Dr detail kept for display: who / what / how many × rate.</summary>
public sealed record JvCostItem(string CostDesc, string? TechCode, string? TechName, decimal Qty, string Uom, decimal Rate, decimal Amount);

/// <summary>Everything that goes into one month's voucher for a sheet.</summary>
public sealed class JvDraft
{
    public DateTime Month { get; init; }        // first day of the month
    public DateTime JvDate { get; set; }        // latest work date in the month
    public List<JvCostItem> Items { get; } = [];
    public List<JvLine> Lines { get; } = [];
    public decimal Total => Lines.Where(l => l.Amount > 0).Sum(l => l.Amount);
}

public sealed class JobCostCalcResult
{
    public List<JvDraft> Vouchers { get; } = [];
    /// <summary>Hours / km that could not be costed (no rate or no account) — shown to the user, never silently lost.</summary>
    public List<string> Warnings { get; } = [];
}

/// <summary>Raised when the sheet's JV cannot be brought in line (closed period, numbering missing…); the save is rolled back.</summary>
public sealed class JobCostPostingException(string message) : Exception(message);

// ------------------------------------------------------------------ JV view

public sealed class SheetJvView
{
    public string? SheetNo { get; set; }
    public string? JobCode { get; set; }
    public bool PostingEnabled { get; set; }
    public bool SchemaReady { get; set; }
    public List<SheetJvVoucher> Vouchers { get; } = [];
    /// <summary>What the sheet would post right now (shown when it differs from, or there is no, voucher).</summary>
    public JobCostCalcResult? Preview { get; set; }
}

public sealed class SheetJvVoucher
{
    public decimal FITH_SYS_ID { get; set; }
    public string? FITH_SRL_NO { get; set; }
    public DateTime FITH_DT { get; set; }
    public string? FITH_JRNL_TYPE { get; set; }
    public string? FITH_STATUS { get; set; }
    public string? FITH_NARRATION { get; set; }
    public string? PERIOD_STATUS { get; set; }
    public bool Missing { get; set; }
    public List<SheetJvLine> Lines { get; set; } = [];
}

public sealed class SheetJvLine
{
    public decimal FITD_SRL_NO { get; set; }
    public decimal FITD_VEL_CODE { get; set; }
    public string? FNAM_FNAM_CODE { get; set; }
    public string? FNAM_DESC { get; set; }
    public string? FITD_TA_CODE1 { get; set; }
    public string? FITD_JRNL_LINE_NARRATION { get; set; }
    public decimal FITD_BC_AMOUNT { get; set; }
}
