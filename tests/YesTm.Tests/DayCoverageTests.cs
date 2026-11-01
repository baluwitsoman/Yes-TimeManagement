using ClosedXML.Excel;
using YesTm.Web.Features.Reports;

namespace YesTm.Tests;

public class DayCoverageTests
{
    private static readonly IReadOnlySet<DayOfWeek> Fri = new HashSet<DayOfWeek> { DayOfWeek.Friday };

    // 12-Oct-2026 (Mon) .. 18-Oct-2026 (Sun): Friday 16th is the weekend → 6 working days.
    private static readonly DateTime From = new(2026, 10, 12);
    private static readonly DateTime To = new(2026, 10, 18);

    private static Dictionary<DateTime, BookedDay> Booked(params (int Day, decimal Hours, decimal NonJob)[] d) =>
        d.ToDictionary(x => new DateTime(2026, 10, x.Day),
            x => new BookedDay(new DateTime(2026, 10, x.Day), x.Hours, x.NonJob, x.NonJob > 0 ? "Annual Leave" : null));

    [Fact]
    public void Classifies_Missed_Short_Full_AndLeaveCountsAsBooked()
    {
        var c = DayCoverageCalculator.Calculate("T1", "Tech", From, To, Fri, 8m, Booked(
            (12, 8.5m, 0),      // Mon full job day
            (13, 4m, 0),        // Tue short
            (14, 8.5m, 8.5m),   // Wed annual leave → booked, not missed
            (16, 6m, 0)));      // Fri weekend worked

        Assert.Equal(6, c.WorkingDays);
        Assert.Equal(3, c.BookedDays);
        Assert.Equal(3, c.MissedDays);            // Thu 15, Sat 17, Sun 18
        Assert.Equal(1, c.ShortDays);
        Assert.Equal(1, c.WeekendDaysWorked);
        Assert.Equal(DayStatus.Weekend, c.Days.Single(d => d.Date.Day == 16).Status);
        Assert.Equal([15, 17, 18], c.Days.Where(d => d.Status == DayStatus.Missed).Select(d => d.Date.Day));
    }

    [Fact]
    public void TechnicianWise_ShowsDayColumns_DayCheckTab_AndTechniciansWithNoBookings()
    {
        var rows = new[]
        {
            new TaskTimeReportRow { TL_TECH_CODE = "T1", TL_TECH_NAME = "Alpha", TL_JOB_CODE = "100", TL_TASK_NAME = "Repair",
                TL_NET_HOURS = 8.5m, TL_NORMAL_HOURS = 8.5m, TL_WORK_DATE = new DateTime(2026, 10, 12), TL_SEGMENT_TYPE = "WORK" }
        };
        var coverage = new DayCoverageReport(From, To, 8m, Fri,
        [
            DayCoverageCalculator.Calculate("T1", "Alpha", From, To, Fri, 8m, Booked((12, 8.5m, 0))),
            DayCoverageCalculator.Calculate("T2", "Bravo", From, To, Fri, 8m, Booked()),   // booked nothing at all
        ]);

        using var wb = new XLWorkbook(new MemoryStream(TaskTimeExcel.BuildTechnicianWise(rows, "test", coverage: coverage)));
        var sum = wb.Worksheet("Summary");
        int Col(string h) => sum.Row(5).CellsUsed().First(c => c.GetString() == h).Address.ColumnNumber;
        int Row(string code) => sum.Column(1).CellsUsed().First(c => c.GetString() == code).Address.RowNumber;

        Assert.Equal(5, sum.Cell(Row("T1"), Col("Missed Days")).GetValue<int>());
        Assert.Equal(6, sum.Cell(Row("T2"), Col("Missed Days")).GetValue<int>());   // Bravo listed with every day missed
        Assert.Equal(0, sum.Cell(Row("T2"), Col("Task Lines")).GetValue<int>());

        var dc = wb.Worksheet("Day Check");
        Assert.Contains("15-Oct", dc.Row(6).CellsUsed().Last().GetString());           // Alpha's missed dates
        var hoursCell = dc.Row(6).Cells(3, 8).First(c => !c.IsEmpty());
        Assert.Equal("[h]:mm", hoursCell.Style.NumberFormat.Format);                   // hh:mm, not 9. / 9.08
        Assert.Matches(@"^\d+:\d\d$", hoursCell.GetFormattedString());
        Assert.DoesNotContain(wb.Worksheets, w => w.Name.Contains("Bravo"));          // no empty per-tech tab
    }

    [Fact]
    public void Summary_ExplainsEveryColumn_BelowTheGrandTotal()
    {
        var rows = new[]
        {
            new TaskTimeReportRow { TL_TECH_CODE = "T1", TL_TECH_NAME = "Alpha", TL_JOB_CODE = "100", TL_TASK_NAME = "Repair",
                TL_NET_HOURS = 8m, TL_NORMAL_HOURS = 8m, TL_WORK_DATE = new DateTime(2026, 10, 12), TL_SEGMENT_TYPE = "WORK" }
        };
        foreach (var bytes in new[] { TaskTimeExcel.BuildTechnicianWise(rows, "t"), TaskTimeExcel.BuildJobWise(rows, "t") })
        {
            var sum = new XLWorkbook(new MemoryStream(bytes)).Worksheet("Summary");
            var headers = sum.Row(5).CellsUsed().Select(c => c.GetString()).ToList();
            var title = sum.Column(1).CellsUsed().First(c => c.GetString() == "How each column is calculated").Address.RowNumber;
            var grand = sum.Column(2).CellsUsed().First(c => c.GetString() == "Grand Total").Address.RowNumber;
            Assert.True(title > grand);
            var explained = sum.Column(1).CellsUsed().Where(c => c.Address.RowNumber > title).Select(c => c.GetString()).ToList();
            Assert.All(headers, h => Assert.Contains(h, explained));                      // one note per column, every column
            var std = sum.Column(1).CellsUsed().First(c => c.Address.RowNumber > title && c.GetString() == "Std Hrs");
            Assert.Contains("counts once per job", sum.Cell(std.Address.RowNumber, 3).GetString());
        }
    }
}
