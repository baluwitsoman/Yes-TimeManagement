using ClosedXML.Excel;

namespace YesTm.Web.Features.Reports;

/// <summary>
/// Builds the Excel workbooks for the Task &amp; Time report: the flat export and the
/// technician-wise export (one worksheet per technician, lines grouped by Job → Sheet).
/// Column layout is shared so both files carry the same set of (Phase 2) columns.
/// </summary>
public static class TaskTimeExcel
{
    private const string DateFmt = "dd-MMM-yyyy";
    private const string TimeFmt = "HH:mm";
    private const string NumFmt = "#,##0.00";

    // One report column: header text, cell writer, whether it is numeric (right-aligned, 2 dp).
    private sealed record Col(string Header, Action<IXLCell, TaskTimeReportRow> Write, bool Numeric = false, string? Format = null);

    private static readonly Col[] Columns =
    [
        new("Sheet No",      (c, x) => c.Value = x.TS_SHEET_NO),
        new("Posting Date",  (c, x) => c.Value = x.TS_POSTING_DATE, Format: DateFmt),
        new("Job No",        (c, x) => c.Value = x.TL_JOB_CODE),
        new("Customer",      (c, x) => c.Value = x.TS_CUSTOMER_NAME),
        new("Task",          (c, x) => c.Value = x.TL_TASK_NAME),
        new("Tech Code",     (c, x) => c.Value = x.TL_TECH_CODE),
        new("Technician",    (c, x) => c.Value = x.TL_TECH_NAME),
        new("Skill",         (c, x) => c.Value = x.TL_SKILL),
        new("Location",      (c, x) => c.Value = x.TL_LOCATION),
        new("Work Date",     (c, x) => { if (x.TL_WORK_DATE is { } d) c.Value = d; }, Format: DateFmt),
        new("Start",         (c, x) => { if (x.TL_START_DT is { } d) c.Value = d; }, Format: TimeFmt),
        new("End",           (c, x) => { if (x.TL_END_DT is { } d) c.Value = d; }, Format: TimeFmt),
        new("Lunch Hrs",     (c, x) => c.Value = x.TL_LUNCH_HOURS, true),
        new("Std Hrs",       (c, x) => c.Value = x.TL_STD_HOURS, true),
        new("Net Hrs",       (c, x) => c.Value = x.TL_NET_HOURS, true),
        new("Normal Hrs",    (c, x) => c.Value = x.TL_NORMAL_HOURS, true),
        new("OT Hrs",        (c, x) => c.Value = x.TL_OT_HOURS, true),
        new("Time Type",     (c, x) => c.Value = x.TL_TIME_TYPE == "MIXED" ? "NORMAL + OT" : x.TL_TIME_TYPE),
        new("Rate",          (c, x) => c.Value = x.TL_RATE, true),
        new("OT Rate",       (c, x) => c.Value = x.TL_OT_RATE, true),
        new("Labour Cost",   (c, x) => c.Value = x.TL_LABOUR_COST, true),
        new("Food Allowance",(c, x) => c.Value = x.TL_FOOD_ALLOWANCE, true),
        new("Total Cost",    (c, x) => c.Value = x.TL_TOTAL_COST, true),
        new("Travel Site",   (c, x) => c.Value = x.TL_TRAVEL_SITE),
        new("Travel Start",  (c, x) => { if (x.TL_TRAVEL_START is { } d) c.Value = d; }, Format: TimeFmt),
        new("Travel End",    (c, x) => { if (x.TL_TRAVEL_END is { } d) c.Value = d; }, Format: TimeFmt),
        new("Travel Hrs",    (c, x) => c.Value = x.TL_TRAVEL_HOURS, true),
        new("Adjusted",      (c, x) => c.Value = x.TL_OVERRIDE_YN == "Y" ? "Yes" : ""),
        new("Work Type",     (c, x) => c.Value = x.WORK_TYPE_NAME),
        new("Job Status",    (c, x) => c.Value = x.TS_JOB_STATUS),
    ];

    private static int ColIndex(string header) => Array.FindIndex(Columns, c => c.Header == header) + 1;

    // Totals are written under these numeric columns.
    private static readonly (string Header, Func<ReportTotals, decimal> Pick)[] TotalCols =
    [
        ("Net Hrs", t => t.Net), ("Normal Hrs", t => t.Normal), ("OT Hrs", t => t.Ot),
        ("Labour Cost", t => t.Cost), ("Food Allowance", t => t.Food), ("Total Cost", t => t.Total), ("Travel Hrs", t => t.Travel),
    ];

    /// <summary>Flat export: one row per task line plus a grand-total row.</summary>
    public static byte[] BuildFlat(IReadOnlyList<TaskTimeReportRow> rows, ReportTotals totals)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Task & Time");
        var r = WriteHeader(ws, 1);
        foreach (var x in rows) WriteRow(ws, r++, x);
        WriteTotals(ws, r, "Grand Total", totals, bold: true);
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents();
        return Save(wb);
    }

    /// <summary>
    /// Technician-wise export: a Summary tab (one row per technician) followed by one tab per
    /// technician whose lines are grouped by Job → Sheet with subtotals per job and a technician total.
    /// </summary>
    public static byte[] BuildTechnicianWise(IReadOnlyList<TaskTimeReportRow> rows, string filterSummary)
    {
        using var wb = new XLWorkbook();

        var byTech = rows
            .GroupBy(x => x.TL_TECH_CODE ?? "")
            .Select(g => new
            {
                Code = g.Key,
                Name = g.Select(x => x.TL_TECH_NAME).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "",
                Rows = g.ToList()
            })
            .OrderBy(t => t.Name).ThenBy(t => t.Code)
            .ToList();

        // ---- Summary tab ----
        var sum = wb.AddWorksheet("Summary");
        sum.Cell(1, 1).Value = "Technician-wise Task & Time Report";
        sum.Cell(1, 1).Style.Font.Bold = true; sum.Cell(1, 1).Style.Font.FontSize = 14;
        sum.Cell(2, 1).Value = filterSummary;
        sum.Cell(3, 1).Value = $"Generated {DateTime.Now:dd-MMM-yyyy HH:mm} · {byTech.Count} technician(s) · {rows.Count} task line(s)";
        sum.Range(2, 1, 3, 1).Style.Font.FontColor = XLColor.DimGray;

        string[] sumHeaders = ["Tech Code", "Technician", "Jobs", "Task Lines", "Net Hrs", "Normal Hrs", "OT Hrs", "Labour Cost", "Food Allowance", "Total Cost", "Travel Hrs"];
        for (var c = 0; c < sumHeaders.Length; c++) sum.Cell(5, c + 1).Value = sumHeaders[c];
        StyleHeader(sum.Range(5, 1, 5, sumHeaders.Length));

        var sr = 6;
        var grand = ReportTotals.Zero;
        foreach (var t in byTech)
        {
            var tt = Sum(t.Rows);
            grand = Add(grand, tt);
            sum.Cell(sr, 1).Value = t.Code;
            sum.Cell(sr, 2).Value = t.Name;
            sum.Cell(sr, 3).Value = t.Rows.Select(x => x.TL_JOB_CODE).Distinct().Count();
            sum.Cell(sr, 4).Value = t.Rows.Count;
            sum.Cell(sr, 5).Value = tt.Net; sum.Cell(sr, 6).Value = tt.Normal; sum.Cell(sr, 7).Value = tt.Ot;
            sum.Cell(sr, 8).Value = tt.Cost; sum.Cell(sr, 9).Value = tt.Food; sum.Cell(sr, 10).Value = tt.Total;
            sum.Cell(sr, 11).Value = tt.Travel;
            sr++;
        }
        sum.Cell(sr, 2).Value = "Grand Total";
        sum.Cell(sr, 4).Value = rows.Count;
        sum.Cell(sr, 5).Value = grand.Net; sum.Cell(sr, 6).Value = grand.Normal; sum.Cell(sr, 7).Value = grand.Ot;
        sum.Cell(sr, 8).Value = grand.Cost; sum.Cell(sr, 9).Value = grand.Food; sum.Cell(sr, 10).Value = grand.Total;
        sum.Cell(sr, 11).Value = grand.Travel;
        sum.Range(sr, 1, sr, sumHeaders.Length).Style.Font.Bold = true;
        sum.Range(6, 5, sr, 11).Style.NumberFormat.Format = NumFmt;
        sum.Columns().AdjustToContents();

        // ---- One tab per technician ----
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Summary" };
        foreach (var t in byTech)
        {
            var ws = wb.AddWorksheet(SheetName(t.Code, t.Name, usedNames));
            ws.Cell(1, 1).Value = $"{t.Code} — {t.Name}";
            ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 13;
            ws.Cell(2, 1).Value = filterSummary;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.DimGray;

            var r = WriteHeader(ws, 4);
            var jobs = t.Rows
                .GroupBy(x => x.TL_JOB_CODE ?? "")
                .OrderBy(g => g.Key);
            foreach (var job in jobs)
            {
                var first = job.First();
                // Job band.
                ws.Cell(r, 1).Value = $"Job {job.Key} — {first.TS_CUSTOMER_NAME}";
                ws.Range(r, 1, r, Columns.Length).Merge().Style
                    .Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EEF9"));
                r++;

                foreach (var sheet in job.GroupBy(x => x.TS_SHEET_NO ?? "").OrderBy(g => g.Min(x => x.TS_POSTING_DATE)))
                {
                    foreach (var x in sheet.OrderBy(x => x.TL_WORK_DATE ?? x.TL_START_DT).ThenBy(x => x.TL_START_DT))
                        WriteRow(ws, r++, x);
                }
                WriteTotals(ws, r++, $"Job {job.Key} total", Sum(job.ToList()), bold: true, fill: "#F5F7FB");
            }
            WriteTotals(ws, r, $"{t.Name} total", Sum(t.Rows), bold: true, fill: "#FFF3CD");
            ws.SheetView.FreezeRows(4);
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = Math.Min(ws.Column(1).Width, 22);
        }

        return Save(wb);
    }

    // ---- shared writers ----
    private static int WriteHeader(IXLWorksheet ws, int row)
    {
        for (var c = 0; c < Columns.Length; c++) ws.Cell(row, c + 1).Value = Columns[c].Header;
        StyleHeader(ws.Range(row, 1, row, Columns.Length));
        return row + 1;
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE3EF");
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
    }

    private static void WriteRow(IXLWorksheet ws, int row, TaskTimeReportRow x)
    {
        for (var c = 0; c < Columns.Length; c++)
        {
            var cell = ws.Cell(row, c + 1);
            Columns[c].Write(cell, x);
            if (Columns[c].Numeric) cell.Style.NumberFormat.Format = NumFmt;
            else if (Columns[c].Format is { } f) cell.Style.DateFormat.Format = f;
        }
        if (x.TL_OVERRIDE_YN == "Y")
            ws.Cell(row, ColIndex("Adjusted")).Style.Font.FontColor = XLColor.FromHtml("#8A6100");
    }

    private static void WriteTotals(IXLWorksheet ws, int row, string label, ReportTotals t, bool bold, string? fill = null)
    {
        ws.Cell(row, ColIndex("Net Hrs") - 1).Value = label;
        ws.Cell(row, ColIndex("Net Hrs") - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        foreach (var (header, pick) in TotalCols)
        {
            var cell = ws.Cell(row, ColIndex(header));
            cell.Value = pick(t);
            cell.Style.NumberFormat.Format = NumFmt;
        }
        var range = ws.Range(row, 1, row, Columns.Length);
        if (bold) range.Style.Font.Bold = true;
        if (fill is not null) range.Style.Fill.BackgroundColor = XLColor.FromHtml(fill);
    }

    private static ReportTotals Sum(IReadOnlyList<TaskTimeReportRow> rows) => new(
        rows.Sum(x => x.TL_NET_HOURS), rows.Sum(x => x.TL_LABOUR_COST), rows.Sum(x => x.TL_NORMAL_HOURS),
        rows.Sum(x => x.TL_OT_HOURS), rows.Sum(x => x.TL_FOOD_ALLOWANCE), rows.Sum(x => x.TL_TOTAL_COST),
        rows.Sum(x => x.TL_TRAVEL_HOURS));

    private static ReportTotals Add(ReportTotals a, ReportTotals b) => new(
        a.Net + b.Net, a.Cost + b.Cost, a.Normal + b.Normal, a.Ot + b.Ot, a.Food + b.Food, a.Total + b.Total, a.Travel + b.Travel);

    // Excel sheet names: max 31 chars, none of  : \ / ? * [ ]  and unique within the workbook.
    private static string SheetName(string code, string name, HashSet<string> used)
    {
        var raw = string.IsNullOrWhiteSpace(name) ? code : $"{code} {name}";
        if (string.IsNullOrWhiteSpace(raw)) raw = "Unassigned";
        var cleaned = new string(raw.Select(ch => ":\\/?*[]".Contains(ch) ? ' ' : ch).ToArray()).Trim();
        if (cleaned.Length > 31) cleaned = cleaned[..31].TrimEnd();
        var candidate = cleaned;
        for (var n = 2; !used.Add(candidate); n++)
        {
            var suffix = $" ({n})";
            candidate = cleaned[..Math.Min(cleaned.Length, 31 - suffix.Length)].TrimEnd() + suffix;
        }
        return candidate;
    }

    private static byte[] Save(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
