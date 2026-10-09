using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using YesTm.Web.Common.Security;
using YesTm.Web.Common.TimeCalc;
using YesTm.Web.Features.TimeBooking;

namespace YesTm.Web.Features.FieldService;

/// <summary>
/// Field Service Report entry — the time sheet for a Field job, laid out like the paper FSR.
/// One FSR per job (the job's single active time sheet). Hours are booked as date-wise segment
/// lines (Travel / Work) for each crew member and stored in TM_TIME_LINE like any other sheet;
/// the report narrative, vehicle and sign-off go to TM_FIELD_REPORT (+ crew).
/// </summary>
public class EntryModel : PageModel
{
    public const string FieldLocation = "Field";

    private readonly ITimeBookingRepository _repo;
    private readonly IFieldServiceRepository _fsr;
    private readonly ILogger<EntryModel> _logger;

    public EntryModel(ITimeBookingRepository repo, IFieldServiceRepository fsr, ILogger<EntryModel> logger)
    {
        _repo = repo;
        _fsr = fsr;
        _logger = logger;
    }

    // ---- Lookups ----
    public IReadOnlyList<JobLookup> Jobs { get; private set; } = [];
    public IReadOnlyList<TechnicianLookup> Technicians { get; private set; } = [];
    public IReadOnlyList<TaskLookup> Tasks { get; private set; } = [];
    public IReadOnlyList<WorkTypeLookup> WorkTypes { get; private set; } = [];
    public DutyTimingDto Duty { get; private set; } = new();
    public IReadOnlyDictionary<string, string> ExistingSheetNos { get; private set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    /// <summary>Default vehicle rate per km (Labour Rate master, Field location).</summary>
    public decimal DefaultKmRate { get; private set; }
    public bool IsOffline { get; private set; }

    // ---- Edit state ----
    [BindProperty] public decimal? TsId { get; set; }
    public bool IsEditMode => TsId.HasValue;
    public string? SheetNo { get; private set; }
    public bool IsAdmin => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);
    public ExistingSheetLookup? ConflictingSheet { get; private set; }

    [BindProperty] public HeaderInput Header { get; set; } = new();
    [BindProperty] public ReportInput Report { get; set; } = new();
    [BindProperty] public List<CrewInput> Crew { get; set; } = [];
    [BindProperty] public List<SegmentInput> Lines { get; set; } = [];

    public class HeaderInput
    {
        [DataType(DataType.Date)] public DateTime PostingDate { get; set; } = DateTime.Today;
        [Required(ErrorMessage = "Select a Field job.")] public string JobCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? IndustryCode { get; set; }
        public string? Brand { get; set; }
        public string? EquipmentType { get; set; }
        public string? ServiceType { get; set; }
        public string? SerialNo { get; set; }
        [Required(ErrorMessage = "Work / Labour Type is required.")] public string WorkTypeCode { get; set; } = string.Empty;
        [Required] public string JobStatus { get; set; } = "In Progress";
        public string? Remarks { get; set; }
    }

    public class ReportInput
    {
        [StringLength(25)] public string? ReportNo { get; set; }
        [DataType(DataType.Date)] public DateTime? ReportDate { get; set; } = DateTime.Today;
        [StringLength(200)] public string? JobSite { get; set; }
        [StringLength(100)] public string? ContactPerson { get; set; }
        [StringLength(100)] public string? ContactPosition { get; set; }
        [StringLength(50)] public string? ContactPhone { get; set; }
        [StringLength(100)] public string? Oem { get; set; }
        [StringLength(100)] public string? Model { get; set; }
        public decimal? SmuHours { get; set; }
        public bool UnderWarranty { get; set; }
        [StringLength(50)] public string? TaiReport { get; set; }
        [StringLength(50)] public string? JobType { get; set; }
        [StringLength(50)] public string? FleetNo { get; set; }
        [StringLength(30)] public string? PlateNo { get; set; }
        [StringLength(100)] public string? DriverName { get; set; }
        public decimal? KmStart { get; set; }
        public decimal? KmEnd { get; set; }
        public decimal? KmRate { get; set; }
        [StringLength(2000)] public string? Complaint { get; set; }
        [StringLength(2000)] public string? ProblemsFound { get; set; }
        [StringLength(2000)] public string? ProblemCauses { get; set; }
        [StringLength(2000)] public string? Correction { get; set; }
        [StringLength(2000)] public string? OtherItems { get; set; }
        public bool JobCompleted { get; set; }
        public bool BroughtToWorkshop { get; set; }
        [StringLength(100)] public string? ServiceEngineer { get; set; }
        [StringLength(100)] public string? ServiceManager { get; set; }
        [DataType(DataType.Date)] public DateTime? SignDate { get; set; }
        [StringLength(1000)] public string? CustomerComments { get; set; }
        [StringLength(100)] public string? CustomerRep { get; set; }
        [StringLength(100)] public string? CustomerRepPosition { get; set; }
        [DataType(DataType.Date)] public DateTime? CustomerSignDate { get; set; }
    }

    public class CrewInput
    {
        public string EmpCode { get; set; } = string.Empty;
        public string? EmpName { get; set; }
        public string? FaCode { get; set; }
    }

    /// <summary>One segment line for one technician (date + Travel/Work + times + hours/cost).</summary>
    public class SegmentInput
    {
        public string SegmentType { get; set; } = SegmentTypes.Work;
        [DataType(DataType.Date)] public DateTime WorkDate { get; set; } = DateTime.Today;
        public string StartTime { get; set; } = string.Empty;   // HH:mm
        public string EndTime { get; set; } = string.Empty;     // HH:mm (rolls to next day when <= start)
        public decimal LunchHours { get; set; }
        public decimal IdleHours { get; set; }
        public string? IdleReason { get; set; }
        public string? TaskCode { get; set; }
        public string? TaskName { get; set; }
        public string? TechCode { get; set; }
        public string? TechName { get; set; }
        public string? Skill { get; set; }
        public decimal NetHours { get; set; }
        public decimal NormalHours { get; set; }
        public decimal OtHours { get; set; }
        public decimal Rate { get; set; }
        public decimal OtRate { get; set; }
        public decimal LabourCost { get; set; }
        public decimal FoodAllowance { get; set; }
        public decimal TotalCost { get; set; }
        // Display seeds / round-tripped legacy values (lines posted before the FSR screen existed).
        public decimal StdHours { get; set; }
        public string? TimeType { get; set; }
        public bool Overridden { get; set; }
        public string? TravelSite { get; set; }
        public string? TravelStart { get; set; }
        public string? TravelEnd { get; set; }
        public decimal TravelHours { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(decimal? tsId, string? jobCode, CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        if (tsId.HasValue)
        {
            var dto = await _repo.GetTimeSheetForEditAsync(tsId.Value, ct);
            if (dto is null)
            {
                TempData["Error"] = "Field service report not found (it may have been voided).";
                return RedirectToPage("/TimeBooking/Index");
            }
            if (!CanEdit(dto))
            {
                TempData["Error"] = "You are not allowed to edit this field service report.";
                return RedirectToPage("/TimeBooking/Index");
            }
            if (dto.TS_SHEET_TYPE != SheetTypes.Field)
                return RedirectToPage("/TimeBooking/Entry", new { tsId = dto.TS_ID });

            TsId = dto.TS_ID;
            SheetNo = dto.TS_SHEET_NO;
            Header = new HeaderInput
            {
                PostingDate = dto.TS_POSTING_DATE,
                JobCode = dto.TS_JOB_CODE ?? string.Empty,
                CustomerName = dto.TS_CUSTOMER_NAME,
                IndustryCode = dto.TS_INDUSTRY_CODE,
                Brand = dto.TS_BRAND,
                EquipmentType = dto.TS_EQUIPMENT_TYPE,
                ServiceType = dto.TS_SERVICE_TYPE,
                SerialNo = dto.TS_SERIAL_NO,
                WorkTypeCode = dto.TS_WORK_TYPE_CODE ?? string.Empty,
                JobStatus = dto.TS_JOB_STATUS ?? "In Progress",
                Remarks = dto.TS_REMARKS
            };
            await EnsureJobListedAsync(ct);

            var fr = await _fsr.GetReportAsync(dto.TS_ID, ct);
            Report = fr is null ? new ReportInput { ReportDate = dto.TS_POSTING_DATE, KmRate = DefaultKmRate } : FromEntity(fr);
            Crew = (await _fsr.GetCrewAsync(dto.TS_ID, ct))
                .Select(c => new CrewInput { EmpCode = c.FRC_EMP_CODE, EmpName = c.FRC_EMP_NAME, FaCode = c.FRC_FA_CODE })
                .ToList();
            // Sheets posted before the FSR existed have no crew rows — derive the crew from their lines.
            if (Crew.Count == 0)
                Crew = dto.Lines.Where(l => !string.IsNullOrWhiteSpace(l.TL_TECH_CODE))
                    .GroupBy(l => l.TL_TECH_CODE!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new CrewInput { EmpCode = g.Key, EmpName = g.First().TL_TECH_NAME })
                    .ToList();
            Lines = dto.Lines.Select(ToInput).ToList();
            return Page();
        }

        // New FSR: one per job — send the user to the job's existing sheet if there is one.
        if (!string.IsNullOrWhiteSpace(jobCode))
        {
            try
            {
                var existing = await _repo.FindActiveSheetByJobAsync(jobCode.Trim(), null, ct);
                if (existing is not null)
                {
                    TempData["Success"] = $"Job {existing.TS_JOB_CODE} already has {existing.TS_SHEET_NO} — opened it for you.";
                    return RedirectToPage(new { tsId = existing.TS_ID });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Existing-sheet check failed for job {JobCode}", jobCode);
            }

            Header.JobCode = jobCode.Trim();
            await EnsureJobListedAsync(ct);
            var job = Jobs.FirstOrDefault(j => string.Equals(j.MTJ_JOB_CODE, Header.JobCode, StringComparison.OrdinalIgnoreCase));
            if (job is not null)
            {
                Header.CustomerName = job.CUSTOMER_NAME;
                Header.IndustryCode = job.INDUSTRY_CODE;
                Header.Brand = job.BRAND_DESC ?? job.MTJ_BRAND;
                Header.EquipmentType = job.EQUIPMENT_TYPE_DESC;
                Header.ServiceType = job.SERVICE_TYPE_DESC;
                Header.SerialNo = job.MTJ_SERIAL_NO;
                Report.ContactPerson = job.MTJ_CONTACT_PERSON;
                Report.ContactPhone = job.MTJ_CONTACT_PHONE;
            }
        }
        Report.KmRate = DefaultKmRate;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadLookupsAsync(ct);

        if (TsId.HasValue && !IsAdmin
            && !await _repo.CanEditTimeSheetAsync(TsId.Value, User.UserId(), User.EmpCode(), ct))
        {
            TempData["Error"] = "You are not allowed to edit this field service report.";
            return RedirectToPage("/TimeBooking/Index");
        }

        Crew = Crew.Where(c => !string.IsNullOrWhiteSpace(c.EmpCode))
            .GroupBy(c => c.EmpCode.Trim(), StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        Validate();

        if (ModelState.IsValid && !string.IsNullOrWhiteSpace(Header.JobCode))
        {
            try
            {
                ConflictingSheet = await _repo.FindActiveSheetByJobAsync(Header.JobCode, TsId, ct);
                if (ConflictingSheet is not null)
                    ModelState.AddModelError(string.Empty,
                        $"Job {Header.JobCode} already has {ConflictingSheet.TS_SHEET_NO}. Open it to add more segments.");

                if (!string.IsNullOrWhiteSpace(Report.ReportNo)
                    && await _fsr.FindReportNoOwnerAsync(Report.ReportNo, TsId, ct) is { } owner)
                    ModelState.AddModelError(string.Empty, $"Report No {Report.ReportNo.Trim()} is already used on {owner}.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Duplicate checks failed for field job {JobCode}", Header.JobCode);
            }
        }

        if (!ModelState.IsValid)
        {
            await EnsureJobListedAsync(ct);
            return Page();
        }

        try
        {
            var (header, lineEntities) = await BuildHeaderAndLinesAsync(ct);
            var report = ToEntity(Report, header.TS_VEHICLE_COST);
            var crew = Crew.Select((c, i) => new TM_FIELD_REPORT_CREW
            {
                FRC_SEQ = i + 1,
                FRC_EMP_CODE = c.EmpCode.Trim(),
                FRC_EMP_NAME = c.EmpName,
                FRC_FA_CODE = string.IsNullOrWhiteSpace(c.FaCode) ? null : c.FaCode.Trim()
            }).ToList();
            var userId = User.UserId();
            SheetChildWriter writeFsr = (conn, tx, id, token) => _fsr.WriteAsync(conn, tx, id, report, crew, userId, token);

            if (TsId.HasValue)
            {
                header.TS_ID = TsId.Value;
                await _repo.UpdateTimeSheetAsync(header, lineEntities, userId, ct, writeFsr);
                TempData["Success"] = $"Field service report updated — {lineEntities.Count} segment line(s), total cost {header.TS_TOTAL_COST + header.TS_VEHICLE_COST:N2}.";
                return RedirectToPage("/TimeBooking/Index");
            }

            var sheetNo = await _repo.SaveTimeSheetAsync(header, lineEntities, ct, writeFsr);
            TempData["Success"] = $"Field service report {sheetNo} posted — {lineEntities.Count} segment line(s), total cost {header.TS_TOTAL_COST + header.TS_VEHICLE_COST:N2}.";
            return RedirectToPage("/TimeBooking/Index");
        }
        catch (OracleException ex) when (ex.Number == 1)
        {
            _logger.LogWarning(ex, "Duplicate field sheet for job {JobCode} rejected by the database", Header.JobCode);
            ConflictingSheet = await _repo.FindActiveSheetByJobAsync(Header.JobCode, TsId, ct);
            ModelState.AddModelError(string.Empty,
                $"Job {Header.JobCode} already has {ConflictingSheet?.TS_SHEET_NO}. Open it to add more segments.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saving field service report failed (edit={IsEdit})", TsId.HasValue);
            ModelState.AddModelError(string.Empty,
                "Unable to save the field service report (database unavailable). Please verify the Oracle connection and try again.");
        }

        await EnsureJobListedAsync(ct);
        return Page();
    }

    private void Validate()
    {
        if (Crew.Count == 0)
            ModelState.AddModelError(string.Empty, "Add at least one technician to the crew.");
        if (Lines.Count == 0)
            ModelState.AddModelError(string.Empty, "Add at least one day / segment line before posting.");
        if (Report.KmStart is { } ks && Report.KmEnd is { } ke && ke < ks)
            ModelState.AddModelError(string.Empty, "KM-End cannot be less than KM-Start.");

        for (var i = 0; i < Lines.Count; i++)
        {
            var l = Lines[i];
            var n = i + 1;
            if (!TimeBooking.EntryModel.TryCombine(l.WorkDate, l.StartTime, out _) || !TimeBooking.EntryModel.TryCombine(l.WorkDate, l.EndTime, out _))
                ModelState.AddModelError(string.Empty, $"Line {n}: from and to times must be valid HH:mm values.");
            if (string.IsNullOrWhiteSpace(l.TechCode))
                ModelState.AddModelError(string.Empty, $"Line {n}: technician is required.");
            if (l.SegmentType == SegmentTypes.Work && string.IsNullOrWhiteSpace(l.TaskCode))
                ModelState.AddModelError(string.Empty, $"Line {n}: a Work segment needs a task.");
            if (l.SegmentType != SegmentTypes.Work && l.SegmentType != SegmentTypes.Travel)
                ModelState.AddModelError(string.Empty, $"Line {n}: unknown segment type.");
            if (l.IdleHours < 0 || l.IdleHours > l.NormalHours + l.OtHours)
                ModelState.AddModelError(string.Empty, $"Line {n}: idle hours must be between 0 and the line's net hours.");
        }
    }

    /// <summary>
    /// Recalculates every segment with the engine (Field labour rate for the job's industry); the
    /// user's figures win and deviations flag the line as overridden. Travel segments carry their
    /// net hours as travel hours and no standard hours; idle is informational (part of Net).
    /// </summary>
    private async Task<(TM_TIME_SHEET header, List<TM_TIME_LINE> lines)> BuildHeaderAndLinesAsync(CancellationToken ct)
    {
        var job = await _repo.GetJobAsync(Header.JobCode, ct);
        var industryCode = job?.INDUSTRY_CODE ?? Header.IndustryCode;
        var rate = await _repo.GetLabourRateAsync(FieldLocation, industryCode, ct);
        var duty = TimeBooking.EntryModel.ToDutyTiming(Duty);
        var stdByTask = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var lines = new List<TM_TIME_LINE>();
        decimal totNet = 0, totLabour = 0, totNormal = 0, totOt = 0, totStd = 0, totFood = 0, totTravel = 0, totIdle = 0;

        // Grid order: by date, then start time, then technician.
        var ordered = Lines
            .Select(l => { TimeBooking.EntryModel.TryCombine(l.WorkDate, l.StartTime, out var s); return (l, s); })
            .OrderBy(x => x.s).ThenBy(x => x.l.TechName).Select(x => x.l);

        var lineNo = 0;
        foreach (var l in ordered)
        {
            lineNo++;
            var isTravel = l.SegmentType == SegmentTypes.Travel;
            TimeBooking.EntryModel.TryCombine(l.WorkDate, l.StartTime, out var startDt);
            TimeBooking.EntryModel.TryCombine(l.WorkDate, l.EndTime, out var endDt);
            if (endDt <= startDt) endDt = endDt.AddDays(1);
            var lunch = Math.Max(0m, l.LunchHours);

            var calc = TimeBookingCalculator.Calculate(
                new TimeLineCalcInput(startDt, endDt, lunch, rate.Rate, rate.OvertimeMultiplier), duty);

            var normal = R2(Math.Max(0m, l.NormalHours));
            var ot = R2(Math.Max(0m, l.OtHours));
            var normalRate = R2(Math.Max(0m, l.Rate));
            var otRate = R2(Math.Max(0m, l.OtRate));
            var labour = R2(Math.Max(0m, l.LabourCost));
            var food = R2(Math.Max(0m, l.FoodAllowance));
            var net = R2(normal + ot);
            var idle = R2(Math.Clamp(l.IdleHours, 0m, net));

            decimal std = 0m;
            if (!isTravel && !string.IsNullOrWhiteSpace(l.TaskCode) && !stdByTask.TryGetValue(l.TaskCode, out std))
                stdByTask[l.TaskCode] = std = await _repo.GetTaskStdHoursAsync(l.TaskCode, ct);

            // Travel: the segment itself is travel time. Lines carried over from the old screen keep their travel fields.
            var travelHours = isTravel ? net : R2(Math.Max(0m, l.TravelHours));
            DateTime? travelStart = isTravel ? startDt
                : TimeBooking.EntryModel.TryCombine(l.WorkDate, l.TravelStart, out var ts) ? ts : null;
            DateTime? travelEnd = isTravel ? endDt
                : TimeBooking.EntryModel.TryCombine(l.WorkDate, l.TravelEnd, out var te) ? te : null;
            if (!isTravel && travelStart.HasValue && travelEnd.HasValue && travelEnd <= travelStart) travelEnd = travelEnd.Value.AddDays(1);

            var overridden = normal != calc.NormalHours || ot != calc.OtHours ||
                             normalRate != calc.NormalRate || otRate != calc.OtRate ||
                             labour != TimeBookingCalculator.Cost(normal, ot, normalRate, otRate);
            var timeType = ot <= 0m ? "NORMAL" : normal <= 0m ? "OVERTIME" : "MIXED";

            totNet += net; totLabour += labour; totNormal += normal; totOt += ot;
            totStd += std; totFood += food; totTravel += travelHours; totIdle += idle;

            lines.Add(new TM_TIME_LINE
            {
                TL_LINE_NO = lineNo,
                TL_SEGMENT_TYPE = isTravel ? SegmentTypes.Travel : SegmentTypes.Work,
                TL_TASK_CODE = isTravel ? null : l.TaskCode,
                TL_TASK_NAME = isTravel ? "Travel" : l.TaskName,
                TL_STD_HOURS = std,
                TL_TECH_CODE = l.TechCode,
                TL_TECH_NAME = l.TechName,
                TL_SKILL = l.Skill,
                TL_START_DT = startDt,
                TL_END_DT = endDt,
                TL_LUNCH_HOURS = lunch,
                TL_NET_HOURS = net,
                TL_TIME_TYPE = timeType,
                TL_RATE = normalRate,
                TL_LABOUR_COST = labour,
                TL_JOB_CODE = Header.JobCode,
                TL_LOCATION = FieldLocation,
                TL_WORK_DATE = l.WorkDate.Date,
                TL_NORMAL_HOURS = normal,
                TL_OT_HOURS = ot,
                TL_OT_RATE = otRate,
                TL_FOOD_ALLOWANCE = food,
                TL_TOTAL_COST = R2(labour + food),
                TL_TRAVEL_SITE = string.IsNullOrWhiteSpace(l.TravelSite) ? null : l.TravelSite.Trim(),
                TL_TRAVEL_START = travelStart,
                TL_TRAVEL_END = travelEnd,
                TL_TRAVEL_HOURS = travelHours,
                TL_IDLE_HOURS = idle,
                TL_IDLE_REASON = idle > 0 && !string.IsNullOrWhiteSpace(l.IdleReason) ? l.IdleReason.Trim() : null,
                TL_OVERRIDE_YN = overridden ? "Y" : "N"
            });
        }

        var (_, _, vehicleCost) = VehicleCost(Report.KmStart, Report.KmEnd, Report.KmRate);
        var brand = job?.BRAND_DESC ?? job?.MTJ_BRAND ?? Header.Brand;
        var equipmentType = job?.EQUIPMENT_TYPE_DESC ?? Header.EquipmentType;
        var composed = string.Join(" · ", new[] { brand, equipmentType }.Where(x => !string.IsNullOrWhiteSpace(x)));

        var header = new TM_TIME_SHEET
        {
            TS_POSTING_DATE = Header.PostingDate,
            TS_JOB_CODE = Header.JobCode,
            TS_CUSTOMER_CODE = job?.MTJ_PARTY_CODE,
            TS_CUSTOMER_NAME = job?.CUSTOMER_NAME ?? Header.CustomerName,
            TS_EQUIPMENT = composed.Length == 0 ? null : composed,
            TS_INDUSTRY_CODE = industryCode,
            TS_BRAND = brand,
            TS_EQUIPMENT_TYPE = equipmentType,
            TS_SERVICE_TYPE = job?.SERVICE_TYPE_DESC ?? Header.ServiceType,
            TS_SERIAL_NO = job?.MTJ_SERIAL_NO ?? Header.SerialNo,
            TS_JOB_OPENING_DATE = job?.MTJ_JOB_OPENING_DATE,
            TS_LOCATION = FieldLocation,
            TS_SHEET_TYPE = SheetTypes.Field,
            TS_WORK_TYPE_CODE = Header.WorkTypeCode,
            TS_JOB_STATUS = Header.JobStatus,
            TS_TOTAL_NET_HOURS = totNet,
            TS_TOTAL_LABOUR_COST = totLabour,
            TS_NORMAL_HOURS = totNormal,
            TS_OT_HOURS = totOt,
            TS_STD_HOURS = totStd,
            TS_TOTAL_FOOD_ALLOWANCE = totFood,
            TS_TOTAL_TRAVEL_HOURS = totTravel,
            TS_TOTAL_IDLE_HOURS = totIdle,
            TS_TOTAL_COST = R2(totLabour + totFood),
            TS_VEHICLE_COST = vehicleCost,
            TS_REMARKS = Header.Remarks,
            TS_CREATION_USER_ID = User.UserId()
        };
        return (header, lines);
    }

    /// <summary>Total km (end − start, never negative), rate and cost = km × rate.</summary>
    public static (decimal km, decimal rate, decimal cost) VehicleCost(decimal? kmStart, decimal? kmEnd, decimal? kmRate)
    {
        var km = kmStart is { } s && kmEnd is { } e && e > s ? e - s : 0m;
        var rate = Math.Max(0m, kmRate ?? 0m);
        return (km, rate, R2(km * rate));
    }

    private static TM_FIELD_REPORT ToEntity(ReportInput r, decimal vehicleCost)
    {
        var (km, rate, _) = VehicleCost(r.KmStart, r.KmEnd, r.KmRate);
        static string? T(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        return new TM_FIELD_REPORT
        {
            FR_REPORT_NO = T(r.ReportNo),
            FR_REPORT_DATE = r.ReportDate,
            FR_JOB_SITE = T(r.JobSite),
            FR_CONTACT_PERSON = T(r.ContactPerson),
            FR_CONTACT_POSITION = T(r.ContactPosition),
            FR_CONTACT_PHONE = T(r.ContactPhone),
            FR_OEM = T(r.Oem),
            FR_MODEL = T(r.Model),
            FR_SMU_HOURS = r.SmuHours,
            FR_UNDER_WARRANTY_YN = r.UnderWarranty ? "Y" : "N",
            FR_TAI_REPORT = T(r.TaiReport),
            FR_JOB_TYPE = T(r.JobType),
            FR_FLEET_NO = T(r.FleetNo),
            FR_PLATE_NO = T(r.PlateNo),
            FR_DRIVER_NAME = T(r.DriverName),
            FR_KM_START = r.KmStart,
            FR_KM_END = r.KmEnd,
            FR_TOTAL_KM = km,
            FR_KM_RATE = rate,
            FR_VEHICLE_COST = vehicleCost,
            FR_COMPLAINT = T(r.Complaint),
            FR_PROBLEMS_FOUND = T(r.ProblemsFound),
            FR_PROBLEM_CAUSES = T(r.ProblemCauses),
            FR_CORRECTION = T(r.Correction),
            FR_OTHER_ITEMS = T(r.OtherItems),
            FR_JOB_COMPLETED_YN = r.JobCompleted ? "Y" : "N",
            FR_TO_WORKSHOP_YN = r.BroughtToWorkshop ? "Y" : "N",
            FR_SERVICE_ENGINEER = T(r.ServiceEngineer),
            FR_SERVICE_MANAGER = T(r.ServiceManager),
            FR_SIGN_DATE = r.SignDate,
            FR_CUST_COMMENTS = T(r.CustomerComments),
            FR_CUST_REP = T(r.CustomerRep),
            FR_CUST_REP_POSITION = T(r.CustomerRepPosition),
            FR_CUST_SIGN_DATE = r.CustomerSignDate
        };
    }

    private static ReportInput FromEntity(TM_FIELD_REPORT f) => new()
    {
        ReportNo = f.FR_REPORT_NO,
        ReportDate = f.FR_REPORT_DATE,
        JobSite = f.FR_JOB_SITE,
        ContactPerson = f.FR_CONTACT_PERSON,
        ContactPosition = f.FR_CONTACT_POSITION,
        ContactPhone = f.FR_CONTACT_PHONE,
        Oem = f.FR_OEM,
        Model = f.FR_MODEL,
        SmuHours = f.FR_SMU_HOURS,
        UnderWarranty = f.FR_UNDER_WARRANTY_YN == "Y",
        TaiReport = f.FR_TAI_REPORT,
        JobType = f.FR_JOB_TYPE,
        FleetNo = f.FR_FLEET_NO,
        PlateNo = f.FR_PLATE_NO,
        DriverName = f.FR_DRIVER_NAME,
        KmStart = f.FR_KM_START,
        KmEnd = f.FR_KM_END,
        KmRate = f.FR_KM_RATE,
        Complaint = f.FR_COMPLAINT,
        ProblemsFound = f.FR_PROBLEMS_FOUND,
        ProblemCauses = f.FR_PROBLEM_CAUSES,
        Correction = f.FR_CORRECTION,
        OtherItems = f.FR_OTHER_ITEMS,
        JobCompleted = f.FR_JOB_COMPLETED_YN == "Y",
        BroughtToWorkshop = f.FR_TO_WORKSHOP_YN == "Y",
        ServiceEngineer = f.FR_SERVICE_ENGINEER,
        ServiceManager = f.FR_SERVICE_MANAGER,
        SignDate = f.FR_SIGN_DATE,
        CustomerComments = f.FR_CUST_COMMENTS,
        CustomerRep = f.FR_CUST_REP,
        CustomerRepPosition = f.FR_CUST_REP_POSITION,
        CustomerSignDate = f.FR_CUST_SIGN_DATE
    };

    private static SegmentInput ToInput(TM_TIME_LINE l) => new()
    {
        SegmentType = l.TL_SEGMENT_TYPE == SegmentTypes.Travel ? SegmentTypes.Travel : SegmentTypes.Work,
        WorkDate = l.TL_WORK_DATE ?? l.TL_START_DT.Date,
        StartTime = l.TL_START_DT.ToString("HH:mm"),
        EndTime = l.TL_END_DT.ToString("HH:mm"),
        LunchHours = l.TL_LUNCH_HOURS,
        IdleHours = l.TL_IDLE_HOURS,
        IdleReason = l.TL_IDLE_REASON,
        TaskCode = l.TL_TASK_CODE,
        TaskName = l.TL_TASK_NAME,
        TechCode = l.TL_TECH_CODE,
        TechName = l.TL_TECH_NAME,
        Skill = l.TL_SKILL,
        NetHours = l.TL_NET_HOURS,
        NormalHours = l.TL_NORMAL_HOURS,
        OtHours = l.TL_OT_HOURS,
        Rate = l.TL_RATE,
        OtRate = l.TL_OT_RATE,
        LabourCost = l.TL_LABOUR_COST,
        FoodAllowance = l.TL_FOOD_ALLOWANCE,
        TotalCost = l.TL_TOTAL_COST,
        StdHours = l.TL_STD_HOURS,
        TimeType = l.TL_TIME_TYPE,
        Overridden = l.TL_OVERRIDE_YN == "Y",
        // Legacy travel-on-a-work-line values (pre-FSR) are carried through unchanged.
        TravelSite = l.TL_SEGMENT_TYPE == SegmentTypes.Travel ? null : l.TL_TRAVEL_SITE,
        TravelStart = l.TL_SEGMENT_TYPE == SegmentTypes.Travel ? null : l.TL_TRAVEL_START?.ToString("HH:mm"),
        TravelEnd = l.TL_SEGMENT_TYPE == SegmentTypes.Travel ? null : l.TL_TRAVEL_END?.ToString("HH:mm"),
        TravelHours = l.TL_SEGMENT_TYPE == SegmentTypes.Travel ? 0m : l.TL_TRAVEL_HOURS
    };

    private bool CanEdit(TimeSheetEditDto dto)
    {
        if (IsAdmin) return true;
        if (dto.TS_CREATION_USER_ID == User.UserId()) return true;
        var emp = User.EmpCode();
        return emp is not null && dto.Lines.Any(l => string.Equals(l.TL_TECH_CODE, emp, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The Field job list excludes CLOSED jobs; keep the sheet's own job selectable.</summary>
    private async Task EnsureJobListedAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(Header.JobCode) || IsOffline || Jobs.Any(j => j.MTJ_JOB_CODE == Header.JobCode)) return;
        try
        {
            var job = await _repo.GetJobAsync(Header.JobCode, ct);
            if (job is not null) Jobs = Jobs.Prepend(job).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Job lookup failed for {JobCode}", Header.JobCode);
        }
    }

    private async Task LoadLookupsAsync(CancellationToken ct)
    {
        try
        {
            Jobs = await _repo.GetJobsAsync(FieldLocation, ct);
            Technicians = await _repo.GetTechniciansAsync(ct);
            Tasks = await _repo.GetTasksAsync(ct);
            WorkTypes = await _repo.GetWorkTypesAsync(ct);
            Duty = await _repo.GetDutyTimingAsync(ct);
            ExistingSheetNos = await _repo.GetActiveSheetNosByJobAsync(ct);
            DefaultKmRate = (await _repo.GetLabourRateAsync(FieldLocation, null, ct)).KmRate;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Field service lookups unavailable");
            IsOffline = true;
        }
    }

    private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
