using YesTm.Web.Common.Availability;
using YesTm.Web.Common.TimeCalc;
using YesTm.Web.Features.Masters;
using YesTm.Web.Features.Reports;
using YesTm.Web.Features.TimeBooking;

namespace YesTm.Tests;

public class NonJobTests
{
    private static TM_NONJOB_ACTIVITY Act(string cat, string paid = "Y") =>
        new() { NJ_CODE = "NJ-X", NJ_NAME = "X", NJ_CATEGORY = cat, NJ_PAID_YN = paid };

    // Friday 08:00-17:00 with a Fri/Sat weekend → the engine makes it all OT.
    private static TimeLineCalcResult WeekendDay() => TimeBookingCalculator.Calculate(
        new TimeLineCalcInput(new DateTime(2026, 10, 9, 8, 0, 0), new DateTime(2026, 10, 9, 17, 0, 0), 1m, 10m, 1.5m),
        new DutyTiming(new TimeOnly(8, 0), new TimeOnly(17, 0), 1m, new HashSet<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday }));

    [Fact]
    public void Absence_IsAllNormal_EvenOnAWeekend()
    {
        var raw = WeekendDay();
        Assert.Equal(8m, raw.OtHours);                                   // the engine says OT…

        var leave = NonJobRules.Apply(raw, Act(NonJobCategories.Leave));
        Assert.Equal((8m, 0m), (leave.NormalHours, leave.OtHours));     // …absence is plain normal time
    }

    [Fact]
    public void NonJobTime_IsHoursOnly_IdleKeepsTheDutySplit()
    {
        foreach (var cat in NonJobCategories.All)
        {
            var r = NonJobRules.Apply(WeekendDay(), Act(cat));
            Assert.Equal((0m, 0m, 0m), (r.NormalRate, r.OtRate, r.LabourCost));   // never job cost
        }

        var idle = NonJobRules.Apply(WeekendDay(), Act(NonJobCategories.Idle));
        Assert.Equal(8m, idle.OtHours);                                  // idle on a weekend is real (OT) time at work
    }

    private static TaskTimeReportRow Row(decimal net, string? cat = null, string seg = "WORK", decimal idle = 0, int day = 5) => new()
    {
        TL_NET_HOURS = net, TL_NORMAL_HOURS = net, NJ_CATEGORY = cat, NJ_NAME = cat is null ? null : "Annual Leave",
        TL_SEGMENT_TYPE = seg, TL_IDLE_HOURS = idle, TL_TECH_CODE = "T1", TL_JOB_CODE = cat is null ? "100" : "NJ-AL",
        TL_TASK_NAME = "Repair", TL_WORK_DATE = new DateTime(2026, 10, day)
    };

    [Fact]
    public void Metrics_SortHoursIntoTheRightBuckets()
    {
        var m = ReportMetrics.Of(
        [
            Row(8, idle: 1),                       // job: 7 work + 1 idle
            Row(3, seg: "TRAVEL"),                 // travel
            Row(8, NonJobCategories.Leave),        // absence
            Row(4, NonJobCategories.Idle),         // idle activity
            Row(2, NonJobCategories.NonProd),      // training
        ]);
        Assert.Equal((7m, 3m, 5m, 8m, 2m), (m.Work, m.Travel, m.Idle, m.Absence, m.NonProd));
    }

    [Fact]
    public void BookedLeave_ReducesAvailability_HalfDayFromHours()
    {
        var days = UtilisationService.BookedAbsenceDays(
        [
            Row(8, NonJobCategories.Leave, day: 5),      // Mon full day
            Row(4, NonJobCategories.Leave, day: 6),      // Tue half day
            Row(8, NonJobCategories.Holiday, day: 7),    // Wed public holiday
            Row(8, day: 8),                              // Thu job work — not absence
        ], stdDay: 8m);

        Assert.Equal(3, days.Count);
        var a = AvailabilityCalculator.Calculate(new DateTime(2026, 10, 4), new DateTime(2026, 10, 10), 8m,
            new HashSet<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday }, days, AvailabilitySources.Booked);

        Assert.Equal(7m, a.CalendarDays);
        Assert.Equal(2m, a.OffDays);              // Fri + Sat
        Assert.Equal(1m, a.HolidayDays);
        Assert.Equal(4m, a.ScheduledDays);        // 7 − 2 − 1
        Assert.Equal(1.5m, a.LeaveDays);
        Assert.Equal(2.5m, a.AvailableDays);
        Assert.Equal(20m, a.AvailableHours);
        Assert.Equal(AvailabilitySources.Booked, a.Source);
    }
}
