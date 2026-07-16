using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;

namespace YesTm.Web.Features.Masters;

public class CustomerIndustryModel : PageModel
{
    private readonly ICustomerIndustryRepository _repo;
    private readonly IIndustryRepository _industries;
    private readonly ILogger<CustomerIndustryModel> _logger;

    public CustomerIndustryModel(ICustomerIndustryRepository repo, IIndustryRepository industries, ILogger<CustomerIndustryModel> logger)
    {
        _repo = repo;
        _industries = industries;
        _logger = logger;
    }

    public IReadOnlyList<TM_CUST_INDUSTRY_MAP> Items { get; private set; } = [];
    public IReadOnlyList<TM_INDUSTRY> Industries { get; private set; } = [];
    public IReadOnlyList<CodeName> Customers { get; private set; } = [];
    public bool IsOffline { get; private set; }

    [BindProperty] public TM_CUST_INDUSTRY_MAP Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.CIM_CUSTOMER_CODE) || string.IsNullOrWhiteSpace(Input.CIM_INDUSTRY_CODE))
        {
            TempData["Error"] = "Customer and Industry are required.";
            return RedirectToPage();
        }
        try
        {
            if (IsEdit) await _repo.UpdateAsync(Input, User.UserId(), ct);
            else await _repo.InsertAsync(Input, User.UserId(), ct);
            TempData["Success"] = "Customer → Industry mapping saved.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Save customer-industry map failed");
            TempData["Error"] = "Save failed.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(decimal id, string active, CancellationToken ct)
    {
        try { await _repo.SetActiveAsync(id, active == "Y", ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle map {Id} failed", id); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            Items = await _repo.GetAllAsync(ct);
            Industries = await _industries.GetAllAsync(ct);
            Customers = await _repo.GetCustomersAsync(ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Customer-industry list unavailable"); IsOffline = true; }
    }
}
