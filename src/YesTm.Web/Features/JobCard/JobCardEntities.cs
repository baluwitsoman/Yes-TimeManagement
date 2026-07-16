namespace YesTm.Web.Features.JobCard;

/// <summary>
/// Subset of the ERP job table MTL_TRANSACTION_JOB_OTHER_DTLS used by Job Card Entry.
/// Property names mirror the Oracle column names 1:1 (project convention).
/// </summary>
public sealed class MTL_TRANSACTION_JOB_OTHER_DTLS
{
    public string? MTJ_COMP_CODE { get; set; }
    public string MTJ_PARTY_IND { get; set; } = "AR";
    public string? MTJ_PARTY_CODE { get; set; }
    public string? MTJ_JOB_LOCATION { get; set; }
    public string? MTJ_BRAND { get; set; }
    public decimal? MTJ_EQUIPMENT_TYPE_ID { get; set; }
    public decimal? MTJ_SERVICE_TYPE_ID { get; set; }
    public string? MTJ_SERIAL_NO { get; set; }
    public string? MTJ_JOB_CODE { get; set; }
    public string? MTJ_JOB_DESC { get; set; }
    public DateTime? MTJ_JOB_OPENING_DATE { get; set; }
    public DateTime? MTJ_JOB_SERVICE_START_DATE { get; set; }
    public DateTime? MTJ_JOB_CLOSING_DATE { get; set; }
    public string? MTJ_JOB_STATUS { get; set; }
    public string? MTJ_CONTACT_PERSON { get; set; }
    public string? MTJ_CONTACT_PHONE { get; set; }
    public string? MTJ_SALES_EMPLOYEE_CODE { get; set; }
    public string? MTJ_SALES_EMPLOYEE_PHONE { get; set; }
    public string? MTJ_BRANCH { get; set; }
    public string? MTJ_SECTOR { get; set; }
    public string? MTJ_REMARKS { get; set; }
    public decimal? MTJ_CREATION_USER_ID { get; set; }
    public decimal? MTJ_UPDATE_USER_ID { get; set; }
}

/// <summary>Row for the Job Card list screen (with resolved display fields).</summary>
public sealed class JobCardListItem
{
    public string? MTJ_JOB_CODE { get; set; }
    public string? MTJ_JOB_DESC { get; set; }
    public string? CUSTOMER_NAME { get; set; }
    public string? BRAND_DESC { get; set; }
    public string? MTJ_JOB_LOCATION { get; set; }
    public string? STATUS_DESC { get; set; }
    public DateTime? MTJ_JOB_OPENING_DATE { get; set; }
}

public enum DeleteResult { Deleted, HasDependencies, NotFound, Failed }
