using YesTm.Web.Common.Availability;
using YesTm.Web.Features.Hrms;

namespace YesTm.Tests;

public class AvailabilityCalculatorTests
{
    private static readonly IReadOnlySet<DayOfWeek> FriSat = new HashSet<DayOfWeek> { DayOfWeek.Friday, DayOfWeek.Saturday };

    // September 2026: 30 days, Fri/Sat = 8 days (4,5,11,12,18,19,25,26) → 22 weekdays.
    private static readonly DateTime From = new(2026, 9, 1);
    private static readonly DateTime To = new(2026, 9, 30);

    private static AttendanceDay Day(int d, string status, decimal leave = 0, decimal hol = 0, decimal absent = 0,
        decimal actual = 0, string? leaveName = null, string? holName = null) =>
        new(new DateTime(2026, 9, d), status, leave, hol, absent, actual, leaveName, holName);

    [Fact]
    public void WithoutHrmsData_UsesDutyTimingWeekendOnly()
    {
        var a = AvailabilityCalculator.Calculate(From, To, 8m, FriSat, null);

        Assert.Equal(30m, a.CalendarDays);
        Assert.Equal(8m, a.OffDays);
        Assert.Equal(22m, a.ScheduledDays);
        Assert.Equal(22m, a.AvailableDays);
        Assert.Equal(176m, a.AvailableHours);
        Assert.Equal(AvailabilitySources.Calendar, a.Source);
    }

    [Fact]
    public void HolidayLeaveAndAbsence_ReduceAvailability_HalfDaysCountHalf()
    {
        var att = new[]
        {
            Day(1, "HOLIDAY", hol: 1, holName: "National Day"),
            Day(2, "LEAVE", leave: 1, leaveName: "Annual Leave"),
            Day(3, "LEAVE", leave: 0.5m, leaveName: "Sick Leave"),
            Day(7, "ABSENT", absent: 1),
            Day(8, "PRESENT", actual: 9.5m),
        }.ToDictionary(x => x.Date);

        var a = AvailabilityCalculator.Calculate(From, To, 8m, FriSat, att);

        Assert.Equal(1m, a.HolidayDays);
        Assert.Equal(21m, a.ScheduledDays);              // 30 − 8 − 1
        Assert.Equal(1.5m, a.LeaveDays);
        Assert.Equal(1m, a.AbsentDays);
        Assert.Equal(18.5m, a.AvailableDays);            // 21 − 1.5 − 1
        Assert.Equal(148m, a.AvailableHours);            // 18.5 × 8
        Assert.Equal(9.5m, a.AttendanceHours);
        Assert.Equal(AvailabilitySources.Partial, a.Source);
        Assert.Contains("Annual Leave 1", a.LeaveText);
        Assert.Contains("Sick Leave 0.5", a.LeaveText);
        Assert.Contains("01-Sep National Day", a.Holidays);
        Assert.Contains("= 18.5 available × 8.00 h = 148.00 h", a.Formula);
    }

    [Fact]
    public void HrmsOff_OverridesTheDutyTimingWeekend()
    {
        // Roster says Thursday 3rd is OFF; Friday 4th was a rostered working day (PRESENT).
        var att = new[] { Day(3, "OFF"), Day(4, "PRESENT", actual: 8) }.ToDictionary(x => x.Date);

        var a = AvailabilityCalculator.Calculate(From, To, 8m, FriSat, att);

        Assert.Equal(8m, a.OffDays);                     // −1 Friday, +1 Thursday
        Assert.Equal(22m, a.ScheduledDays);
    }

    [Fact]
    public void Kpis_AreRatiosOfTheirBases()
    {
        var a = AvailabilityCalculator.Calculate(From, To, 8m, FriSat, null);   // 176 h available
        var k = new UtilisationKpi(a, BookedNet: 150m, BookedNormal: 140m, BookedOt: 4m,
            WorkHours: 132m, TravelHours: 12m, IdleHours: 6m, StdEarned: 145.2m);

        Assert.Equal(36m, k.UnbookedHours);              // 176 − 140
        Assert.Equal(180m, k.PaidHours);                 // 176 + 4 OT
        Assert.Equal(73.3m, k.UtilisationPct);           // 132 / 180
        Assert.Equal(110m, k.EfficiencyPct);             // 145.2 / 132
        Assert.Equal(80.7m, k.ProductivityPct);          // 145.2 / 180
        Assert.Null(k.ClockedNotBooked);                 // no HRMS data
    }
}

public class EmployeeMatcherTests
{
    private static HrmsEmployeeDto H(string code, string name, string? empNo = null) =>
        new() { EMP_CODE = code, EMP_NAME = name, EMPLOYMENT_NO = empNo };

    [Fact]
    public void Code_IgnoresCaseSeparatorsAndZeroPadding()
    {
        Assert.Equal(EmployeeMatcher.NormCode("YES0027"), EmployeeMatcher.NormCode("yes-027"));
        var m = EmployeeMatcher.Match([("YES027", "Hariharan L")], [H("YES0027", "Someone Else")]).Single();
        Assert.Equal(("YES0027", MatchTypes.Code, true), (m.HrmsCode, m.MatchType, m.AutoConfirm));
    }

    [Fact]
    public void EmploymentNumber_MatchesTmCode()
    {
        var m = EmployeeMatcher.Match([("YES0016", "Vignesh P")], [H("3901", "Vignesh Parthasarathy", "YES0016")]).Single();
        Assert.Equal(("3901", MatchTypes.EmploymentNo), (m.HrmsCode, m.MatchType));
    }

    [Fact]
    public void SameName_InAnyWordOrder_IsSuggestedNotConfirmed()
    {
        var m = EmployeeMatcher.Match([("T1", "Hariharan L")], [H("3902", "L. HARIHARAN"), H("3903", "Arun Anand")]).Single();
        Assert.Equal(("3902", MatchTypes.Name, 95m), (m.HrmsCode, m.MatchType, m.Confidence));
        Assert.False(m.AutoConfirm);
    }

    [Fact]
    public void SimilarName_IsSuggestedWithItsScore_DissimilarIsNot()
    {
        var hrms = new[] { H("3904", "Jakheer Hussain Babajan"), H("3905", "Manoj Lakshmanasamy") };
        var fuzzy = EmployeeMatcher.Match([("T1", "JAKHEER HUSSAIN BABAJAN K")], hrms).Single();
        Assert.Equal(MatchTypes.FuzzyName, fuzzy.MatchType);
        Assert.Equal("3904", fuzzy.HrmsCode);
        Assert.InRange(fuzzy.Confidence, EmployeeMatcher.FuzzyThreshold, 99m);

        var none = EmployeeMatcher.Match([("T2", "Completely Different")], hrms).Single();
        Assert.Equal(MatchTypes.None, none.MatchType);
        Assert.Null(none.HrmsCode);
    }

    [Fact]
    public void OneHrmsEmployee_GoesToTheStrongestClaimOnly()
    {
        var res = EmployeeMatcher.Match([("YES0030", "Ravi Kumar"), ("T9", "Ravi Kumar")], [H("YES030", "Ravi Kumar")]);
        Assert.Equal("YES030", res.Single(r => r.TmCode == "YES0030").HrmsCode);   // code match 100 beats name 95
        Assert.Null(res.Single(r => r.TmCode == "T9").HrmsCode);
    }
}

public class HrmsParsingTests
{
    [Theory]
    [InlineData("8.00", 8.0)]
    [InlineData("8:30", 8.5)]
    [InlineData("9.3", 9.5)]
    [InlineData("", 0.0)]
    [InlineData(".", 0.0)]
    public void ShiftHours_AreHoursDotMinutes(string raw, double expected) =>
        Assert.Equal((decimal)expected, HrmsSyncService.ParseHoursMinutes(raw));

    [Fact]
    public void Attendance_DayFractions_FollowTheStatus()
    {
        var leave = HrmsSyncService.ToEntity(new HrmsAttendanceDto
            { EMP_CODE = " 39001 ", ATT_DATE = new DateTime(2026, 9, 2, 0, 0, 0), DAY_STATUS = "leave", LEAVE_COUNT = 0.5m, ACTUAL_TOT_MINUTES = 240 }, 7);
        Assert.Equal(("39001", "LEAVE", 0.5m, 0m, 4m), (leave.ATT_HRMS_EMP_CODE, leave.ATT_DAY_STATUS, leave.ATT_LEAVE_DAYS, leave.ATT_HOLIDAY_DAYS, leave.ATT_ACTUAL_HOURS));

        var hol = HrmsSyncService.ToEntity(new HrmsAttendanceDto { EMP_CODE = "1", ATT_DATE = DateTime.Today, DAY_STATUS = "HOLIDAY" }, 7);
        Assert.Equal(1m, hol.ATT_HOLIDAY_DAYS);          // missing count = whole day

        var absent = HrmsSyncService.ToEntity(new HrmsAttendanceDto { EMP_CODE = "1", ATT_DATE = DateTime.Today, DAY_STATUS = "ABSENT", ABSENT_FACTOR = 0 }, 7);
        Assert.Equal(1m, absent.ATT_ABSENT_DAYS);
    }
}
