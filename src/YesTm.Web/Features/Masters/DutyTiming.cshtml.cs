using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class DutyTimingModel : PageModel
{
    private readonly IDutyTimingRepository _repo;
    private readonly ILogger<DutyTimingModel> _logger;

    public DutyTimingModel(IDutyTimingRepository repo, ILogger<DutyTimingModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<TM_DUTY_TIMING> Items { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_DUTY_TIMING Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.DT_START) || string.IsNullOrWhiteSpace(Input.DT_END))
        {
            TempData["Error"] = "Duty start and end are required.";
            return RedirectToPage();
        }
        try
        {
            if (IsEdit) await _repo.UpdateAsync(Input, User.UserId(), ct);
            else await _repo.InsertAsync(Input, User.UserId(), ct);
            TempData["Success"] = "Duty timing saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save duty timing failed");
            TempData["Error"] = "Save failed.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(decimal id, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(id, active == "Y", ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle duty timing {Id} failed", id); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try { Items = await _repo.GetAllAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Duty timing list unavailable"); IsOffline = true; }
    }
}
