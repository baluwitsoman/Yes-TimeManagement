using System.Security.Claims;

namespace YesTm.Web.Common.Security;

public static class ClaimsExtensions
{
    /// <summary>Numeric AMM_USER_DETAILS.USER_ID from the auth cookie (0 if absent).</summary>
    public static decimal UserId(this ClaimsPrincipal user) =>
        decimal.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0m;
}
