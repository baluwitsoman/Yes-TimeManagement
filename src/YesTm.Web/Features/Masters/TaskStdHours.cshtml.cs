using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class TaskStdHoursModel : PageModel
{
    private readonly ITaskStdRepository _repo;
    private readonly ITaskRepository _tasks;
    private readonly ILogger<TaskStdHoursModel> _logger;

    public TaskStdHoursModel(ITaskStdRepository repo, ITaskRepository tasks, ILogger<TaskStdHoursModel> logger)
    {
        _repo = repo;
        _tasks = tasks;
        _logger = logger;
    }

    public IReadOnlyList<TM_TASK_STD> Items { get; private set; } = [];
    public IReadOnlyList<CodeName> Tasks { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_TASK_STD Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.TST_TASK_CODE) || Input.TST_STD_HOURS <= 0)
        {
            TempData["Error"] = "Task and a positive standard-hours value are required.";
            return RedirectToPage();
        }
        try
        {
            if (IsEdit) await _repo.UpdateAsync(Input, User.UserId(), ct);
            else await _repo.InsertAsync(Input, User.UserId(), ct);
            TempData["Success"] = "Task standard hours saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save task std hours failed");
            TempData["Error"] = "Save failed.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(decimal id, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(id, active == "Y", ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle task std {Id} failed", id); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            Items = await _repo.GetAllAsync(ct);
            Tasks = await _tasks.GetActiveCodeNamesAsync(ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Task std list unavailable"); IsOffline = true; }
    }
}
