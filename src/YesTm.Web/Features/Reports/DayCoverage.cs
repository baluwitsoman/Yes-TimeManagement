using System.Data;
using Dapper;
using YesTm.Web.Common.Data;
using YesTm.Web.Features.TimeBooking;

namespace YesTm.Web.Features.Reports;

/// <summary>What one technician booked on one date (all active sheets — job and non-job).</summary>
public sealed record BookedDay(DateTime Date, decimal Hours, decimal NonJobHours, string? NonJobNames);

public static class DayStatus
{
    public const string Weekend = "Weekend";
    public const string Missed = "Missed";       // working day, nothing booked
    public const string Short = "Short";         // booked, but less than the standard day
    public const string Full = "Full";
}

/// <summary>One technician's day-by-day booking check for a period.</summary>
public sealed record TechDayCoverage(string TechCode, string? TechName, IReadOnlyList<(DateTime Date, string Status, BookedDay? Booked)> Days)
{
    public int WorkingDays => Days.Count(d => d.Status != DayStatus.Weekend);
    public int BookedDays => Days.Count(d => d.Status is DayStatus.Full or DayStatus.Short);
    public int MissedDays => Days.Count(d => d.Status == DayStatus.Missed);
    public int ShortDays => Days.Count(d => d.Status == DayStatus.Short);
    public int WeekendDaysWorked => Days.Count(d => d.Status == DayStatus.Weekend && d.Booked is { Hours: > 0 });
}

public sealed record DayCoverageReport(DateTime From, DateTime To, decimal StdHoursPerDay, IReadOnlySet<DayOfWeek> Weekend,
    IReadOnlyList<TechDayCoverage> Technicians);

/// <summary>Pure day-by-day classification (unit-tested).</summary>
public static class DayCoverageCalculator
{
    /// <summary>
    /// Every date in [from, to]: a weekend day (Duty Timing) is "Weekend" whatever was booked; a working
    /// day with no hours is "Missed"; with fewer hours than the standard day "Short"; otherwise "Full".
    /// Leave / holiday / off booked as non-job time counts as booked — a day of leave is not missed.
    /// </summary>
    public static TechDayCoverage Calculate(string techCode, string? techName, DateTime from, DateTime to,
        IReadOnlySet<DayOfWeek> weekend, decimal stdHoursPerDay, IReadOnlyDictionary<DateTime, BookedDay> booked)
    {
        var days = new List<(DateTime, string, BookedDay?)>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            booked.TryGetValue(d, out var b);
            var status = weekend.Contains(d.DayOfWeek) ? DayStatus.Weekend
                       : b is null || b.Hours <= 0 ? DayStatus.Missed
                       : b.Hours < stdHoursPerDay ? DayStatus.Short
                       : DayStatus.Full;
            days.Add((d, status, b));
        }
        return new TechDayCoverage(techCode, techName, days);
    }
}

public interface IDayCoverageService
{
    /// <param name="onlyTechCode">Normal users: just themselves. Null (admins): every technician expected to book.</param>
    Task<DayCoverageReport> BuildAsync(DateTime from, DateTime to, string? onlyTechCode, CancellationToken ct = default);
}

public sealed class DayCoverageService : IDayCoverageService
{
    private readonly IDbConnectionFactory _db;
    private readonly ITimeBookingRepository _booking;

    public DayCoverageService(IDbConnectionFactory db, ITimeBookingRepository booking)
    {
        _db = db;
        _booking = booking;
    }

    public async Task<DayCoverageReport> BuildAsync(DateTime from, DateTime to, string? onlyTechCode, CancellationToken ct = default)
    {
        from = from.Date; to = to.Date;
        if (to < from) (from, to) = (to, from);
        var duty = await _booking.GetDutyTimingAsync(ct);
        var weekend = TimeBooking.EntryModel.ToDutyTiming(duty).WeekendDays;
        var stdDay = duty.STD_HOURS_PER_DAY > 0 ? duty.STD_HOURS_PER_DAY : 8m;

        using var conn = await _db.CreateOpenConnectionAsync(ct);

        // Every booking of the period (ignores any job / customer search on the report): one row per technician-day.
        var p = new DynamicParameters();
        p.Add("fromDate", from, DbType.Date);
        p.Add("toExcl", to.AddDays(1), DbType.Date);
        p.Add("tech", onlyTechCode, DbType.String);
        var rows = await conn.QueryAsync<(string TL_TECH_CODE, string? TL_TECH_NAME, DateTime WORK_DATE, decimal HOURS, decimal NJ_HOURS, string? NJ_NAMES)>(
            new CommandDefinition(@"
                SELECT l.TL_TECH_CODE, MAX(l.TL_TECH_NAME) AS TL_TECH_NAME,
                       NVL(l.TL_WORK_DATE, TRUNC(l.TL_START_DT)) AS WORK_DATE,
                       SUM(NVL(l.TL_NET_HOURS,0)) AS HOURS,
                       SUM(CASE WHEN s.TS_SHEET_TYPE = 'NONJOB' THEN NVL(l.TL_NET_HOURS,0) ELSE 0 END) AS NJ_HOURS,
                       LISTAGG(DISTINCT CASE WHEN s.TS_SHEET_TYPE = 'NONJOB' THEN l.TL_TASK_NAME END, ', ')
                           WITHIN GROUP (ORDER BY l.TL_TASK_NAME) AS NJ_NAMES
                FROM   TM_TIME_LINE l
                JOIN   TM_TIME_SHEET s ON s.TS_ID = l.TL_TS_ID
                WHERE  NVL(s.TS_ACTIVE_YN,'Y') = 'Y'
                  AND  l.TL_TECH_CODE IS NOT NULL
                  AND  NVL(l.TL_WORK_DATE, TRUNC(l.TL_START_DT)) >= :fromDate
                  AND  NVL(l.TL_WORK_DATE, TRUNC(l.TL_START_DT)) <  :toExcl
                  AND  (:tech IS NULL OR l.TL_TECH_CODE = :tech)
                GROUP  BY l.TL_TECH_CODE, NVL(l.TL_WORK_DATE, TRUNC(l.TL_START_DT))", p, cancellationToken: ct));

        var byTech = rows.GroupBy(r => r.TL_TECH_CODE, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (Name: g.Select(x => x.TL_TECH_NAME).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                Days: (IReadOnlyDictionary<DateTime, BookedDay>)g.ToDictionary(x => x.WORK_DATE.Date,
                    x => new BookedDay(x.WORK_DATE.Date, x.HOURS, x.NJ_HOURS, x.NJ_NAMES))), StringComparer.OrdinalIgnoreCase);

        // Who is expected to book: active (de-duplicated) technicians + anyone who has ever booked time.
        var expected = onlyTechCode is not null
            ? [(onlyTechCode, (string?)null)]
            : (await conn.QueryAsync<(string PEMP_EMP_CODE, string? PEMP_EMP_NAME)>(new CommandDefinition(@"
                SELECT d.PEMP_EMP_CODE, d.PEMP_EMP_NAME
                FROM   " + EmployeeSql.Distinct + @" d
                JOIN   PPM_EMPLOYEE_DETAILS e ON e.PEMP_EMP_CODE = d.PEMP_EMP_CODE
                WHERE  NVL(d.PEMP_EMP_ACTIVE,'Y') = 'Y'
                  AND  (UPPER(e.PEMP_WORKING_STATUS) = 'TECHNICIAN'
                        OR EXISTS (SELECT 1 FROM TM_TIME_LINE l WHERE l.TL_TECH_CODE = d.PEMP_EMP_CODE))", cancellationToken: ct)))
              .Select(x => (x.PEMP_EMP_CODE, x.PEMP_EMP_NAME)).ToList();

        var codes = expected.Select(e => e.Item1).Concat(byTech.Keys).Distinct(StringComparer.OrdinalIgnoreCase);
        var names = expected.ToDictionary(e => e.Item1, e => e.Item2, StringComparer.OrdinalIgnoreCase);
        var empty = new Dictionary<DateTime, BookedDay>();

        var list = codes.Select(code =>
            {
                byTech.TryGetValue(code, out var b);
                return DayCoverageCalculator.Calculate(code, b.Name ?? names.GetValueOrDefault(code), from, to, weekend, stdDay, b.Days ?? empty);
            })
            .OrderBy(t => (t.TechName ?? t.TechCode).ToUpperInvariant()).ToList();

        return new DayCoverageReport(from, to, stdDay, weekend, list);
    }
}
