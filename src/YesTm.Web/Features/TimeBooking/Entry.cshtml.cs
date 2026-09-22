using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using YesTm.Web.Common.Security;
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
    /// <summary>Job code → sheet no for jobs that already have an active time sheet (one sheet per job).</summary>
    public IReadOnlyDictionary<string, string> ExistingSheetNos { get; private set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public bool IsOffline { get; private set; }

    // ---- Edit-mode state ----
    [BindProperty] public decimal? TsId { get; set; }
    public bool IsEditMode => TsId.HasValue;
    public string? SheetNo { get; private set; }
    public IReadOnlyList<LineInput> ExistingLines { get; private set; } = [];
    public bool IsAdmin => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);
    /// <summary>Set when a post was rejected because the job already has a sheet — the view links to it.</summary>
    public ExistingSheetLookup? ConflictingSheet { get; private set; }

    /// <summary>Admins edit any sheet; a normal employee may edit one he created or is a technician on.</summary>
    private bool CanEdit(TimeSheetEditDto dto)
    {
        if (IsAdmin) return true;
        if (dto.TS_CREATION_USER_ID == User.UserId()) return true;
        var emp = User.EmpCode();
        return emp is not null && dto.Lines.Any(l =>
            string.Equals(l.TL_TECH_CODE, emp, StringComparison.OrdinalIgnoreCase));
    }

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

    /// <summary>
    /// One task line as posted from the grid. Times are "HH:mm" strings on <see cref="WorkDate"/>;
    /// the hour/rate/cost fields carry what the user saw (auto-calculated, possibly adjusted).
    /// </summary>
    public class LineInput
    {
        public string TaskCode { get; set; } = string.Empty;
        public string? TaskName { get; set; }
        public string? TechCode { get; set; }
        public string? TechName { get; set; }
        public string? Skill { get; set; }
        // Job Location where this line's work was done — drives the line's labour rate.
        public string? Location { get; set; }
        [DataType(DataType.Date)] public DateTime WorkDate { get; set; } = DateTime.Today;
        public string StartTime { get; set; } = string.Empty;   // HH:mm
        public string EndTime { get; set; } = string.Empty;     // HH:mm (rolls to next day when <= start)
        public decimal LunchHours { get; set; }
        // Auto-calculated by the CalcLine endpoint, editable by the user before adding to the grid.
        public decimal NetHours { get; set; }
        public decimal NormalHours { get; set; }
        public decimal OtHours { get; set; }
        public decimal Rate { get; set; }
        public decimal OtRate { get; set; }
        public decimal LabourCost { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal TotalCost { get; set; }
        // Travel
        public string? TravelSite { get; set; }
        public string? TravelStart { get; set; }   // HH:mm
        public string? TravelEnd { get; set; }     // HH:mm
        public decimal TravelHours { get; set; }
        // Display-only seeds for edit mode.
        public decimal StdHours { get; set; }
        public string? TimeType { get; set; }
        public bool Overridden { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(decimal? tsId, CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        if (tsId.HasValue)
        {
            var dto = await _repo.GetTimeSheetForEditAsync(tsId.Value, ct);
            if (dto is null)
            {
                TempData["Error"] = "Time sheet not found (it may have been voided).";
                return RedirectToPage("/TimeBooking/Index");
            }

            // Admins edit any sheet; a normal employee may edit one he is involved in
            // (he created it, or he is a technician on one of its lines).
            if (!CanEdit(dto))
            {
                TempData["Error"] = "You are not allowed to edit this time sheet.";
                return RedirectToPage("/TimeBooking/Index");
            }

            TsId = dto.TS_ID;
            SheetNo = dto.TS_SHEET_NO;
            Header = new HeaderInput
            {
                PostingDate = dto.TS_POSTING_DATE,
                JobCode = dto.TS_JOB_CODE ?? string.Empty,
                CustomerCode = dto.TS_CUSTOMER_CODE,
                CustomerName = dto.TS_CUSTOMER_NAME,
                IndustryCode = dto.TS_INDUSTRY_CODE,
                Brand = dto.TS_BRAND,
                EquipmentType = dto.TS_EQUIPMENT_TYPE,
                ServiceType = dto.TS_SERVICE_TYPE,
                SerialNo = dto.TS_SERIAL_NO,
                JobOpeningDate = dto.TS_JOB_OPENING_DATE?.ToString("dd-MMM-yyyy HH:mm"),
                Location = dto.TS_LOCATION ?? "Field",
                WorkTypeCode = dto.TS_WORK_TYPE_CODE ?? string.Empty,
                JobStatus = dto.TS_JOB_STATUS ?? "In Progress",
                Remarks = dto.TS_REMARKS
            };

            // The list of Jobs excludes CLOSED jobs; make sure the sheet's own job is selectable.
            if (!string.IsNullOrEmpty(Header.JobCode) && !Jobs.Any(j => j.MTJ_JOB_CODE == Header.JobCode))
            {
                var job = await _repo.GetJobAsync(Header.JobCode, ct);
                if (job is not null) Jobs = Jobs.Prepend(job).ToList();
            }

            ExistingLines = dto.Lines.Select(l => new LineInput
            {
                TaskCode = l.TL_TASK_CODE ?? string.Empty,
                TaskName = l.TL_TASK_NAME,
                TechCode = l.TL_TECH_CODE,
                TechName = l.TL_TECH_NAME,
                Skill = l.TL_SKILL,
                Location = l.TL_LOCATION,
                WorkDate = l.TL_WORK_DATE ?? l.TL_START_DT.Date,
                StartTime = l.TL_START_DT.ToString("HH:mm"),
                EndTime = l.TL_END_DT.ToString("HH:mm"),
                LunchHours = l.TL_LUNCH_HOURS,
                NetHours = l.TL_NET_HOURS,
                NormalHours = l.TL_NORMAL_HOURS,
                OtHours = l.TL_OT_HOURS,
                Rate = l.TL_RATE,
                OtRate = l.TL_OT_RATE,
                LabourCost = l.TL_LABOUR_COST,
                FoodAllowance = l.TL_FOOD_ALLOWANCE,
                TotalCost = l.TL_TOTAL_COST,
                TravelSite = l.TL_TRAVEL_SITE,
                TravelStart = l.TL_TRAVEL_START?.ToString("HH:mm"),
                TravelEnd = l.TL_TRAVEL_END?.ToString("HH:mm"),
                TravelHours = l.TL_TRAVEL_HOURS,
                StdHours = l.TL_STD_HOURS,
                TimeType = l.TL_TIME_TYPE,
                Overridden = string.Equals(l.TL_OVERRIDE_YN, "Y", StringComparison.OrdinalIgnoreCase)
            }).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        // Re-check involvement on the server (never trust the posted TsId): admins may edit any
        // sheet; a normal employee only one he created or is a technician on.
        if (TsId.HasValue && !IsAdmin
            && !await _repo.CanEditTimeSheetAsync(TsId.Value, User.UserId(), User.EmpCode(), ct))
        {
            TempData["Error"] = "You are not allowed to edit this time sheet.";
            return RedirectToPage("/TimeBooking/Index");
        }

        if (Lines.Count == 0)
            ModelState.AddModelError(string.Empty, "Add at least one task line before posting.");

        for (var i = 0; i < Lines.Count; i++)
        {
            var l = Lines[i];
            if (!TryCombine(l.WorkDate, l.StartTime, out _) || !TryCombine(l.WorkDate, l.EndTime, out _))
                ModelState.AddModelError(string.Empty, $"Line {i + 1}: start and end times must be valid HH:mm values.");
        }

        // One time sheet per job: reject a second sheet (or an edit that re-points to a job that has one).
        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(Header.JobCode))
        {
            try
            {
                ConflictingSheet = await _repo.FindActiveSheetByJobAsync(Header.JobCode, TsId, ct);
                if (ConflictingSheet is not null)
                    ModelState.AddModelError(string.Empty,
                        $"Job {Header.JobCode} already has time sheet {ConflictingSheet.TS_SHEET_NO}. Open that sheet to add more task lines.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Existing-sheet check failed for job {JobCode}", Header.JobCode);
            }
        }

        // Whatever fails below, the user must not lose the task lines already keyed in.
        ExistingLines = Lines;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var (header, lineEntities) = await BuildHeaderAndLinesAsync(ct);

            if (TsId.HasValue)
            {
                header.TS_ID = TsId.Value;
                await _repo.UpdateTimeSheetAsync(header, lineEntities, CurrentUserId(), ct);
                TempData["Success"] = $"Time sheet updated — {lineEntities.Count} task line(s), total cost {header.TS_TOTAL_COST:N2}.";
            }
            else
            {
                var sheetNo = await _repo.SaveTimeSheetAsync(header, lineEntities, ct);
                TempData["Success"] = $"Time sheet {sheetNo} posted — {lineEntities.Count} task line(s), total cost {header.TS_TOTAL_COST:N2}.";
            }
            return RedirectToPage("/TimeBooking/Index");
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            // Unique index TM_TIME_SHEET_UK_JOB_ACTIVE: someone posted a sheet for this job in the meantime.
            _logger.LogWarning(ex, "Duplicate time sheet for job {JobCode} rejected by the database", Header.JobCode);
            ConflictingSheet = await _repo.FindActiveSheetByJobAsync(Header.JobCode, TsId, ct);
            ModelState.AddModelError(string.Empty,
                $"Job {Header.JobCode} already has time sheet {ConflictingSheet?.TS_SHEET_NO}. Open that sheet to add more task lines.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saving time sheet failed (edit={IsEdit})", TsId.HasValue);
            ModelState.AddModelError(string.Empty,
                "Unable to save the time sheet (database unavailable). Please verify the Oracle connection and try again.");
            return Page();
        }
    }

    /// <summary>
    /// AJAX endpoint: returns the open jobs at a given Job Location (empty/absent = all) as JSON so the
    /// Job / Work Order dropdown can be re-filtered without reposting the whole page. Create-mode only.
    /// </summary>
    public async Task<IActionResult> OnGetJobsByLocationAsync(string? location, CancellationToken ct)
    {
        try
        {
            var jobs = await _repo.GetJobsAsync(string.IsNullOrWhiteSpace(location) ? null : location, ct);
            var sheets = await _repo.GetActiveSheetNosByJobAsync(ct);
            return new JsonResult(jobs.Select(j => new
            {
                code = j.MTJ_JOB_CODE,
                desc = j.MTJ_JOB_DESC,
                customerCode = j.MTJ_PARTY_CODE,
                customerName = j.CUSTOMER_NAME,
                industry = j.INDUSTRY_CODE,
                location = j.MTJ_JOB_LOCATION,
                brand = j.BRAND_DESC,
                equipType = j.EQUIPMENT_TYPE_DESC,
                serviceType = j.SERVICE_TYPE_DESC,
                serial = j.MTJ_SERIAL_NO,
                opening = j.MTJ_JOB_OPENING_DATE?.ToString("dd-MMM-yyyy HH:mm"),
                sheetNo = sheets.TryGetValue(j.MTJ_JOB_CODE, out var sn) ? sn : null
            }));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job-by-location lookup failed for {Location}", location);
            return StatusCode(503);   // let the client keep the current list
        }
    }

    /// <summary>
    /// AJAX endpoint: does this job already have an active time sheet (other than the one being edited)?
    /// The entry screen uses it to offer a redirect to the existing sheet as soon as the job is picked.
    /// </summary>
    public async Task<IActionResult> OnGetSheetByJobAsync(string? jobCode, decimal? excludeTsId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(jobCode)) return new JsonResult(new { exists = false });
        try
        {
            var hit = await _repo.FindActiveSheetByJobAsync(jobCode.Trim(), excludeTsId, ct);
            return new JsonResult(hit is null
                ? new { exists = false, tsId = (decimal?)null, sheetNo = (string?)null }
                : new { exists = true, tsId = (decimal?)hit.TS_ID, sheetNo = hit.TS_SHEET_NO });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sheet-by-job lookup failed for {JobCode}", jobCode);
            return StatusCode(503);
        }
    }

    /// <summary>
    /// AJAX endpoint: the server-side calculation for one task line (Net, Normal/OT split, rates, labour
    /// cost, food allowance, travel hours) so the Add-Task modal shows the same numbers the post will use.
    /// Every returned value is a starting point the user may adjust before adding the line.
    /// </summary>
    public async Task<IActionResult> OnGetCalcLineAsync(DateTime? workDate, string? start, string? end, decimal? lunch,
        string? location, string? industry, string? travelStart, string? travelEnd, CancellationToken ct)
    {
        var date = workDate ?? DateTime.Today;
        if (!TryCombine(date, start, out var startDt) || !TryCombine(date, end, out var endDt))
            return BadRequest(new { error = "Start and end times are required (HH:mm)." });
        if (endDt <= startDt) endDt = endDt.AddDays(1);   // overnight shift

        var lunchHours = Math.Max(0m, lunch ?? 0m);
        LabourRateResult rate;
        DutyTimingDto dutyDto;
        try
        {
            dutyDto = await _repo.GetDutyTimingAsync(ct);
            rate = await _repo.GetLabourRateAsync(string.IsNullOrWhiteSpace(location) ? Header.Location : location,
                string.IsNullOrWhiteSpace(industry) ? null : industry, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Line calculation lookups failed");
            dutyDto = new DutyTimingDto();
            rate = new LabourRateResult(0m, 1.5m, 0m);
        }

        TimeLineCalcResult calc;
        try
        {
            calc = TimeBookingCalculator.Calculate(
                new TimeLineCalcInput(startDt, endDt, lunchHours, rate.Rate, rate.OvertimeMultiplier), ToDutyTiming(dutyDto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        var travelHours = TravelHours(date, travelStart, travelEnd);
        return new JsonResult(new
        {
            startDt = startDt.ToString("yyyy-MM-ddTHH:mm"),
            endDt = endDt.ToString("yyyy-MM-ddTHH:mm"),
            elapseHours = calc.ElapseHours,
            netHours = calc.NetHours,
            normalHours = calc.NormalHours,
            otHours = calc.OtHours,
            timeType = TimeTypeCode(calc.TimeType),
            rate = calc.NormalRate,
            otRate = calc.OtRate,
            labourCost = calc.LabourCost,
            foodAllowance = rate.FoodAllowance,
            totalCost = Math.Round(calc.LabourCost + rate.FoodAllowance, 2, MidpointRounding.AwayFromZero),
            travelHours,
            rateFound = rate.Rate > 0
        });
    }

    public async Task<IActionResult> OnPostVoidAsync(decimal tsId, CancellationToken ct)
    {
        if (!IsAdmin)
        {
            TempData["Error"] = "You are not allowed to void time sheets.";
            return RedirectToPage("/TimeBooking/Index");
        }

        try
        {
            await _repo.VoidTimeSheetAsync(tsId, CurrentUserId(), ct);
            TempData["Success"] = "Time sheet voided.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Voiding time sheet {TsId} failed", tsId);
            TempData["Error"] = "Unable to void the time sheet.";
        }
        return RedirectToPage("/TimeBooking/Index");
    }

    /// <summary>
    /// Re-fetches the job (authoritative equipment/customer/industry) and builds the time-sheet
    /// header + line entities. Each line is recalculated with the engine; where the posted values
    /// differ from that calculation the user's figures win and the line is flagged as overridden,
    /// so an admin can see exactly what was adjusted. Shared by the create and edit post paths.
    /// </summary>
    private async Task<(TM_TIME_SHEET header, List<TM_TIME_LINE> lines)> BuildHeaderAndLinesAsync(CancellationToken ct)
    {
        var job = await _repo.GetJobAsync(Header.JobCode, ct);
        var industryCode = job?.INDUSTRY_CODE ?? Header.IndustryCode;

        var duty = ToDutyTiming(Duty);

        // Labour rate is resolved per line by that line's own location (falling back to the header
        // location). Cache by location so repeat locations don't re-query — industry is fixed per sheet.
        var rateByLocation = new Dictionary<string, LabourRateResult>(StringComparer.OrdinalIgnoreCase);

        var lineEntities = new List<TM_TIME_LINE>();
        decimal totalNet = 0, totalCost = 0, totalNormal = 0, totalOt = 0, totalStd = 0, totalFood = 0, totalTravel = 0;
        var lineNo = 0;

        foreach (var l in Lines)
        {
            lineNo++;
            var lineLocation = string.IsNullOrWhiteSpace(l.Location) ? Header.Location : l.Location;
            if (!rateByLocation.TryGetValue(lineLocation, out var rate))
            {
                rate = await _repo.GetLabourRateAsync(lineLocation, industryCode, ct);
                rateByLocation[lineLocation] = rate;
            }

            var stdHours = await _repo.GetTaskStdHoursAsync(l.TaskCode, ct);

            TryCombine(l.WorkDate, l.StartTime, out var startDt);
            TryCombine(l.WorkDate, l.EndTime, out var endDt);
            if (endDt <= startDt) endDt = endDt.AddDays(1);
            var lunch = Math.Max(0m, l.LunchHours);

            var calc = TimeBookingCalculator.Calculate(
                new TimeLineCalcInput(startDt, endDt, lunch, rate.Rate, rate.OvertimeMultiplier), duty);

            // User-adjusted figures take precedence over the engine; note every deviation.
            var normalHours = R2(Math.Max(0m, l.NormalHours));
            var otHours = R2(Math.Max(0m, l.OtHours));
            var normalRate = R2(Math.Max(0m, l.Rate));
            var otRate = R2(Math.Max(0m, l.OtRate));
            var labourCost = R2(Math.Max(0m, l.LabourCost));
            var food = R2(Math.Max(0m, l.FoodAllowance));
            var netHours = R2(normalHours + otHours);

            DateTime? travelStart = TryCombine(l.WorkDate, l.TravelStart, out var ts) ? ts : null;
            DateTime? travelEnd = TryCombine(l.WorkDate, l.TravelEnd, out var te) ? te : null;
            if (travelStart.HasValue && travelEnd.HasValue && travelEnd <= travelStart) travelEnd = travelEnd.Value.AddDays(1);
            var autoTravel = TravelHours(l.WorkDate, l.TravelStart, l.TravelEnd);
            var travelHours = R2(Math.Max(0m, l.TravelHours));

            var overridden =
                normalHours != calc.NormalHours || otHours != calc.OtHours ||
                normalRate != calc.NormalRate || otRate != calc.OtRate ||
                labourCost != TimeBookingCalculator.Cost(normalHours, otHours, normalRate, otRate) ||
                food != R2(rate.FoodAllowance) ||
                (autoTravel > 0m && travelHours != autoTravel);

            var timeType = otHours <= 0m ? TimeType.Normal : normalHours <= 0m ? TimeType.Overtime : TimeType.Mixed;
            var totalLineCost = R2(labourCost + food);

            totalNet += netHours;
            totalCost += labourCost;
            totalNormal += normalHours;
            totalOt += otHours;
            totalStd += stdHours;
            totalFood += food;
            totalTravel += travelHours;

            lineEntities.Add(new TM_TIME_LINE
            {
                TL_LINE_NO = lineNo,
                TL_TASK_CODE = l.TaskCode,
                TL_TASK_NAME = l.TaskName,
                TL_STD_HOURS = stdHours,
                TL_TECH_CODE = l.TechCode,
                TL_TECH_NAME = l.TechName,
                TL_SKILL = l.Skill,
                TL_START_DT = startDt,
                TL_END_DT = endDt,
                TL_LUNCH_HOURS = lunch,
                TL_NET_HOURS = netHours,
                TL_TIME_TYPE = TimeTypeCode(timeType),
                TL_RATE = normalRate,
                TL_LABOUR_COST = labourCost,
                TL_JOB_CODE = Header.JobCode,
                TL_LOCATION = lineLocation,
                TL_WORK_DATE = l.WorkDate.Date,
                TL_NORMAL_HOURS = normalHours,
                TL_OT_HOURS = otHours,
                TL_OT_RATE = otRate,
                TL_FOOD_ALLOWANCE = food,
                TL_TOTAL_COST = totalLineCost,
                TL_TRAVEL_SITE = string.IsNullOrWhiteSpace(l.TravelSite) ? null : l.TravelSite.Trim(),
                TL_TRAVEL_START = travelStart,
                TL_TRAVEL_END = travelEnd,
                TL_TRAVEL_HOURS = travelHours,
                TL_OVERRIDE_YN = overridden ? "Y" : "N"
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
            TS_TOTAL_FOOD_ALLOWANCE = totalFood,
            TS_TOTAL_TRAVEL_HOURS = totalTravel,
            TS_TOTAL_COST = R2(totalCost + totalFood),
            TS_REMARKS = Header.Remarks,
            TS_CREATION_USER_ID = CurrentUserId()
        };

        return (header, lineEntities);
    }

    private decimal? CurrentUserId() =>
        decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static string TimeTypeCode(TimeType t) => t switch
    {
        TimeType.Overtime => "OVERTIME",
        TimeType.Mixed => "MIXED",
        _ => "NORMAL"
    };

    /// <summary>date + "HH:mm" (also accepts "H:mm" and "HH:mm:ss") → DateTime.</summary>
    private static bool TryCombine(DateTime date, string? time, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(time)) return false;
        if (!TimeOnly.TryParseExact(time.Trim(), ["HH:mm", "H:mm", "HH:mm:ss", "H:mm:ss"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
            return false;
        result = date.Date.Add(t.ToTimeSpan());
        return true;
    }

    /// <summary>Hours between travel start and end on the work date (end rolls over midnight); 0 when either is blank.</summary>
    private static decimal TravelHours(DateTime date, string? travelStart, string? travelEnd)
    {
        if (!TryCombine(date, travelStart, out var s) || !TryCombine(date, travelEnd, out var e)) return 0m;
        if (e <= s) e = e.AddDays(1);
        return R2((decimal)(e - s).TotalHours);
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            Jobs = await _repo.GetJobsAsync(null, ct);
            Technicians = await _repo.GetTechniciansAsync(ct);
            Tasks = await _repo.GetTasksAsync(ct);
            WorkTypes = await _repo.GetWorkTypesAsync(ct);
            Locations = await _locations.GetActiveAsync(ct);
            Duty = await _repo.GetDutyTimingAsync(ct);
            ExistingSheetNos = await _repo.GetActiveSheetNosByJobAsync(ct);
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
        ExistingSheetNos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
