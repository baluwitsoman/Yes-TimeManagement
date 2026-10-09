namespace YesTm.Web.Features.Hrms;

/// <summary>"TimeManagement:Hrms" settings — where the HRMS feed lives and how often to pull it.</summary>
public sealed class HrmsOptions
{
    public const string Section = "TimeManagement:Hrms";

    /// <summary>HRMS site root incl. path base, e.g. https://hrms.example.com/ERP</summary>
    public string? BaseUrl { get; set; }
    /// <summary>HRMS company short code (AMM_COMPANY_DETAILS.ACD_COMP_SHORT_NAME) of company 39.</summary>
    public string? CompanyShortCode { get; set; }
    /// <summary>Shared secret, sent as X-Api-Key (HRMS config TmSync:ApiKey).</summary>
    public string? ApiKey { get; set; }
    /// <summary>Run the nightly sync at this local hour (0-23); null/negative = no schedule.</summary>
    public int? NightlySyncHour { get; set; } = 2;
    /// <summary>The nightly sync re-pulls this many past days (attendance gets corrected late).</summary>
    public int SyncDaysBack { get; set; } = 35;
    public int TimeoutSeconds { get; set; } = 120;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(CompanyShortCode) && !string.IsNullOrWhiteSpace(ApiKey);
}

// -------- Payloads from HRMS GET {short}/api/TmSync/... (property names = HRMS JSON keys) --------

public sealed class HrmsEmployeeDto
{
    public string? EMP_CODE { get; set; }
    public string? EMPLOYMENT_NO { get; set; }
    public string? EMP_NAME { get; set; }
    public string? ACTIVE { get; set; }
    public string? BRANCH_CODE { get; set; }
    public string? DEPT_NAME { get; set; }
    public string? DESIG_NAME { get; set; }
}

public sealed class HrmsAttendanceDto
{
    public string? EMP_CODE { get; set; }
    public DateTime ATT_DATE { get; set; }
    public string? DAY_STATUS { get; set; }
    public string? LEAVE_ID { get; set; }
    public string? LEAVE_NAME { get; set; }
    public string? HOL_NAME { get; set; }
    public decimal? LEAVE_COUNT { get; set; }
    public decimal? HOLIDAY_COUNT { get; set; }
    public decimal? ABSENT_FACTOR { get; set; }
    public string? SHIFT_TOT_HOURS { get; set; }
    public decimal? ACTUAL_TOT_MINUTES { get; set; }
    public decimal? OT_HOURS { get; set; }
    public DateTime? FIRST_IN { get; set; }
    public DateTime? LAST_OUT { get; set; }
}

// -------- TM tables (10_hrms_attendance.sql) --------

/// <summary>New table TM_HRMS_EMPLOYEE — HRMS employee list cached for mapping.</summary>
public sealed class TM_HRMS_EMPLOYEE
{
    public string HE_EMP_CODE { get; set; } = string.Empty;
    public string? HE_EMPLOYMENT_NO { get; set; }
    public string? HE_EMP_NAME { get; set; }
    public string? HE_ACTIVE { get; set; }
    public string? HE_BRANCH_CODE { get; set; }
    public string? HE_DEPT_NAME { get; set; }
    public string? HE_DESIG_NAME { get; set; }
    public DateTime? HE_SYNC_DATE { get; set; }
}

/// <summary>New table TM_EMP_CODE_MAP — TM technician code → HRMS employee code.</summary>
public sealed class TM_EMP_CODE_MAP
{
    public string ECM_TM_EMP_CODE { get; set; } = string.Empty;
    public string? ECM_HRMS_EMP_CODE { get; set; }
    public string? ECM_MATCH_TYPE { get; set; }
    public decimal? ECM_CONFIDENCE { get; set; }
    public string? ECM_STATUS { get; set; }
    public decimal? ECM_UPDATE_USER_ID { get; set; }
    public DateTime? ECM_UPDATE_DATE { get; set; }
}

public static class MapStatus
{
    public const string Suggested = "SUGGESTED";
    public const string Confirmed = "CONFIRMED";
    public const string Rejected = "REJECTED";
}

/// <summary>New table TM_ATTENDANCE — one HRMS employee-day.</summary>
public sealed class TM_ATTENDANCE
{
    public string ATT_HRMS_EMP_CODE { get; set; } = string.Empty;
    public DateTime ATT_DATE { get; set; }
    public string? ATT_DAY_STATUS { get; set; }
    public string? ATT_LEAVE_ID { get; set; }
    public string? ATT_LEAVE_NAME { get; set; }
    public string? ATT_HOLIDAY_NAME { get; set; }
    public decimal ATT_LEAVE_DAYS { get; set; }
    public decimal ATT_HOLIDAY_DAYS { get; set; }
    public decimal ATT_ABSENT_DAYS { get; set; }
    public decimal ATT_SHIFT_HOURS { get; set; }
    public decimal ATT_ACTUAL_HOURS { get; set; }
    public decimal ATT_OT_HOURS { get; set; }
    public DateTime? ATT_FIRST_IN { get; set; }
    public DateTime? ATT_LAST_OUT { get; set; }
    public decimal? ATT_SYNC_ID { get; set; }
}

/// <summary>New table TM_SYNC_LOG.</summary>
public sealed class TM_SYNC_LOG
{
    public decimal SL_ID { get; set; }
    public string? SL_TRIGGER { get; set; }
    public DateTime? SL_FROM_DATE { get; set; }
    public DateTime? SL_TO_DATE { get; set; }
    public DateTime? SL_START { get; set; }
    public DateTime? SL_END { get; set; }
    public string? SL_STATUS { get; set; }
    public decimal SL_EMPLOYEES { get; set; }
    public decimal SL_ATT_ROWS { get; set; }
    public decimal SL_NEW_MATCHES { get; set; }
    public string? SL_MESSAGE { get; set; }
    public decimal? SL_USER_ID { get; set; }
}

/// <summary>One row of the mapping screen: a TM technician and its (suggested / confirmed) HRMS employee.</summary>
public sealed class EmpMapRow
{
    public string TM_EMP_CODE { get; set; } = string.Empty;
    public string? TM_EMP_NAME { get; set; }
    /// <summary>Task lines booked by this technician (technicians who book time come first).</summary>
    public int BOOKED_LINES { get; set; }
    public string? ECM_HRMS_EMP_CODE { get; set; }
    public string? HRMS_EMP_NAME { get; set; }
    public string? HRMS_EMPLOYMENT_NO { get; set; }
    public string? ECM_MATCH_TYPE { get; set; }
    public decimal? ECM_CONFIDENCE { get; set; }
    public string? ECM_STATUS { get; set; }
}
