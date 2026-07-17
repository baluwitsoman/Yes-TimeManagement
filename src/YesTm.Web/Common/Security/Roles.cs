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

    /// <summary>
    /// Maps a legacy ERP role name (AMM_ROLE_DETAILS.ROLE_NAME) to a TM role code.
    /// These three names are the Time-Management roles the ERP login hands off with;
    /// the mapping is intentionally hardcoded (single source of truth in the TM app).
    /// Returns false for any role that is not a Time-Management role.
    /// </summary>
    public static bool TryMapErpRole(string? erpRoleName, out string tmRole)
    {
        tmRole = erpRoleName switch
        {
            "TimeManagement" => User,
            "TM-Admin"       => Admin,
            "TM-SiteAdmin"   => SiteAdmin,
            _                => string.Empty
        };
        return tmRole.Length > 0;
    }

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
