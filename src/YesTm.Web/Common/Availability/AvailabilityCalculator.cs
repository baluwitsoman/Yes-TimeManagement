namespace YesTm.Web.Common.Availability;

/// <summary>One employee-day of HRMS attendance, as far as availability needs it.</summary>
public sealed record AttendanceDay(
    DateTime Date, string Status, decimal LeaveDays, decimal HolidayDays, decimal AbsentDays,
    decimal ActualHours, string? LeaveName, string? HolidayName);

/// <summary>
/// How many days / hours a technician could have worked in a period, built up step by step so a
/// report can print the whole chain:
///   Calendar days − Weekend/OFF − Public holidays = Scheduled working days
///   Scheduled − Leave − Absent                   = Available days
///   Available days × Std hours/day               = Available hours
/// </summary>
public sealed record Availability(
    DateTime From, DateTime To, decimal StdHoursPerDay,
    decimal CalendarDays, decimal OffDays, decimal HolidayDays, decimal ScheduledDays,
    decimal LeaveDays, decimal AbsentDays, decimal AvailableDays,
    decimal AttendanceHours, int HrmsDays, string Source,
    IReadOnlyDictionary<string, decimal> LeaveByType, IReadOnlyList<string> Holidays)
{
    public decimal ScheduledHours => Math.Round(ScheduledDays * StdHoursPerDay, 2);
    public decimal AvailableHours => Math.Round(AvailableDays * StdHoursPerDay, 2);

    /// <summary>"31 cal − 9 off − 1 hol = 21 sched − 1 leave − 0 absent = 20 avail × 8.00 h = 160.00 h"</summary>
    public string Formula =>
        $"{D(CalendarDays)} calendar − {D(OffDays)} weekend/off − {D(HolidayDays)} holiday = {D(ScheduledDays)} scheduled; " +
        $"− {D(LeaveDays)} leave − {D(AbsentDays)} absent = {D(AvailableDays)} available × {StdHoursPerDay:0.00} h = {AvailableHours:N2} h";

    public string LeaveText => LeaveByType.Count == 0 ? "" :
        string.Join(", ", LeaveByType.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {D(kv.Value)}"));

    private static string D(decimal d) => d == Math.Floor(d) ? d.ToString("0") : d.ToString("0.#");
}

public static class AvailabilitySources
{
    public const string Hrms = "HRMS attendance";
    public const string Partial = "HRMS + calendar";
    public const string Calendar = "Calendar estimate (no HRMS data)";
}

/// <summary>Pure availability arithmetic (unit-tested); days without HRMS attendance fall back to the duty-timing weekend.</summary>
public static class AvailabilityCalculator
{
    private static readonly HashSet<string> Worked = new(StringComparer.OrdinalIgnoreCase)
        { "PRESENT", "LATE", "PERM", "SPECIAL", "MANUAL", "OTHER" };

    public static Availability Calculate(DateTime from, DateTime to, decimal stdHoursPerDay,
        IReadOnlySet<DayOfWeek> weekend, IReadOnlyDictionary<DateTime, AttendanceDay>? attendance)
    {
        from = from.Date; to = to.Date;
        if (to < from) (from, to) = (to, from);

        decimal cal = 0, off = 0, hol = 0, leave = 0, absent = 0, actual = 0;
        var hrmsDays = 0;
        var leaveByType = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var holidays = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var d = from; d <= to; d = d.AddDays(1))
        {
            cal++;
            if (attendance is not null && attendance.TryGetValue(d, out var a))
            {
                hrmsDays++;
                actual += a.ActualHours;
                switch (a.Status.ToUpperInvariant())
                {
                    case "OFF":
                        off += 1;
                        break;
                    case "HOLIDAY":
                        hol += Clamp(a.HolidayDays);
                        if (!string.IsNullOrWhiteSpace(a.HolidayName)) holidays.Add($"{d:dd-MMM} {a.HolidayName}");
                        break;
                    case "LEAVE":
                        var l = Clamp(a.LeaveDays);
                        leave += l;
                        var key = string.IsNullOrWhiteSpace(a.LeaveName) ? "Leave" : a.LeaveName!.Trim();
                        leaveByType[key] = leaveByType.GetValueOrDefault(key) + l;
                        break;
                    case "ABSENT":
                        absent += Clamp(a.AbsentDays);
                        break;
                    default:
                        if (!Worked.Contains(a.Status) && weekend.Contains(d.DayOfWeek)) off += 1;
                        break;
                }
            }
            else if (weekend.Contains(d.DayOfWeek))
            {
                off += 1;
            }
        }

        var scheduled = cal - off - hol;
        var available = Math.Max(0m, scheduled - leave - absent);
        var source = hrmsDays == 0 ? AvailabilitySources.Calendar
                   : hrmsDays < cal ? AvailabilitySources.Partial
                   : AvailabilitySources.Hrms;

        return new Availability(from, to, stdHoursPerDay, cal, off, hol, scheduled, leave, absent, available,
            Math.Round(actual, 2), hrmsDays, source, leaveByType, holidays.ToList());
    }

    private static decimal Clamp(decimal v) => v is > 0m and <= 1m ? v : 1m;
}

/// <summary>
/// Availability + what was booked → the standard KPIs. Every ratio is null when its base is zero.
///   Paid hrs     = Available hrs + OT booked        (the time the technician was actually on the clock)
///   Utilisation  = Productive work hrs ÷ Paid hrs
///   Efficiency   = Standard (earned) hrs ÷ Productive work hrs
///   Productivity = Standard (earned) hrs ÷ Paid hrs  (= Utilisation × Efficiency)
///   Coverage     = Booked normal hrs ÷ Available hrs (how much of the normal day is accounted for)
/// </summary>
public sealed record UtilisationKpi(
    Availability Avail, decimal BookedNet, decimal BookedNormal, decimal BookedOt,
    decimal WorkHours, decimal TravelHours, decimal IdleHours, decimal StdEarned)
{
    public decimal UnbookedHours => Math.Max(0m, Avail.AvailableHours - BookedNormal);
    public decimal PaidHours => Avail.AvailableHours + BookedOt;
    public decimal? UtilisationPct => Pct(WorkHours, PaidHours);
    public decimal? EfficiencyPct => Pct(StdEarned, WorkHours);
    public decimal? ProductivityPct => Pct(StdEarned, PaidHours);
    public decimal? CoveragePct => Pct(BookedNormal, Avail.AvailableHours);
    /// <summary>Punched (HRMS) hours minus booked hours — time at work that no job carries.</summary>
    public decimal? ClockedNotBooked => Avail.HrmsDays > 0 ? Math.Round(Avail.AttendanceHours - BookedNet, 2) : null;

    private static decimal? Pct(decimal num, decimal den) => den > 0 ? Math.Round(num / den * 100m, 1) : null;
}
