using YesTm.Web.Common.TimeCalc;

namespace YesTm.Tests;

public class TimeBookingCalculatorTests
{
    private static DutyTiming StandardDuty(params DayOfWeek[] weekend) =>
        new(new TimeOnly(8, 0), new TimeOnly(17, 0), 0.5m, weekend.ToHashSet());

    [Fact]
    public void NetHours_SubtractsLunch_FromElapse()
    {
        // 08:00 -> 16:30 = 8.5h elapse, minus 0.5 lunch = 8.0 net
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 8, 0, 0), new DateTime(2026, 6, 8, 16, 30, 0),
            LunchHours: 0.5m, NormalRate: 15m, OvertimeMultiplier: 1.5m);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(/* no weekend */));

        Assert.Equal(8.5m, result.ElapseHours);
        Assert.Equal(8.0m, result.NetHours);
    }

    [Fact]
    public void WithinDutyHours_OnWeekday_IsNormal_AndCostsNetTimesRate()
    {
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 8, 0, 0), new DateTime(2026, 6, 8, 16, 30, 0),
            0.5m, 15m, 1.5m);

        // Make the weekend set NOT contain this day so weekday-classification is tested.
        var duty = StandardDuty(DayOfWeek.Sunday);
        var result = TimeBookingCalculator.Calculate(input, duty);

        Assert.Equal(TimeType.Normal, result.TimeType);
        Assert.Equal(8.0m, result.NormalHours);
        Assert.Equal(0m, result.OtHours);
        Assert.Equal(15m, result.NormalRate);
        Assert.Equal(22.5m, result.OtRate);
        Assert.Equal(120.00m, result.LabourCost);   // 8.0 * 15
    }

    [Fact]
    public void EndAfterDuty_IsOvertime_AndAppliesMultiplier()
    {
        // 18:00 -> 21:00, ends after 17:00 duty end -> Overtime
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 18, 0, 0), new DateTime(2026, 6, 8, 21, 0, 0),
            0m, 15m, 1.5m);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(DayOfWeek.Sunday));

        Assert.Equal(TimeType.Overtime, result.TimeType);
        Assert.Equal(3.0m, result.NetHours);
        Assert.Equal(0m, result.NormalHours);
        Assert.Equal(3.0m, result.OtHours);
        Assert.Equal(22.50m, result.OtRate);         // 15 * 1.5
        Assert.Equal(67.50m, result.LabourCost);     // 3.0 * 22.5
    }

    [Fact]
    public void StartBeforeDuty_SplitsIntoOtAndNormal()
    {
        // 06:00 -> 09:00: 2h before duty start (OT) + 1h inside (Normal) = Mixed line
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 6, 0, 0), new DateTime(2026, 6, 8, 9, 0, 0),
            0m, 15m, 1.5m);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(DayOfWeek.Sunday));

        Assert.Equal(TimeType.Mixed, result.TimeType);
        Assert.Equal(1.0m, result.NormalHours);
        Assert.Equal(2.0m, result.OtHours);
        Assert.Equal(60.00m, result.LabourCost);     // 1*15 + 2*22.5
    }

    [Fact]
    public void RunsPastDutyEnd_LunchComesOutOfNormalFirst()
    {
        // 08:00 -> 20:00 with 1h lunch: 9h inside duty, 3h after -> Normal 8, OT 3, Net 11
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 8, 0, 0), new DateTime(2026, 6, 8, 20, 0, 0),
            1m, 10m, 1.5m);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(DayOfWeek.Sunday));

        Assert.Equal(11.0m, result.NetHours);
        Assert.Equal(8.0m, result.NormalHours);
        Assert.Equal(3.0m, result.OtHours);
        Assert.Equal(result.NetHours, result.NormalHours + result.OtHours);
        Assert.Equal(125.00m, result.LabourCost);    // 8*10 + 3*15
    }

    [Fact]
    public void OvernightShift_CountsNextDayDutyWindowAsNormal()
    {
        // Mon 22:00 -> Tue 10:00: 10h OT (22:00-08:00) + 2h Normal (08:00-10:00 Tue)
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 22, 0, 0), new DateTime(2026, 6, 9, 10, 0, 0),
            0m, 10m, 2m);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(DayOfWeek.Sunday));

        Assert.Equal(12.0m, result.NetHours);
        Assert.Equal(2.0m, result.NormalHours);
        Assert.Equal(10.0m, result.OtHours);
        Assert.Equal(TimeType.Mixed, result.TimeType);
        Assert.Equal(220.00m, result.LabourCost);    // 2*10 + 10*20
    }

    [Fact]
    public void Cost_UsesAdjustedSplitAndRates()
    {
        Assert.Equal(125.00m, TimeBookingCalculator.Cost(8m, 3m, 10m, 15m));
        Assert.Equal(0m, TimeBookingCalculator.Cost(0m, 0m, 10m, 15m));
    }

    [Fact]
    public void WorkOnWeekend_IsOvertime_EvenWithinDutyHours()
    {
        var start = new DateTime(2026, 6, 6, 9, 0, 0);   // any date within 08:00-17:00
        var end = new DateTime(2026, 6, 6, 12, 0, 0);
        var input = new TimeLineCalcInput(start, end, 0m, 15m, 1.5m);

        // Treat this date's weekday as a weekend day.
        var result = TimeBookingCalculator.Calculate(input, StandardDuty(start.DayOfWeek));

        Assert.Equal(TimeType.Overtime, result.TimeType);
        Assert.Equal(0m, result.NormalHours);
        Assert.Equal(3.0m, result.OtHours);
    }

    [Fact]
    public void Holiday_IsOvertime()
    {
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 9, 0, 0), new DateTime(2026, 6, 8, 12, 0, 0),
            0m, 15m, 1.5m, IsHoliday: true);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(DayOfWeek.Sunday));

        Assert.Equal(TimeType.Overtime, result.TimeType);
    }

    [Fact]
    public void NetHours_NeverNegative_WhenLunchExceedsElapse()
    {
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 8, 0, 0), new DateTime(2026, 6, 8, 8, 30, 0),
            LunchHours: 2m, NormalRate: 15m, OvertimeMultiplier: 1.5m);

        var result = TimeBookingCalculator.Calculate(input, StandardDuty(DayOfWeek.Sunday));

        Assert.Equal(0m, result.NetHours);
        Assert.Equal(0m, result.LabourCost);
    }

    [Fact]
    public void EndNotAfterStart_Throws()
    {
        var input = new TimeLineCalcInput(
            new DateTime(2026, 6, 8, 10, 0, 0), new DateTime(2026, 6, 8, 10, 0, 0),
            0m, 15m, 1.5m);

        Assert.Throws<ArgumentException>(() => TimeBookingCalculator.Calculate(input, StandardDuty()));
    }
}
