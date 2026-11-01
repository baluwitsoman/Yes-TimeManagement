using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YesTm.Web.Features.JobCostPosting;

/// <summary>
/// The finance JV(s) a time sheet has posted, read live from the ERP voucher tables, plus the cost
/// calculation behind them. Rendered without layout into the "Finance JV" modal of the entry screens.
/// </summary>
public class SheetJvModel : PageModel
{
    private readonly IJobCostPostingService _svc;
    private readonly ILogger<SheetJvModel> _logger;

    public SheetJvModel(IJobCostPostingService svc, ILogger<SheetJvModel> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    public SheetJvView View { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(decimal tsId, CancellationToken ct)
    {
        try { View = await _svc.GetSheetJvAsync(tsId, ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reading finance JV for sheet {TsId} failed", tsId);
            Error = "Finance vouchers could not be read (database unavailable).";
        }
        return Page();
    }
}
