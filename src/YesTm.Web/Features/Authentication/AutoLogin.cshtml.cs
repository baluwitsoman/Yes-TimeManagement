using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Authentication;

/// <summary>
/// Single sign-on landing endpoint. The legacy ERP redirects here with a one-time
/// token after the user picks a Time-Management role at the ERP login; we redeem the
/// token, auto-provision the TM role if needed, and sign the user in with a cookie.
/// </summary>
[AllowAnonymous]
public class AutoLoginModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly ILogger<AutoLoginModel> _logger;

    public AutoLoginModel(IUserRepository users, ILogger<AutoLoginModel> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(string? token, CancellationToken ct)
    {
        AutoLoginTicket? ticket;
        try
        {
            ticket = await _users.ConsumeAutoLoginTokenAsync(token ?? string.Empty, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-login failed due to a data-access error");
            return RedirectToLoginWithError();
        }

        if (ticket is null)
            return RedirectToLoginWithError();

        var (userId, erpRoleName) = ticket.Value;

        if (!Roles.TryMapErpRole(erpRoleName, out var tmRole))
        {
            _logger.LogWarning("Auto-login token carried non-TM role {ErpRole} for user {UserId}", erpRoleName, userId);
            return RedirectToLoginWithError();
        }

        // Create the assignment only if the user has none yet (respects SITEADMIN's choices).
        await _users.EnsureRoleAsync(userId, tmRole, ct);

        var user = await _users.GetUserByIdAsync(userId, ct);
        if (user is null)
        {
            _logger.LogWarning("Auto-login token referenced unknown/inactive user {UserId}", userId);
            return RedirectToLoginWithError();
        }

        // Session role = the user's effective (highest active) role, matching manual login.
        var effectiveRole = await _users.GetEffectiveRoleAsync(userId, ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.USER_ID.ToString("0")),
            new(ClaimTypes.Name, user.USER_NAME ?? string.Empty),
            new("user_code", user.USER_CODE ?? string.Empty),
            new("emp_code", user.USER_EMP_CODE ?? string.Empty),
            new("location", user.USER_DEFAULT_LOCATION ?? string.Empty),
            new(ClaimTypes.Role, effectiveRole)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        _logger.LogInformation("Auto-login: user {UserName} (id {UserId}) signed in as {Role} via ERP role {ErpRole}",
            user.USER_NAME, userId, effectiveRole, erpRoleName);

        return RedirectToPage("/Dashboard/Index");
    }

    private IActionResult RedirectToLoginWithError() =>
        RedirectToPage("/Authentication/Login", new { error = "autologin" });
}
