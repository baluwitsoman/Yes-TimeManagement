using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;
using YesTm.Web.Features.Masters;

namespace YesTm.Web.Features.JobCard;

public class EntryModel : PageModel
{
    private readonly IJobCardRepository _repo;
    private readonly IJobLocationRepository _locations;
    private readonly ILogger<EntryModel> _logger;

    public EntryModel(IJobCardRepository repo, IJobLocationRepository locations, ILogger<EntryModel> logger)
    {
        _repo = repo;
        _locations = locations;
        _logger = logger;
    }

    [BindProperty] public MTL_TRANSACTION_JOB_OTHER_DTLS Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public IReadOnlyList<CodeName> Customers { get; private set; } = [];
    public IReadOnlyList<CodeName> Brands { get; private set; } = [];
    public IReadOnlyList<CodeName> EquipmentTypes { get; private set; } = [];
    public IReadOnlyList<CodeName> ServiceTypes { get; private set; } = [];
    public IReadOnlyList<CodeName> JobStatuses { get; private set; } = [];
    public IReadOnlyList<CodeName> Branches { get; private set; } = [];
    public IReadOnlyList<CodeName> Salesmen { get; private set; } = [];
    public IReadOnlyList<CodeName> Locations { get; private set; } = [];
    public bool IsOffline { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? jobCode, CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        if (!string.IsNullOrWhiteSpace(jobCode))
        {
            var existing = await _repo.GetAsync(jobCode, ct);
            if (existing is null)
            {
                TempData["Error"] = $"Job card '{jobCode}' was not found.";
                return RedirectToPage("/JobCard/Index");
            }
            Input = existing;
            IsEdit = true;
        }
        else
        {
            Input.MTJ_JOB_LOCATION = "Field";
            Input.MTJ_JOB_OPENING_DATE = DateTime.Today;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Input.MTJ_PARTY_CODE)) errors.Add("Customer is required.");
        if (string.IsNullOrWhiteSpace(Input.MTJ_JOB_LOCATION)) errors.Add("Job Location is required.");
        if (string.IsNullOrWhiteSpace(Input.MTJ_BRAND)) errors.Add("Brand is required.");
        if (Input.MTJ_EQUIPMENT_TYPE_ID is null) errors.Add("Equipment Type is required.");
        if (Input.MTJ_SERVICE_TYPE_ID is null) errors.Add("Type of Service is required.");
        if (string.IsNullOrWhiteSpace(Input.MTJ_JOB_DESC)) errors.Add("Job Description is required.");
        if (Input.MTJ_JOB_OPENING_DATE is null) errors.Add("Job Opening Date is required.");
        if (string.IsNullOrWhiteSpace(Input.MTJ_JOB_STATUS)) errors.Add("Job Status is required.");
        if ((Input.MTJ_JOB_DESC?.Length ?? 0) > 200) errors.Add("Job Description cannot exceed 200 characters.");
        if ((Input.MTJ_REMARKS?.Length ?? 0) > 1000) errors.Add("Remarks cannot exceed 1000 characters.");

        if (errors.Count > 0)
        {
            foreach (var e in errors) ModelState.AddModelError(string.Empty, e);
            return Page();
        }

        try
        {
            if (IsEdit)
            {
                await _repo.UpdateAsync(Input, User.UserId(), ct);
                TempData["Success"] = $"Job card {Input.MTJ_JOB_CODE} updated.";
            }
            else
            {
                Input.MTJ_JOB_CODE = await _repo.GenerateJobCodeAsync(ct);
                await _repo.InsertAsync(Input, User.UserId(), ct);
                TempData["Success"] = $"Job card {Input.MTJ_JOB_CODE} created.";
            }
            return RedirectToPage("/JobCard/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save job card failed (edit={IsEdit})", IsEdit);
            ModelState.AddModelError(string.Empty, "Unable to save the job card. Please try again or contact your administrator.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string jobCode, CancellationToken ct)
    {
        try
        {
            var result = await _repo.DeleteAsync(jobCode, ct);
            switch (result)
            {
                case DeleteResult.Deleted:
                    TempData["Success"] = $"Job card {jobCode} deleted.";
                    return RedirectToPage("/JobCard/Index");
                case DeleteResult.HasDependencies:
                    TempData["Error"] = "This job card has time bookings or ERP transactions and cannot be deleted.";
                    break;
                case DeleteResult.NotFound:
                    TempData["Error"] = "Job card not found.";
                    break;
                default:
                    TempData["Error"] = "Unable to delete the job card.";
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete job card {JobCode} failed", jobCode);
            TempData["Error"] = "Unable to delete the job card.";
        }
        return RedirectToPage("/JobCard/Entry", new { jobCode });
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            Customers = await _repo.GetCustomersAsync(ct);
            Brands = await _repo.GetBrandsAsync(ct);
            EquipmentTypes = await _repo.GetEquipmentTypesAsync(ct);
            ServiceTypes = await _repo.GetServiceTypesAsync(ct);
            JobStatuses = await _repo.GetJobStatusesAsync(ct);
            Branches = await _repo.GetBranchesAsync(ct);
            Salesmen = await _repo.GetSalesmenAsync(ct);
            Locations = await _locations.GetActiveAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job card lookups unavailable");
            IsOffline = true;
        }
    }
}
