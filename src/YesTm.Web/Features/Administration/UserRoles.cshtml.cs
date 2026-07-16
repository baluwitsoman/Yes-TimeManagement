using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Administration;

[Authorize(Policy = Policies.SiteAdminOnly)]
public class UserRolesModel : PageModel
{
    private readonly IAdministrationRepository _repo;
    private readonly ILogger<UserRolesModel> _logger;

    public UserRolesModel(IAdministrationRepository repo, ILogger<UserRolesModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<UserRoleListItem> Users { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public static readonly string[] AssignableRoles = [Roles.User, Roles.Admin, Roles.SiteAdmin];

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSetRoleAsync(decimal userId, string role, CancellationToken ct)
    {
        if (!Roles.IsKnown(role))
        {
            TempData["Error"] = "Unknown role selected.";
            return RedirectToPage(new { Search });
        }

        var actingUserId = decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0m;
        try
        {
            await _repo.SetUserRoleAsync(userId, role, actingUserId, ct);
            TempData["Success"] = $"Role updated to {Roles.DisplayName(role)}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Set role failed for user {UserId}", userId);
            TempData["Error"] = "Unable to update the role (database unavailable).";
        }
        return RedirectToPage(new { Search });
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            Users = await _repo.GetUsersWithRolesAsync(Search, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User list unavailable (database offline)");
            IsOffline = true;
        }
    }
}
