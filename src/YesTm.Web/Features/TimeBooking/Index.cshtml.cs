using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
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

    public const int PageSize = 20;

    public IReadOnlyList<TimeSheetListItem> Sheets { get; private set; } = [];
    public bool IsOffline { get; private set; }
    public bool ScopedToMe { get; private set; }
    public bool IsAdmin { get; private set; }

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNo { get; set; } = 1;

    public int TotalCount { get; private set; }
    public int TotalPages => TotalCount == 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Admins see everything; normal users see only their own postings.
        IsAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);
        ScopedToMe = !IsAdmin;
        // Non-admins see only sheets they are involved in — created themselves, or where their
        // employee code is a technician on a line. Admins pass null scope (see all).
        decimal? scope = ScopedToMe ? User.UserId() : null;
        string? scopeEmp = ScopedToMe ? User.EmpCode() : null;

        if (PageNo < 1) PageNo = 1;

        try
        {
            var result = await _repo.GetTimeSheetsAsync(scope, scopeEmp, Search, DateFrom, DateTo, PageNo, PageSize, ct);
            Sheets = result.Items;
            TotalCount = result.TotalCount;
            if (PageNo > TotalPages) PageNo = TotalPages;   // clamp after a filter shrinks the set
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Time sheet list unavailable (database offline)");
            IsOffline = true;
        }
    }
}
