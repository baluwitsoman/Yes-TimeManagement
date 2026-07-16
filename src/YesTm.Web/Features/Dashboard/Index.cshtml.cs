using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Dashboard;

public class IndexModel : PageModel
{
    private readonly IDashboardRepository _repo;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IDashboardRepository repo, ILogger<IndexModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public DashboardSummary Summary { get; private set; } = new();
    public bool IsOffline { get; private set; }
    public bool ScopedToMe { get; private set; }
    public string UserName => User.Identity?.Name ?? "User";

    /// <summary>Monthly available-hours benchmark from the solution document (192 hr/month).</summary>
    public const decimal MonthlyBenchmark = 192m;

    public decimal Utilisation => MonthlyBenchmark <= 0 ? 0
        : Math.Round(Summary.TOTAL_NET_HOURS / MonthlyBenchmark * 100m, 1);

    public decimal Efficiency => Summary.TOTAL_NET_HOURS <= 0 ? 0
        : Math.Round(Summary.STD_HOURS / Summary.TOTAL_NET_HOURS * 100m, 1);

    public async Task OnGetAsync(CancellationToken ct)
    {
        var isAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);
        ScopedToMe = !isAdmin;
        decimal? scope = null;
        if (ScopedToMe && decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            scope = id;

        try
        {
            Summary = await _repo.GetMonthSummaryAsync(scope, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Dashboard summary unavailable (database offline)");
            IsOffline = true;
        }
    }
}
