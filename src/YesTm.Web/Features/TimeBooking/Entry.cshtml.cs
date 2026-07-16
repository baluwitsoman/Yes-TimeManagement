using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.TimeCalc;
using YesTm.Web.Features.Masters;

namespace YesTm.Web.Features.TimeBooking;

public class EntryModel : PageModel
{
    private readonly ITimeBookingRepository _repo;
    private readonly IJobLocationRepository _locations;
    private readonly ILogger<EntryModel> _logger;

    public EntryModel(ITimeBookingRepository repo, IJobLocationRepository locations, ILogger<EntryModel> logger)
    {
        _repo = repo;
        _locations = locations;
        _logger = logger;
    }

    // ---- Lookups for the screen ----
    public IReadOnlyList<JobLookup> Jobs { get; private set; } = [];
    public IReadOnlyList<TechnicianLookup> Technicians { get; private set; } = [];
    public IReadOnlyList<TaskLookup> Tasks { get; private set; } = [];
    public IReadOnlyList<WorkTypeLookup> WorkTypes { get; private set; } = [];
    public IReadOnlyList<CodeName> Locations { get; private set; } = [];
    public DutyTimingDto Duty { get; private set; } = new();
    public bool IsOffline { get; private set; }

    [BindProperty] public HeaderInput Header { get; set; } = new();
    [BindProperty] public List<LineInput> Lines { get; set; } = [];

    public class HeaderInput
    {
        [DataType(DataType.Date)] public DateTime PostingDate { get; set; } = DateTime.Today;
        [Required(ErrorMessage = "Select a Job / Work Order.")] public string JobCode { get; set; } = string.Empty;
        public string? CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public string? IndustryCode { get; set; }
        // Equipment details (auto-filled from the selected job; re-fetched on post).
        public string? Brand { get; set; }
        public string? EquipmentType { get; set; }
        public string? ServiceType { get; set; }
        public string? SerialNo { get; set; }
        public string? JobOpeningDate { get; set; }
        [Required(ErrorMessage = "Job Location is required.")] public string Location { get; set; } = "Field";
        [Required(ErrorMessage = "Work / Labour Type is required.")] public string WorkTypeCode { get; set; } = string.Empty;
        [Required] public string JobStatus { get; set; } = "In Progress";
        public string? Remarks { get; set; }
    }

    public class LineInput
    {
        public string TaskCode { get; set; } = string.Empty;
        public string? TaskName { get; set; }
        public string? TechCode { get; set; }
        public string? TechName { get; set; }
        public string? Skill { get; set; }
        public DateTime StartDt { get; set; }
        public DateTime EndDt { get; set; }
        public decimal LunchHours { get; set; }
    }

    public async Task OnGetAsync(CancellationToken ct) => await LoadLookupsAsync(ct);

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        if (Lines.Count == 0)
            ModelState.AddModelError(string.Empty, "Add at least one task line before posting.");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            // Re-fetch the job so equipment/customer/industry are authoritative (not client-supplied).
            var job = await _repo.GetJobAsync(Header.JobCode, ct);
            var industryCode = job?.INDUSTRY_CODE ?? Header.IndustryCode;

            var duty = ToDutyTiming(Duty);
            var rate = await _repo.GetLabourRateAsync(Header.Location, industryCode, ct);

            var lineEntities = new List<TM_TIME_LINE>();
            decimal totalNet = 0, totalCost = 0, totalNormal = 0, totalOt = 0, totalStd = 0;
            var lineNo = 0;

            foreach (var l in Lines)
            {
                lineNo++;
                var stdHours = await _repo.GetTaskStdHoursAsync(l.TaskCode, ct);
                var calc = TimeBookingCalculator.Calculate(
                    new TimeLineCalcInput(l.StartDt, l.EndDt, l.LunchHours, rate.Rate, rate.OvertimeMultiplier),
                    duty);

                totalNet += calc.NetHours;
                totalCost += calc.LabourCost;
                totalStd += stdHours;
                if (calc.TimeType == TimeType.Overtime) totalOt += calc.NetHours; else totalNormal += calc.NetHours;

                lineEntities.Add(new TM_TIME_LINE
                {
                    TL_LINE_NO = lineNo,
                    TL_TASK_CODE = l.TaskCode,
                    TL_TASK_NAME = l.TaskName,
                    TL_STD_HOURS = stdHours,
                    TL_TECH_CODE = l.TechCode,
                    TL_TECH_NAME = l.TechName,
                    TL_SKILL = l.Skill,
                    TL_START_DT = l.StartDt,
                    TL_END_DT = l.EndDt,
                    TL_LUNCH_HOURS = l.LunchHours,
                    TL_NET_HOURS = calc.NetHours,
                    TL_TIME_TYPE = calc.TimeType == TimeType.Overtime ? "OVERTIME" : "NORMAL",
                    TL_RATE = calc.AppliedRate,
                    TL_LABOUR_COST = calc.LabourCost,
                    TL_JOB_CODE = Header.JobCode
                });
            }

            var brand = job?.BRAND_DESC ?? job?.MTJ_BRAND ?? Header.Brand;
            var equipmentType = job?.EQUIPMENT_TYPE_DESC ?? Header.EquipmentType;
            var serviceType = job?.SERVICE_TYPE_DESC ?? Header.ServiceType;
            var composedEquipment = string.Join(" · ",
                new[] { brand, equipmentType }.Where(x => !string.IsNullOrWhiteSpace(x)));

            var header = new TM_TIME_SHEET
            {
                TS_POSTING_DATE = Header.PostingDate,
                TS_JOB_CODE = Header.JobCode,
                TS_CUSTOMER_CODE = job?.MTJ_PARTY_CODE ?? Header.CustomerCode,
                TS_CUSTOMER_NAME = job?.CUSTOMER_NAME ?? Header.CustomerName,
                TS_EQUIPMENT = composedEquipment.Length == 0 ? null : composedEquipment,
                TS_INDUSTRY_CODE = industryCode,
                TS_BRAND = brand,
                TS_EQUIPMENT_TYPE = equipmentType,
                TS_SERVICE_TYPE = serviceType,
                TS_SERIAL_NO = job?.MTJ_SERIAL_NO ?? Header.SerialNo,
                TS_JOB_OPENING_DATE = job?.MTJ_JOB_OPENING_DATE,
                TS_LOCATION = Header.Location,
                TS_WORK_TYPE_CODE = Header.WorkTypeCode,
                TS_JOB_STATUS = Header.JobStatus,
                TS_TOTAL_NET_HOURS = totalNet,
                TS_TOTAL_LABOUR_COST = totalCost,
                TS_NORMAL_HOURS = totalNormal,
                TS_OT_HOURS = totalOt,
                TS_STD_HOURS = totalStd,
                TS_REMARKS = Header.Remarks,
                TS_CREATION_USER_ID = CurrentUserId()
            };

            var sheetNo = await _repo.SaveTimeSheetAsync(header, lineEntities, ct);
            TempData["Success"] = $"Time sheet {sheetNo} posted — {lineEntities.Count} task line(s), labour cost {totalCost:N2}.";
            return RedirectToPage("/TimeBooking/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Posting time sheet failed");
            ModelState.AddModelError(string.Empty,
                "Unable to post the time sheet (database unavailable). Please verify the Oracle connection and try again.");
            return Page();
        }
    }

    private decimal? CurrentUserId() =>
        decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            Jobs = await _repo.GetJobsAsync(ct);
            Technicians = await _repo.GetTechniciansAsync(ct);
            Tasks = await _repo.GetTasksAsync(ct);
            WorkTypes = await _repo.GetWorkTypesAsync(ct);
            Locations = await _locations.GetActiveAsync(ct);
            Duty = await _repo.GetDutyTimingAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Time Booking lookups unavailable — falling back to demo data");
            IsOffline = true;
            LoadDemoData();
        }
    }

    private void LoadDemoData()
    {
        Jobs =
        [
            new() { MTJ_JOB_CODE = "JOB-2026-00471", MTJ_JOB_DESC = "Recondition Transmission", MTJ_PARTY_CODE = "C-1042", CUSTOMER_NAME = "Petro Gulf Energy LLC", MTJ_JOB_LOCATION = "Field", INDUSTRY_CODE = "OG",
                    BRAND_DESC = "ALLISON", EQUIPMENT_TYPE_DESC = "Transmission", SERVICE_TYPE_DESC = "G Type", MTJ_SERIAL_NO = "VCE0A35EA00010859", MTJ_JOB_OPENING_DATE = new DateTime(2026, 6, 1, 8, 0, 0) },
            new() { MTJ_JOB_CODE = "JOB-2026-00488", MTJ_JOB_DESC = "Engine Overhaul", MTJ_PARTY_CODE = "C-1080", CUSTOMER_NAME = "Gulf Contracting Co", MTJ_JOB_LOCATION = "Workshop", INDUSTRY_CODE = "GC",
                    BRAND_DESC = "Volvo", EQUIPMENT_TYPE_DESC = "Excavator", SERVICE_TYPE_DESC = "C Type", MTJ_SERIAL_NO = "SL NO 3110119571", MTJ_JOB_OPENING_DATE = new DateTime(2026, 5, 20, 8, 0, 0) }
        ];
        Technicians =
        [
            new() { PEMP_EMP_CODE = "E-204", PEMP_EMP_NAME = "Rajesh K." },
            new() { PEMP_EMP_CODE = "E-211", PEMP_EMP_NAME = "Sami A." },
            new() { PEMP_EMP_CODE = "E-230", PEMP_EMP_NAME = "Imran H." }
        ];
        Tasks =
        [
            new() { TASK_CODE = "DIS", TASK_NAME = "Disassemble", DEFAULT_SKILL = "Assistant", STD_HOURS = 16m },
            new() { TASK_CODE = "ASM", TASK_NAME = "Assemble", DEFAULT_SKILL = "Lead Hand", STD_HOURS = 19m },
            new() { TASK_CODE = "TST", TASK_NAME = "Testing", DEFAULT_SKILL = "Lead Hand", STD_HOURS = 8m },
            new() { TASK_CODE = "CLN", TASK_NAME = "Cleaning & Reusability", DEFAULT_SKILL = "Assistant", STD_HOURS = 6m }
        ];
        WorkTypes =
        [
            new() { WORK_TYPE_CODE = "REP", WORK_TYPE_NAME = "Repair – Revenue (Chargeable)" },
            new() { WORK_TYPE_CODE = "WAR", WORK_TYPE_NAME = "Warranty" },
            new() { WORK_TYPE_CODE = "NCT", WORK_TYPE_NAME = "Non-Chargeable – Training" },
            new() { WORK_TYPE_CODE = "IDL", WORK_TYPE_NAME = "Idle / Loss Time" }
        ];
        Locations =
        [
            new("Field", "Field"), new("Workshop", "Workshop"), new("Service", "Service")
        ];
        Duty = new DutyTimingDto();
    }

    public static DutyTiming ToDutyTiming(DutyTimingDto dto)
    {
        var weekend = (dto.WEEKEND_DAYS ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => Enum.TryParse<DayOfWeek>(d, true, out var dow) ? dow : (DayOfWeek?)null)
            .Where(d => d.HasValue).Select(d => d!.Value)
            .ToHashSet();
        if (weekend.Count == 0) { weekend.Add(DayOfWeek.Friday); weekend.Add(DayOfWeek.Saturday); }

        return new DutyTiming(
            TimeOnly.TryParse(dto.DUTY_START, out var s) ? s : new TimeOnly(8, 0),
            TimeOnly.TryParse(dto.DUTY_END, out var e) ? e : new TimeOnly(17, 0),
            dto.DEFAULT_LUNCH_HOURS,
            weekend);
    }
}
