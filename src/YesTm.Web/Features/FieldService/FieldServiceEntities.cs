namespace YesTm.Web.Features.FieldService;

/// <summary>
/// New table TM_FIELD_REPORT — the Field Service Report (paper FSR) for a field time sheet, 1:1 with
/// TM_TIME_SHEET (FR_TS_ID = TS_ID). The hours themselves are TM_TIME_LINE segment lines.
/// </summary>
public sealed class TM_FIELD_REPORT
{
    public decimal FR_TS_ID { get; set; }
    public string? FR_REPORT_NO { get; set; }
    public DateTime? FR_REPORT_DATE { get; set; }
    public string? FR_JOB_SITE { get; set; }
    public string? FR_CONTACT_PERSON { get; set; }
    public string? FR_CONTACT_POSITION { get; set; }
    public string? FR_CONTACT_PHONE { get; set; }
    public string? FR_OEM { get; set; }
    public string? FR_MODEL { get; set; }
    public decimal? FR_SMU_HOURS { get; set; }
    public string? FR_UNDER_WARRANTY_YN { get; set; }
    public string? FR_TAI_REPORT { get; set; }
    public string? FR_JOB_TYPE { get; set; }
    public string? FR_FLEET_NO { get; set; }
    public string? FR_PLATE_NO { get; set; }
    public string? FR_DRIVER_NAME { get; set; }
    public decimal? FR_KM_START { get; set; }
    public decimal? FR_KM_END { get; set; }
    public decimal FR_TOTAL_KM { get; set; }
    public decimal FR_KM_RATE { get; set; }
    public decimal FR_VEHICLE_COST { get; set; }
    public string? FR_COMPLAINT { get; set; }
    public string? FR_PROBLEMS_FOUND { get; set; }
    public string? FR_PROBLEM_CAUSES { get; set; }
    public string? FR_CORRECTION { get; set; }
    public string? FR_OTHER_ITEMS { get; set; }
    public string? FR_JOB_COMPLETED_YN { get; set; }
    public string? FR_TO_WORKSHOP_YN { get; set; }
    public string? FR_SERVICE_ENGINEER { get; set; }
    public string? FR_SERVICE_MANAGER { get; set; }
    public DateTime? FR_SIGN_DATE { get; set; }
    public string? FR_CUST_COMMENTS { get; set; }
    public string? FR_CUST_REP { get; set; }
    public string? FR_CUST_REP_POSITION { get; set; }
    public DateTime? FR_CUST_SIGN_DATE { get; set; }
    public decimal? FR_CREATION_USER_ID { get; set; }
    public DateTime? FR_CREATION_DATE { get; set; }
    public decimal? FR_UPDATE_USER_ID { get; set; }
    public DateTime? FR_UPDATE_DATE { get; set; }
}

/// <summary>New table TM_FIELD_REPORT_CREW — technicians on the field trip.</summary>
public sealed class TM_FIELD_REPORT_CREW
{
    public decimal FRC_TS_ID { get; set; }
    public decimal FRC_SEQ { get; set; }
    public string FRC_EMP_CODE { get; set; } = string.Empty;
    public string? FRC_EMP_NAME { get; set; }
    public string? FRC_FA_CODE { get; set; }
}
