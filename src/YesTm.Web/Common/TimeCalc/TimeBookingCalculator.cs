namespace YesTm.Web.Common.TimeCalc;

public enum TimeType
{
    Normal,
    Overtime,
    /// <summary>Part of the line falls inside the duty window and part outside.</summary>
    Mixed
}

/// <summary>Duty-timing rules that drive Net-hours and Normal/Overtime classification.</summary>
public sealed record DutyTiming(
    TimeOnly DutyStart,
    TimeOnly DutyEnd,
    decimal DefaultLunchHours,
    IReadOnlySet<DayOfWeek> WeekendDays);

public sealed record TimeLineCalcInput(
    DateTime Start,
    DateTime End,
    decimal LunchHours,
    decimal NormalRate,
    decimal OvertimeMultiplier,
    bool IsHoliday = false);

public sealed record TimeLineCalcResult(
    decimal ElapseHours,
    decimal NetHours,
    decimal NormalHours,
    decimal OtHours,
    TimeType TimeType,
    decimal NormalRate,
    decimal OtRate,
    decimal LabourCost);

/// <summary>
/// Pure labour-time calculation used by the Time Booking transaction and reports.
/// Mirrors the rules in the solution document section 8, refined to a per-line split:
///   Elapse = End - Start | Net = Elapse - Lunch
///   Normal = the part of the line inside the duty window on a working day
///   OT     = everything else (before duty start, after duty end, weekends/holidays)
///   Lunch is deducted from Normal first, the remainder from OT
///   Cost   = Normal × Rate + OT × (Rate × OT multiplier)
/// Kept free of I/O so it can be exercised by unit tests.
/// </summary>
public static class TimeBookingCalculator
{
    public static TimeLineCalcResult Calculate(TimeLineCalcInput input, DutyTiming duty)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(duty);

        if (input.End <= input.Start)
            throw new ArgumentException("End date-time must be after start date-time.", nameof(input));
        if (input.LunchHours < 0)
            throw new ArgumentException("Lunch hours cannot be negative.", nameof(input));

        var elapseHours = Round2((decimal)(input.End - input.Start).TotalHours);
        var netHours = Math.Max(0m, Round2(elapseHours - input.LunchHours));

        var rawNormal = input.IsHoliday ? 0m : Round2((decimal)NormalOverlap(input.Start, input.End, duty).TotalHours);
        var rawOt = Math.Max(0m, Round2(elapseHours - rawNormal));

        // Lunch comes out of the Normal portion first; anything left reduces OT.
        var normalHours = Math.Max(0m, Round2(rawNormal - input.LunchHours));
        var lunchLeft = Math.Max(0m, input.LunchHours - rawNormal);
        var otHours = Math.Max(0m, Round2(rawOt - lunchLeft));
        // Keep Normal + OT == Net despite independent rounding.
        var drift = netHours - (normalHours + otHours);
        if (drift != 0m) { if (normalHours + drift >= 0m) normalHours += drift; else otHours += drift; }

        var timeType = otHours <= 0m ? TimeType.Normal
                     : normalHours <= 0m ? TimeType.Overtime
                     : TimeType.Mixed;

        var multiplier = input.OvertimeMultiplier <= 0 ? 1m : input.OvertimeMultiplier;
        var normalRate = Round2(input.NormalRate);
        var otRate = Round2(input.NormalRate * multiplier);
        var labourCost = Round2(normalHours * normalRate + otHours * otRate);

        return new TimeLineCalcResult(elapseHours, netHours, normalHours, otHours, timeType, normalRate, otRate, labourCost);
    }

    /// <summary>Labour cost for a (possibly user-adjusted) split — the same formula the calculator uses.</summary>
    public static decimal Cost(decimal normalHours, decimal otHours, decimal normalRate, decimal otRate) =>
        Round2(normalHours * normalRate + otHours * otRate);

    /// <summary>Overtime if the work is on a weekend/holiday, or starts before / ends after duty timings.</summary>
    public static TimeType ClassifyTimeType(TimeLineCalcInput input, DutyTiming duty)
    {
        if (input.IsHoliday || duty.WeekendDays.Contains(input.Start.DayOfWeek))
            return TimeType.Overtime;

        var startsBeforeDuty = TimeOnly.FromDateTime(input.Start) < duty.DutyStart;
        var endsAfterDuty = TimeOnly.FromDateTime(input.End) > duty.DutyEnd;

        return startsBeforeDuty || endsAfterDuty ? TimeType.Overtime : TimeType.Normal;
    }

    /// <summary>
    /// Total time of [start,end) that falls inside the duty window on working (non-weekend) days.
    /// Walks day by day so a line that runs past midnight is handled.
    /// </summary>
    private static TimeSpan NormalOverlap(DateTime start, DateTime end, DutyTiming duty)
    {
        var total = TimeSpan.Zero;
        for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
        {
            if (duty.WeekendDays.Contains(day.DayOfWeek)) continue;
            var winStart = day.Add(duty.DutyStart.ToTimeSpan());
            var winEnd = day.Add(duty.DutyEnd.ToTimeSpan());
            if (winEnd <= winStart) continue;                 // degenerate duty window
            var s = start > winStart ? start : winStart;
            var e = end < winEnd ? end : winEnd;
            if (e > s) total += e - s;
        }
        return total;
    }

    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
