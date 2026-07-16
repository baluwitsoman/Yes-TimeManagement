using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class LabourRateModel : PageModel
{
    private readonly ILabourRateRepository _repo;
    private readonly IIndustryRepository _industries;
    private readonly ILogger<LabourRateModel> _logger;

    public LabourRateModel(ILabourRateRepository repo, IIndustryRepository industries, ILogger<LabourRateModel> logger)
    {
        _repo = repo;
        _industries = industries;
        _logger = logger;
    }

    public IReadOnlyList<TM_LABOUR_RATE> Items { get; private set; } = [];
    public IReadOnlyList<TM_INDUSTRY> Industries { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_LABOUR_RATE Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.LR_LOCATION) || Input.LR_RATE <= 0)
        {
            TempData["Error"] = "Location and a positive rate are required.";
            return RedirectToPage();
        }
        try
        {
            if (IsEdit) await _repo.UpdateAsync(Input, User.UserId(), ct);
            else await _repo.InsertAsync(Input, User.UserId(), ct);
            TempData["Success"] = "Labour rate saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save labour rate failed");
            TempData["Error"] = "Save failed.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(decimal id, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(id, active == "Y", ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle rate {Id} failed", id); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            Items = await _repo.GetAllAsync(ct);
            Industries = await _industries.GetAllAsync(ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Labour rate list unavailable"); IsOffline = true; }
    }
}
