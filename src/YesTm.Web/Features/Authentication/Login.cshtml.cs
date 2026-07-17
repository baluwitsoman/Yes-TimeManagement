using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Authentication;

public class LoginModel : PageModel
{
    private readonly IUserRepository _users;
    private readonly ILogger<LoginModel> _logger;
    private readonly IWebHostEnvironment _env;

    public LoginModel(IUserRepository users, ILogger<LoginModel> logger, IWebHostEnvironment env)
    {
        _users = users;
        _logger = logger;
        _env = env;
    }

    /// <summary>Demo sign-in buttons are shown only in the Development environment.</summary>
    public bool IsDevelopment => _env.IsDevelopment();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "User name is required.")]
        [Display(Name = "User Name")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet(string? error = null)
    {
        if (error == "autologin")
            ErrorMessage = "Your single sign-on link has expired or was already used. Please sign in.";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return Page();

        AMM_USER_DETAILS? user;
        try
        {
            user = await _users.ValidateCredentialsAsync(Input.UserName, Input.Password, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed due to a data-access error for {UserName}", Input.UserName);
            ErrorMessage = "Unable to reach the authentication service. Please contact your administrator.";
            return Page();
        }

        if (user is null)
        {
            ErrorMessage = "Invalid user name or password.";
            return Page();
        }

        var role = await _users.GetEffectiveRoleAsync(user.USER_ID, ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.USER_ID.ToString("0")),
            new(ClaimTypes.Name, user.USER_NAME ?? Input.UserName),
            new("user_code", user.USER_CODE ?? string.Empty),
            new("emp_code", user.USER_EMP_CODE ?? string.Empty),
            new("location", user.USER_DEFAULT_LOCATION ?? string.Empty),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        _logger.LogInformation("User {UserName} (id {UserId}) signed in as {Role}", user.USER_NAME, user.USER_ID, role);

        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            return LocalRedirect(ReturnUrl);

        return RedirectToPage("/Dashboard/Index");
    }

    /// <summary>
    /// Development-only sign-in that bypasses the database so the UI can be reviewed
    /// before the Oracle connection is configured. Disabled outside Development.
    /// </summary>
    public async Task<IActionResult> OnPostDemoAsync(string role, CancellationToken ct)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var effectiveRole = Roles.IsKnown(role) ? role : Roles.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "9001"),
            new(ClaimTypes.Name, $"demo.{effectiveRole.ToLowerInvariant()}"),
            new("user_code", "DEMO"),
            new(ClaimTypes.Role, effectiveRole)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        _logger.LogWarning("DEV demo sign-in as {Role}", effectiveRole);
        return RedirectToPage("/Dashboard/Index");
    }
}
