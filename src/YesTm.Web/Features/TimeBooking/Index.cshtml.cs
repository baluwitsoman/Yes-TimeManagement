using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.TimeBooking;

public class IndexModel : PageModel
{
    private readonly ITimeBookingRepository _repo;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ITimeBookingRepository repo, ILogger<IndexModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<TimeSheetListItem> Sheets { get; private set; } = [];
    public bool IsOffline { get; private set; }
    public bool ScopedToMe { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Admins see everything; normal users see only their own postings.
        var isAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);
        ScopedToMe = !isAdmin;
        decimal? scope = null;
        if (ScopedToMe && decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            scope = id;

        try
        {
            Sheets = await _repo.GetRecentTimeSheetsAsync(scope, 50, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Time sheet list unavailable (database offline)");
            IsOffline = true;
        }
    }
}
