using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;
using YesTm.Web.Features.Masters;

namespace YesTm.Web.Features.JobCostPosting;

/// <summary>
/// Job Cost Posting Setup (admin+): switches the time-sheet → finance JV posting on, and holds the
/// cost rates with the GL accounts (VEL codes) each one debits (job cost) and credits (absorbed).
/// </summary>
public class SetupModel : PageModel
{
    private readonly IJobCostPostingService _svc;
    private readonly IJobLocationRepository _locations;
    private readonly ILogger<SetupModel> _logger;

    public SetupModel(IJobCostPostingService svc, IJobLocationRepository locations, ILogger<SetupModel> logger)
    {
        _svc = svc;
        _locations = locations;
        _logger = logger;
    }

    public bool SchemaReady { get; private set; } = true;
    public bool IsOffline { get; private set; }
    public TM_JCP_SETUP Current { get; private set; } = new();
    public IReadOnlyList<TM_JCP_COST_CODE> Codes { get; private set; } = [];
    public IReadOnlyList<GlAccountLookup> Accounts { get; private set; } = [];
    public IReadOnlyList<string> JournalTypes { get; private set; } = [];
    public IReadOnlyList<CodeName> Locations { get; private set; } = [];
    /// <summary>What still blocks a clean posting (missing accounts / rates) — shown above the grid.</summary>
    public List<string> Gaps { get; } = [];
    /// <summary>Non-blocking: locations without a labour rate (their hours are reported as "not posted").</summary>
    public List<string> Notes { get; } = [];

    [BindProperty] public SetupInput Form { get; set; } = new();
    [BindProperty] public TM_JCP_COST_CODE Input { get; set; } = new();
    [BindProperty] public bool IsEdit { get; set; }

    public class SetupInput
    {
        public bool Enabled { get; set; }
        public string CompCode { get; set; } = "01";
        public string OffCode { get; set; } = "01";
        public string JrnlType { get; set; } = "JRV";
        public DateTime? StartDate { get; set; }
    }

    public string AccountName(decimal? vel) =>
        vel is null ? "—" : Accounts.FirstOrDefault(a => a.FNAM_VEL_CODE == vel)?.Display ?? $"VEL {vel} (not found)";

    public async Task OnGetAsync(CancellationToken ct) => await LoadAsync(ct);

    public async Task<IActionResult> OnPostSaveSetupAsync(CancellationToken ct)
    {
        try
        {
            if (Form.Enabled)
            {
                await LoadAsync(ct);
                if (Gaps.Count > 0)
                {
                    TempData["Error"] = "Posting not switched on — complete the setup first: " + string.Join(" ", Gaps);
                    return RedirectToPage();
                }
                if (Form.StartDate is null)
                {
                    TempData["Error"] = "Set the cut-over date (first work date to post) before switching posting on.";
                    return RedirectToPage();
                }
            }
            await _svc.SaveSetupAsync(new TM_JCP_SETUP
            {
                JCS_ENABLED_YN = Form.Enabled ? "Y" : "N",
                JCS_COMP_CODE = (Form.CompCode ?? "01").Trim(),
                JCS_OFF_CODE = (Form.OffCode ?? "01").Trim(),
                JCS_JRNL_TYPE = (Form.JrnlType ?? "JRV").Trim(),
                JCS_START_DATE = Form.StartDate?.Date
            }, User.UserId(), ct);
            TempData["Success"] = Form.Enabled
                ? $"Job cost posting is ON — every time-sheet save from now on posts its JV (work dates from {Form.StartDate:dd-MMM-yyyy})."
                : "Job cost posting settings saved (posting is OFF).";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saving job cost posting setup failed");
            TempData["Error"] = "Save failed.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveCodeAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Input.JCC_DESC) || Input.JCC_RATE < 0
            || (Input.JCC_TYPE == CostTypes.Labour && string.IsNullOrWhiteSpace(Input.JCC_LOCATION)))
        {
            TempData["Error"] = "Description, a rate of 0 or more, and (for labour) the job location are required.";
            return RedirectToPage();
        }
        if (Input.JCC_DR_VEL_CODE is not null && Input.JCC_DR_VEL_CODE == Input.JCC_CR_VEL_CODE)
        {
            TempData["Error"] = "The debit (job cost) and credit (absorbed) accounts must be different.";
            return RedirectToPage();
        }
        try
        {
            if (!IsEdit) Input.JCC_ID = 0;
            await _svc.SaveCostCodeAsync(Input, User.UserId(), ct);
            TempData["Success"] = $"Cost code “{Input.JCC_DESC}” saved. It applies to sheets saved from now on.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saving job cost code failed");
            TempData["Error"] = "Save failed.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(decimal id, string active, CancellationToken ct)
    {
        try { await _svc.SetCostCodeActiveAsync(id, active == "Y", User.UserId(), ct); }
        catch (Exception ex) { _logger.LogError(ex, "Toggle cost code {Id} failed", id); TempData["Error"] = "Action failed."; }
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        try
        {
            SchemaReady = await _svc.IsSchemaReadyAsync(ct);
            Accounts = await _svc.GetAccountsAsync(ct);
            JournalTypes = await _svc.GetJournalTypesAsync(ct);
            Locations = await _locations.GetActiveAsync(ct);
            if (!SchemaReady) return;
            Current = await _svc.GetSetupAsync(ct);
            Codes = await _svc.GetCostCodesAsync(ct);
            Form = new SetupInput
            {
                Enabled = Current.Enabled, CompCode = Current.JCS_COMP_CODE, OffCode = Current.JCS_OFF_CODE,
                JrnlType = Current.JCS_JRNL_TYPE, StartDate = Current.JCS_START_DATE
            };

            var active = Codes.Where(c => c.Active).ToList();
            foreach (var c in active.Where(c => c.JCC_DR_VEL_CODE is null || c.JCC_CR_VEL_CODE is null))
                Gaps.Add($"“{c.JCC_DESC}” has no {(c.JCC_DR_VEL_CODE is null ? "debit" : "credit")} account.");
            foreach (var t in new[] { CostTypes.TravelHours, CostTypes.TravelKm }.Where(t => active.All(c => c.JCC_TYPE != t)))
                Gaps.Add($"No active {CostTypes.Label(t).ToLowerInvariant()} rate.");
            foreach (var loc in Locations.Where(l => active.All(c => c.JCC_TYPE != CostTypes.Labour
                         || !string.Equals(c.JCC_LOCATION, l.CODE, StringComparison.OrdinalIgnoreCase))))
                Notes.Add($"No labour rate for location {loc.NAME} — its hours will not be posted.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job cost posting setup unavailable");
            IsOffline = true;
        }
    }
}
