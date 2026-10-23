using YesTm.Web.Common.TimeCalc;

namespace YesTm.Tests;

public class BookingOverlapTests
{
    private static BookedRange R(string tech, int h1, int m1, int h2, int m2, string where = "x", int day = 19) =>
        new(tech, "Tech " + tech, new DateTime(2026, 10, day, h1, m1, 0), new DateTime(2026, 10, day, h2, m2, 0), where);

    [Fact]
    public void SameDay_DifferentTimes_IsAllowed()
    {
        // 08:00-12:00 leave, 13:00-17:30 job, and a touching 12:00-13:00 line.
        var posted = new[] { R("T1", 8, 0, 12, 0, "leave"), R("T1", 12, 0, 13, 0, "idle") };
        var existing = new[] { R("T1", 13, 0, 17, 30, "TS-1") };
        Assert.Empty(BookingOverlap.Find(posted, existing));
    }

    [Fact]
    public void OverlapWithAnotherSheet_IsReported()
    {
        var problems = BookingOverlap.Find([R("T1", 8, 0, 17, 0, "line 1, Annual Leave")], [R("T1", 10, 0, 12, 0, "TS-2026-000090")]);
        Assert.Single(problems);
        Assert.Contains("TS-2026-000090", problems[0]);
    }

    [Fact]
    public void OverlapInsideTheSameSheet_IsReported_OtherTechniciansAreIndependent()
    {
        var posted = new[] { R("T1", 8, 0, 12, 0, "line 1"), R("T1", 11, 0, 14, 0, "line 2"), R("T2", 8, 0, 12, 0, "line 3") };
        var problems = BookingOverlap.Find(posted, []);
        Assert.Single(problems);
        Assert.Contains("line 1", problems[0]);
        Assert.Contains("line 2", problems[0]);
    }

    [Fact]
    public void OvernightLine_OverlapsTheNextMorning()
    {
        var night = new BookedRange("T1", null, new DateTime(2026, 10, 19, 22, 0, 0), new DateTime(2026, 10, 20, 4, 0, 0), "travel");
        Assert.Single(BookingOverlap.Find([night], [R("T1", 3, 0, 8, 0, "TS-9", day: 20)]));
    }
}
