using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class WorkTypeModel : PageModel
{
    private readonly IWorkTypeRepository _repo;
    private readonly ILogger<WorkTypeModel> _logger;

    public WorkTypeModel(IWorkTypeRepository repo, ILogger<WorkTypeModel> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public IReadOnlyList<TM_WORK_TYPE> Items { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_WORK_TYPE Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.WORK_TYPE_CODE) || string.IsNullOrWhiteSpace(Input.WORK_TYPE_NAME))
        {
            TempData["Error"] = "Code and Name are required.";
            return RedirectToPage();
        }
        try
        {
            Input.WORK_TYPE_CODE = Input.WORK_TYPE_CODE.Trim().ToUpperInvariant();
            await _repo.UpsertAsync(Input, User.UserId(), !IsEdit, ct);
            TempData["Success"] = $"Work type '{Input.WORK_TYPE_CODE}' saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save work type {Code} failed", Input.WORK_TYPE_CODE);
            TempData["Error"] = IsEdit ? "Update failed." : "Save failed — the code may already exist.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(string code, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(code, active == "Y", User.UserId(), ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle work type {Code} failed", code); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try { Items = await _repo.GetAllAsync(ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Work type list unavailable"); IsOffline = true; }
    }
}
