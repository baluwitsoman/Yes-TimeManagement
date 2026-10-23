using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class NonJobActivityModel : PageModel
{
    private readonly INonJobActivityRepository _repo;
    private readonly ILogger<NonJobActivityModel> _logger;

    public NonJobActivityModel(INonJobActivityRepository repo, ILogger<NonJobActivityModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<TM_NONJOB_ACTIVITY> Items { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_NONJOB_ACTIVITY Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.NJ_CODE) || string.IsNullOrWhiteSpace(Input.NJ_NAME)
            || !NonJobCategories.All.Contains(Input.NJ_CATEGORY))
        {
            TempData["Error"] = "Code, Name and Category are required.";
            return RedirectToPage();
        }
        try
        {
            // Prefix keeps non-job codes apart from ERP job numbers in TS_JOB_CODE.
            var code = Input.NJ_CODE.Trim().ToUpperInvariant();
            Input.NJ_CODE = code.StartsWith("NJ-", StringComparison.Ordinal) ? code : "NJ-" + code;
            Input.NJ_PAID_YN = Input.NJ_PAID_YN == "N" ? "N" : "Y";
            await _repo.UpsertAsync(Input, User.UserId(), !IsEdit, ct);
            TempData["Success"] = $"Non-job activity '{Input.NJ_CODE}' saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save non-job activity {Code} failed", Input.NJ_CODE);
            TempData["Error"] = IsEdit ? "Update failed." : "Save failed — the code may already exist.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(string code, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(code, active == "Y", User.UserId(), ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle non-job activity {Code} failed", code); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try { Items = await _repo.GetAllAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Non-job activity list unavailable (run db/11_nonjob_activities.sql?)"); IsOffline = true; }
    }
}
