namespace YesTm.Web.Common.TimeCalc;

public enum TimeType
{
    Normal,
    Overtime
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
    TimeType TimeType,
    decimal AppliedRate,
    decimal LabourCost);

/// <summary>
/// Pure labour-time calculation used by the Time Booking transaction and reports.
/// Mirrors the rules in the solution document section 8:
///   Elapse = End - Start | Net = Elapse - Lunch
///   Overtime if weekend/holiday OR outside duty timings, else Normal
///   Rate = Normal rate (× OT multiplier when Overtime) | Cost = Net × Rate
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

        var timeType = ClassifyTimeType(input, duty);

        var multiplier = input.OvertimeMultiplier <= 0 ? 1m : input.OvertimeMultiplier;
        var appliedRate = timeType == TimeType.Overtime
            ? Round2(input.NormalRate * multiplier)
            : Round2(input.NormalRate);

        var labourCost = Round2(netHours * appliedRate);

        return new TimeLineCalcResult(elapseHours, netHours, timeType, appliedRate, labourCost);
    }

    /// <summary>Overtime if the work is on a weekend/holiday, or starts before / ends after duty timings.</summary>
    public static TimeType ClassifyTimeType(TimeLineCalcInput input, DutyTiming duty)
    {
        if (input.IsHoliday || duty.WeekendDays.Contains(input.Start.DayOfWeek))
            return TimeType.Overtime;

        var startsBeforeDuty = TimeOnly.FromDateTime(input.Start) < duty.DutyStart;
        var endsAfterDuty = TimeOnly.FromDateTime(input.End) > duty.DutyEnd;

        return startsBeforeDuty || endsAfterDuty ? TimeType.Overtime : TimeType.Normal;
    }

    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
