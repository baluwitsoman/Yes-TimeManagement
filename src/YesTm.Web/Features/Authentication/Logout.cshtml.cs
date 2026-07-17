using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YesTm.Web.Features.Authentication;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;
    private readonly IConfiguration _config;

    public LogoutModel(ILogger<LogoutModel> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public IActionResult OnGet() => RedirectToPage("/Dashboard/Index");

    public async Task<IActionResult> OnPostAsync()
    {
        var name = User.Identity?.Name;
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User {UserName} signed out", name);

        // Time Management is entered through the legacy ERP's central login, so sign-out
        // returns the user there. Falls back to the local login page when no ERP URL is
        // configured (e.g. in development).
        var erpLoginUrl = _config["TimeManagement:ErpLoginUrl"];
        if (!string.IsNullOrWhiteSpace(erpLoginUrl))
            return Redirect(erpLoginUrl);

        return RedirectToPage("/Authentication/Login");
    }
}
