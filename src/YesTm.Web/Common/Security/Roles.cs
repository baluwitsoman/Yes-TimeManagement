namespace YesTm.Web.Common.Security;

/// <summary>
/// Role codes stored in TM_USER_ROLE.ROLE_CODE and written as the user's role claim.
/// </summary>
public static class Roles
{
    /// <summary>Normal user — enters and views their own time bookings.</summary>
    public const string User = "USER";

    /// <summary>Full admin — enters all data and defines system parameters/masters.</summary>
    public const string Admin = "ADMIN";

    /// <summary>Site/System admin — manages who is a full admin; highest privilege.</summary>
    public const string SiteAdmin = "SITEADMIN";

    public static bool IsKnown(string? role) =>
        role is User or Admin or SiteAdmin;

    public static string DisplayName(string? role) => role switch
    {
        SiteAdmin => "Site Administrator",
        Admin => "Administrator",
        User => "User",
        _ => "User"
    };
}

/// <summary>Authorization policy names used across slices.</summary>
public static class Policies
{
    public const string AdminOrAbove = "AdminOrAbove";
    public const string SiteAdminOnly = "SiteAdminOnly";
}
