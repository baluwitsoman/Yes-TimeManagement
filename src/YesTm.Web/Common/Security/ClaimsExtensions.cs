using System.Security.Claims;

namespace YesTm.Web.Common.Security;

public static class ClaimsExtensions
{
    /// <summary>Numeric AMM_USER_DETAILS.USER_ID from the auth cookie (0 if absent).</summary>
    public static decimal UserId(this ClaimsPrincipal user) =>
        decimal.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0m;

    /// <summary>
    /// The user's employee code (AMM_USER_DETAILS.USER_EMP_CODE), carried in the "emp_code" claim.
    /// Same code space as PPM_EMPLOYEE_DETAILS.PEMP_EMP_CODE / TM_TIME_LINE.TL_TECH_CODE, so it
    /// identifies whether the user is a technician on a booking. Null/empty when not set.
    /// </summary>
    public static string? EmpCode(this ClaimsPrincipal user)
    {
        var code = user.FindFirstValue("emp_code");
        return string.IsNullOrWhiteSpace(code) ? null : code;
    }
}
