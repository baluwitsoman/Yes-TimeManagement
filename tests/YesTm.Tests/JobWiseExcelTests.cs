using ClosedXML.Excel;
using YesTm.Web.Features.Reports;

namespace YesTm.Tests;

public class JobWiseExcelTests
{
    private static TaskTimeReportRow Line(string job, string tech, string task, decimal std, decimal net,
        string segment = "WORK", decimal idle = 0m, int day = 1) => new()
    {
        TS_SHEET_NO = "TS-" + job, TL_JOB_CODE = job, TS_CUSTOMER_NAME = "Cust " + job, TS_LOCATION = "Field",
        TS_JOB_STATUS = "In Progress", TL_TECH_CODE = tech, TL_TECH_NAME = "Tech " + tech, TL_TASK_NAME = task,
        TL_STD_HOURS = std, TL_NET_HOURS = net, TL_NORMAL_HOURS = net, TL_LABOUR_COST = net * 10m,
        TL_TOTAL_COST = net * 10m, TL_SEGMENT_TYPE = segment, TL_IDLE_HOURS = idle,
        TL_WORK_DATE = new DateTime(2026, 9, day), TL_START_DT = new DateTime(2026, 9, day, 8, 0, 0)
    };

    private static IXLWorksheet Summary(byte[] bytes) => new XLWorkbook(new MemoryStream(bytes)).Worksheet("Summary");

    private static int Col(IXLWorksheet ws, string header) =>
        ws.Row(5).CellsUsed().First(c => c.GetString() == header).Address.ColumnNumber;

    [Fact]
    public void Summary_HasOneRowPerJob_PlusOneTabPerJob()
    {
        var rows = new[] { Line("100", "A", "Inspection", 8, 8), Line("200", "B", "Repair", 4, 5) };
        var bytes = TaskTimeExcel.BuildJobWise(rows, "test");
        using var wb = new XLWorkbook(new MemoryStream(bytes));

        Assert.Equal(4, wb.Worksheets.Count);                    // Summary + 2 jobs + Definitions
        Assert.Equal("Definitions", wb.Worksheets.Last().Name);
        var ws = wb.Worksheet("Summary");
        Assert.Equal("100", ws.Cell(6, 1).GetString());
        Assert.Equal("200", ws.Cell(7, 1).GetString());
        Assert.Equal("Grand Total", ws.Cell(8, 2).GetString());
    }

    [Fact]
    public void StdHours_CountOncePerTask_NotPerLine()
    {
        // Same 8h task booked by two technicians over two days → std 8, work 4×4 = 16 → 50 %.
        var rows = new[]
        {
            Line("100", "A", "Inspection", 8, 4, day: 1), Line("100", "B", "Inspection", 8, 4, day: 1),
            Line("100", "A", "Inspection", 8, 4, day: 2), Line("100", "B", "Inspection", 8, 4, day: 2),
        };
        var ws = Summary(TaskTimeExcel.BuildJobWise(rows, "test"));

        Assert.Equal(8m, ws.Cell(6, Col(ws, "Std Hrs")).GetValue<decimal>());
        Assert.Equal(16m, ws.Cell(6, Col(ws, "Work Hrs")).GetValue<decimal>());
        Assert.Equal(50m, ws.Cell(6, Col(ws, "Efficiency %")).GetValue<decimal>());
        Assert.Equal(2, ws.Cell(6, Col(ws, "Technicians")).GetValue<int>());
    }

    [Fact]
    public void TechnicianWise_SplitsTaskStdByWorkShare_AndReconcilesWithJobWise()
    {
        // 9h task: A works 6h, B works 3h → A earns 6, B earns 3 (both 100 %); total std 9 = job-wise std.
        var rows = new[] { Line("100", "A", "Repair", 9, 6), Line("100", "B", "Repair", 9, 3) };
        var ws = Summary(TaskTimeExcel.BuildTechnicianWise(rows, "test"));
        int Row(string code) => ws.Column(1).CellsUsed().First(c => c.GetString() == code).Address.RowNumber;

        Assert.Equal(6m, ws.Cell(Row("A"), Col(ws, "Std Hrs")).GetValue<decimal>());
        Assert.Equal(3m, ws.Cell(Row("B"), Col(ws, "Std Hrs")).GetValue<decimal>());
        Assert.Equal(100m, ws.Cell(Row("A"), Col(ws, "Efficiency %")).GetValue<decimal>());
        var grand = ws.Column(2).CellsUsed().First(c => c.GetString() == "Grand Total").Address.RowNumber;
        Assert.Equal(9m, ws.Cell(grand, Col(ws, "Std Hrs")).GetValue<decimal>());
    }

    [Fact]
    public void TravelAndIdle_AreExcludedFromWorkHours()
    {
        var rows = new[]
        {
            Line("100", "A", "Travel", 0, 3, segment: "TRAVEL"),
            Line("100", "A", "Repair", 6, 8, idle: 2),
        };
        var ws = Summary(TaskTimeExcel.BuildJobWise(rows, "test"));

        Assert.Equal(6m, ws.Cell(6, Col(ws, "Work Hrs")).GetValue<decimal>());     // 8 net − 2 idle
        Assert.Equal(3m, ws.Cell(6, Col(ws, "Travel Hrs")).GetValue<decimal>());
        Assert.Equal(2m, ws.Cell(6, Col(ws, "Idle Hrs")).GetValue<decimal>());
        Assert.Equal(11m, ws.Cell(6, Col(ws, "Net Hrs")).GetValue<decimal>());     // paid hours keep everything
        Assert.Equal(100m, ws.Cell(6, Col(ws, "Efficiency %")).GetValue<decimal>());
    }
}
