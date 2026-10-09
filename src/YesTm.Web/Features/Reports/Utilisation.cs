using System.Data;
using Dapper;
using YesTm.Web.Common.Availability;
using YesTm.Web.Common.Data;
using YesTm.Web.Features.TimeBooking;

namespace YesTm.Web.Features.Reports;

/// <summary>One technician's availability and booking KPIs for a period.</summary>
public sealed record TechUtilisation(string TechCode, string? TechName, string? HrmsCode, UtilisationKpi Kpi)
{
    public bool Mapped => HrmsCode is not null;
}

/// <summary>Utilisation for a whole report: the period used, the standard day, and one row per technician.</summary>
public sealed record UtilisationReport(DateTime From, DateTime To, bool PeriodFromData, decimal StdHoursPerDay,
    IReadOnlySet<DayOfWeek> Weekend, IReadOnlyList<TechUtilisation> Technicians)
{
    public string PeriodText => $"{From:dd-MMM-yyyy} to {To:dd-MMM-yyyy}" + (PeriodFromData ? " (first to last booked work date — no date filter given)" : "");
    public string WeekendText => string.Join(", ", Weekend.OrderBy(d => ((int)d + 1) % 7).Select(d => d.ToString()));
}

public interface IUtilisationService
{
    /// <summary>
    /// Builds availability + KPIs for the technicians in <paramref name="rows"/>; when
    /// <paramref name="includeAllMapped"/> is set (admins), technicians with a confirmed HRMS mapping but
    /// no bookings in the period are listed too (0 % utilisation is the point of the report).
    /// </summary>
    Task<UtilisationReport> BuildAsync(IReadOnlyList<TaskTimeReportRow> rows, DateTime? from, DateTime? to,
        bool includeAllMapped, CancellationToken ct = default);
}

public sealed class UtilisationService : IUtilisationService
{
    private readonly IDbConnectionFactory _db;
    private readonly ITimeBookingRepository _booking;
    private readonly ILogger<UtilisationService> _logger;

    public UtilisationService(IDbConnectionFactory db, ITimeBookingRepository booking, ILogger<UtilisationService> logger)
    {
        _db = db;
        _booking = booking;
        _logger = logger;
    }

    private sealed class AttRow
    {
        public string ECM_TM_EMP_CODE { get; set; } = "";
        public string ATT_HRMS_EMP_CODE { get; set; } = "";
        public DateTime ATT_DATE { get; set; }
        public string? ATT_DAY_STATUS { get; set; }
        public decimal ATT_LEAVE_DAYS { get; set; }
        public decimal ATT_HOLIDAY_DAYS { get; set; }
        public decimal ATT_ABSENT_DAYS { get; set; }
        public decimal ATT_ACTUAL_HOURS { get; set; }
        public string? ATT_LEAVE_NAME { get; set; }
        public string? ATT_HOLIDAY_NAME { get; set; }
    }

    public async Task<UtilisationReport> BuildAsync(IReadOnlyList<TaskTimeReportRow> rows, DateTime? from, DateTime? to,
        bool includeAllMapped, CancellationToken ct = default)
    {
        // Period: the filter, else first..last booked work date, else this month.
        var dates = rows.Select(r => r.TL_WORK_DATE ?? r.TL_START_DT).Where(d => d.HasValue).Select(d => d!.Value.Date).ToList();
        var fromData = from is null || to is null;
        var f = from?.Date ?? (dates.Count > 0 ? dates.Min() : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
        var t = to?.Date ?? (dates.Count > 0 ? dates.Max() : DateTime.Today);
        if (t < f) (f, t) = (t, f);

        var duty = await _booking.GetDutyTimingAsync(ct);
        var weekend = TimeBooking.EntryModel.ToDutyTiming(duty).WeekendDays;
        var stdDay = duty.STD_HOURS_PER_DAY > 0 ? duty.STD_HOURS_PER_DAY : 8m;

        // Confirmed TM → HRMS mappings + their attendance in the period. Tables may not exist yet (10 not run).
        var maps = new Dictionary<string, (string Hrms, string? Name)>(StringComparer.OrdinalIgnoreCase);
        var att = new Dictionary<string, Dictionary<DateTime, AttendanceDay>>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var conn = await _db.CreateOpenConnectionAsync(ct);
            var m = await conn.QueryAsync<(string ECM_TM_EMP_CODE, string ECM_HRMS_EMP_CODE, string? PEMP_EMP_NAME)>(new CommandDefinition(@"
                SELECT m.ECM_TM_EMP_CODE, m.ECM_HRMS_EMP_CODE, e.PEMP_EMP_NAME
                FROM   TM_EMP_CODE_MAP m
                LEFT JOIN PPM_EMPLOYEE_DETAILS e ON e.PEMP_EMP_CODE = m.ECM_TM_EMP_CODE
                WHERE  m.ECM_STATUS = 'CONFIRMED' AND m.ECM_HRMS_EMP_CODE IS NOT NULL", cancellationToken: ct));
            foreach (var x in m) maps[x.ECM_TM_EMP_CODE] = (x.ECM_HRMS_EMP_CODE, x.PEMP_EMP_NAME);

            var p = new DynamicParameters();
            p.Add("fromDate", f, DbType.Date);
            p.Add("toExcl", t.AddDays(1), DbType.Date);
            var a = await conn.QueryAsync<AttRow>(new CommandDefinition(@"
                SELECT m.ECM_TM_EMP_CODE, a.ATT_HRMS_EMP_CODE, a.ATT_DATE, a.ATT_DAY_STATUS, a.ATT_LEAVE_DAYS,
                       a.ATT_HOLIDAY_DAYS, a.ATT_ABSENT_DAYS, a.ATT_ACTUAL_HOURS, a.ATT_LEAVE_NAME, a.ATT_HOLIDAY_NAME
                FROM   TM_EMP_CODE_MAP m
                JOIN   TM_ATTENDANCE a ON a.ATT_HRMS_EMP_CODE = m.ECM_HRMS_EMP_CODE
                WHERE  m.ECM_STATUS = 'CONFIRMED'
                  AND  a.ATT_DATE >= :fromDate AND a.ATT_DATE < :toExcl", p, cancellationToken: ct));
            foreach (var x in a)
            {
                if (!att.TryGetValue(x.ECM_TM_EMP_CODE, out var days)) att[x.ECM_TM_EMP_CODE] = days = [];
                days[x.ATT_DATE.Date] = new AttendanceDay(x.ATT_DATE.Date, x.ATT_DAY_STATUS ?? "OTHER", x.ATT_LEAVE_DAYS,
                    x.ATT_HOLIDAY_DAYS, x.ATT_ABSENT_DAYS, x.ATT_ACTUAL_HOURS, x.ATT_LEAVE_NAME, x.ATT_HOLIDAY_NAME);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HRMS attendance unavailable — availability falls back to the calendar");
        }

        // Booked figures per technician, only for lines inside the period.
        var inPeriod = rows.Where(r => (r.TL_WORK_DATE ?? r.TL_START_DT)?.Date is { } d && d >= f && d <= t).ToList();
        var earned = TaskTimeExcel.EarnedStdByTech(inPeriod);
        var techs = inPeriod.GroupBy(r => r.TL_TECH_CODE ?? "")
            .ToDictionary(g => g.Key, g => (Name: g.Select(x => x.TL_TECH_NAME).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)), Rows: g.ToList()),
                StringComparer.OrdinalIgnoreCase);
        if (includeAllMapped)
            foreach (var (code, (_, name)) in maps)
                techs.TryAdd(code, (name, []));

        var list = techs.Select(kv =>
            {
                maps.TryGetValue(kv.Key, out var map);
                var avail = AvailabilityCalculator.Calculate(f, t, stdDay, weekend, att.GetValueOrDefault(kv.Key));
                var r = kv.Value.Rows;
                var work = r.Where(x => x.TL_SEGMENT_TYPE != "TRAVEL").Sum(x => x.TL_NET_HOURS - x.TL_IDLE_HOURS);
                var travel = r.Where(x => x.TL_SEGMENT_TYPE == "TRAVEL").Sum(x => x.TL_NET_HOURS)
                           + r.Where(x => x.TL_SEGMENT_TYPE != "TRAVEL").Sum(x => x.TL_TRAVEL_HOURS);
                var kpi = new UtilisationKpi(avail, r.Sum(x => x.TL_NET_HOURS), r.Sum(x => x.TL_NORMAL_HOURS), r.Sum(x => x.TL_OT_HOURS),
                    work, travel, r.Sum(x => x.TL_IDLE_HOURS), earned.GetValueOrDefault(kv.Key));
                return new TechUtilisation(kv.Key, kv.Value.Name ?? map.Name, map.Hrms, kpi);
            })
            .OrderBy(x => x.TechName).ThenBy(x => x.TechCode)
            .ToList();

        return new UtilisationReport(f, t, fromData, stdDay, weekend, list);
    }
}
