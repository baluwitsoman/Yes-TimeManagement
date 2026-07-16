using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YesTm.Web.Features.JobCard;

public class IndexModel : PageModel
{
    private readonly IJobCardRepository _repo;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IJobCardRepository repo, ILogger<IndexModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<JobCardListItem> Items { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Items = await _repo.GetListAsync(Search, 100, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job card list unavailable");
            IsOffline = true;
        }
    }
}
