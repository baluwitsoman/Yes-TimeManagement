using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class IndustryModel : PageModel
{
    private readonly IIndustryRepository _repo;
    private readonly ILogger<IndustryModel> _logger;

    public IndustryModel(IIndustryRepository repo, ILogger<IndustryModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<TM_INDUSTRY> Items { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_INDUSTRY Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.INDUSTRY_CODE) || string.IsNullOrWhiteSpace(Input.INDUSTRY_NAME))
        {
            TempData["Error"] = "Code and Name are required.";
            return RedirectToPage();
        }
        try
        {
            Input.INDUSTRY_CODE = Input.INDUSTRY_CODE.Trim().ToUpperInvariant();
            await _repo.UpsertAsync(Input, User.UserId(), !IsEdit, ct);
            TempData["Success"] = $"Industry '{Input.INDUSTRY_CODE}' saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save industry {Code} failed", Input.INDUSTRY_CODE);
            TempData["Error"] = IsEdit ? "Update failed." : "Save failed — the code may already exist.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(string code, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(code, active == "Y", User.UserId(), ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle industry {Code} failed", code); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try { Items = await _repo.GetAllAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Industry list unavailable"); IsOffline = true; }
    }
}
