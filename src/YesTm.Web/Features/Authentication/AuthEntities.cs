namespace YesTm.Web.Features.Authentication;

/// <summary>
/// Subset of the existing ERP login table AMM_USER_DETAILS.
/// Property names intentionally match the Oracle column names 1:1 for maintainability.
/// </summary>
public sealed class AMM_USER_DETAILS
{
    public decimal USER_ID { get; set; }
    public string? USER_CODE { get; set; }
    public string? USER_NAME { get; set; }
    public string? USER_SHORT_NAME { get; set; }
    public string? USER_PASSWORD { get; set; }
    public string? USER_EMP_CODE { get; set; }
    public string? USER_EMAIL_ID { get; set; }
    public string? USER_DEFAULT_LOCATION { get; set; }
    public string? USER_ACTIVE_YN { get; set; }
    public DateTime? USER_ACTIVE_FROM_DATE { get; set; }
    public DateTime? USER_ACTIVE_TO_DATE { get; set; }
}

/// <summary>
/// New table TM_USER_ROLE — assigns an application role to an ERP user.
/// The site admin maintains rows here to decide who is a full admin.
/// </summary>
public sealed class TM_USER_ROLE
{
    public decimal TUR_ID { get; set; }
    public decimal TUR_USER_ID { get; set; }
    public string TUR_ROLE_CODE { get; set; } = "USER";
    public string TUR_ACTIVE_YN { get; set; } = "Y";
    public decimal? TUR_CREATION_USER_ID { get; set; }
    public DateTime? TUR_CREATION_DATE { get; set; }
    public decimal? TUR_UPDATE_USER_ID { get; set; }
    public DateTime? TUR_UPDATE_DATE { get; set; }
}
