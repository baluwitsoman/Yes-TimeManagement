using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class JobLocationModel : PageModel
{
    private readonly IJobLocationRepository _repo;
    private readonly ILogger<JobLocationModel> _logger;

    public JobLocationModel(IJobLocationRepository repo, ILogger<JobLocationModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<TM_JOB_LOCATION> Items { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_JOB_LOCATION Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.LOC_CODE) || string.IsNullOrWhiteSpace(Input.LOC_NAME))
        {
            TempData["Error"] = "Code and Name are required.";
            return RedirectToPage();
        }
        try
        {
            Input.LOC_CODE = Input.LOC_CODE.Trim();
            await _repo.UpsertAsync(Input, User.UserId(), !IsEdit, ct);
            TempData["Success"] = $"Job location '{Input.LOC_CODE}' saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save job location {Code} failed", Input.LOC_CODE);
            TempData["Error"] = IsEdit ? "Update failed." : "Save failed — the code may already exist.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(string code, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(code, active == "Y", User.UserId(), ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle job location {Code} failed", code); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try { Items = await _repo.GetAllAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Job location list unavailable"); IsOffline = true; }
    }
}
