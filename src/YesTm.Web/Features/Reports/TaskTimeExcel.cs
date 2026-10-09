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
        new("Segment",       (c, x) => c.Value = x.TL_SEGMENT_TYPE == "TRAVEL" ? "Travel" : "Work"),
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
        new("Idle Hrs",      (c, x) => c.Value = x.TL_IDLE_HOURS, true),
        new("Idle Reason",   (c, x) => c.Value = x.TL_IDLE_REASON),
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
        AddDefinitionsSheet(wb);
        return Save(wb);
    }

    /// <summary>
    /// Technician-wise export: a Summary tab (one row per technician) followed by one tab per
    /// technician whose lines are grouped by Job → Sheet with subtotals per job and a technician total.
    /// </summary>
    public static byte[] BuildTechnicianWise(IReadOnlyList<TaskTimeReportRow> rows, string filterSummary,
        UtilisationReport? utilisation = null)
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
        // Standard hours are earned per job + task and shared among the technicians by their share of
        // the work hours on it — the same rule as the Job-wise report, so both reports reconcile.
        var earned = EarnedStdByTech(rows);
        var sum = wb.AddWorksheet("Summary");
        sum.Cell(1, 1).Value = "Technician-wise Task & Time Report";
        sum.Cell(1, 1).Style.Font.Bold = true; sum.Cell(1, 1).Style.Font.FontSize = 14;
        sum.Cell(2, 1).Value = filterSummary;
        sum.Cell(3, 1).Value = $"Generated {DateTime.Now:dd-MMM-yyyy HH:mm} · {byTech.Count} technician(s) · {rows.Count} task line(s)";
        sum.Range(2, 1, 3, 1).Style.Font.FontColor = XLColor.DimGray;

        string[] sumHeaders = ["Tech Code", "Technician", "Jobs", "Task Lines", "Std Hrs", "Work Hrs", "Efficiency %",
            "Travel Hrs", "Idle Hrs", "Net Hrs", "Normal Hrs", "OT Hrs", "Labour Cost", "Food Allowance", "Total Cost"];
        for (var c = 0; c < sumHeaders.Length; c++) sum.Cell(5, c + 1).Value = sumHeaders[c];
        StyleHeader(sum.Range(5, 1, 5, sumHeaders.Length));

        var sr = 6;
        foreach (var t in byTech)
        {
            sum.Cell(sr, 1).Value = t.Code;
            sum.Cell(sr, 2).Value = t.Name;
            sum.Cell(sr, 3).Value = t.Rows.Select(x => x.TL_JOB_CODE).Distinct().Count();
            sum.Cell(sr, 4).Value = t.Rows.Count;
            WriteJobFigures(sum, sr, 5, Kpi(t.Rows, earned.GetValueOrDefault(t.Code)), Sum(t.Rows));
            sr++;
        }
        sum.Cell(sr, 2).Value = "Grand Total";
        sum.Cell(sr, 3).Value = rows.Select(x => x.TL_JOB_CODE).Distinct().Count();
        sum.Cell(sr, 4).Value = rows.Count;
        WriteJobFigures(sum, sr, 5, Kpi(rows, earned.Values.Sum()), Sum(rows));
        sum.Range(sr, 1, sr, sumHeaders.Length).Style.Font.Bold = true;
        sum.Range(6, 5, sr, sumHeaders.Length).Style.NumberFormat.Format = NumFmt;
        sum.SheetView.FreezeRows(5);
        sum.Columns().AdjustToContents();

        if (utilisation is not null) AddUtilisationSheet(wb, utilisation, filterSummary);

        // ---- One tab per technician ----
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Summary", "Utilisation", "Definitions" };
        foreach (var t in byTech)
        {
            var ws = wb.AddWorksheet(SheetName(t.Code, t.Name, usedNames));
            ws.Cell(1, 1).Value = $"{t.Code} — {t.Name}";
            ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 13;
            var tk = Kpi(t.Rows, earned.GetValueOrDefault(t.Code));
            ws.Cell(2, 1).Value = $"{filterSummary} · Std {tk.Std:N2} h · Work {tk.Work:N2} h · " +
                                  $"Efficiency {(tk.Efficiency is { } te ? te.ToString("N1") + "%" : "—")} · Travel {tk.Travel:N2} h · Idle {tk.Idle:N2} h";
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

        AddDefinitionsSheet(wb);
        return Save(wb);
    }

    /// <summary>
    /// Job-wise export: a Summary tab (one row per job: technicians, lines, standard vs actual work
    /// hours, efficiency, travel/idle, cost) followed by one tab per job whose lines are grouped by
    /// technician with a subtotal per technician and a job total.
    /// </summary>
    public static byte[] BuildJobWise(IReadOnlyList<TaskTimeReportRow> rows, string filterSummary)
    {
        using var wb = new XLWorkbook();

        var byJob = rows
            .GroupBy(x => x.TL_JOB_CODE ?? "")
            .Select(g => new { Code = g.Key, First = g.First(), Rows = g.ToList() })
            .OrderBy(j => j.Code)
            .ToList();

        // ---- Summary tab ----
        var sum = wb.AddWorksheet("Summary");
        sum.Cell(1, 1).Value = "Job-wise Task & Time Report";
        sum.Cell(1, 1).Style.Font.Bold = true; sum.Cell(1, 1).Style.Font.FontSize = 14;
        sum.Cell(2, 1).Value = filterSummary;
        sum.Cell(3, 1).Value = $"Generated {DateTime.Now:dd-MMM-yyyy HH:mm} · {byJob.Count} job(s) · {rows.Count} task line(s)";
        sum.Range(2, 1, 3, 1).Style.Font.FontColor = XLColor.DimGray;

        string[] headers = ["Job No", "Customer", "Location", "Job Status", "Sheet No", "First Work Date", "Last Work Date",
            "Technicians", "Task Lines", "Std Hrs", "Work Hrs", "Efficiency %", "Travel Hrs", "Idle Hrs",
            "Net Hrs", "Normal Hrs", "OT Hrs", "Labour Cost", "Food Allowance", "Total Cost"];
        for (var c = 0; c < headers.Length; c++) sum.Cell(5, c + 1).Value = headers[c];
        StyleHeader(sum.Range(5, 1, 5, headers.Length));

        var sr = 6;
        foreach (var j in byJob)
        {
            var dates = j.Rows.Select(x => x.TL_WORK_DATE ?? x.TL_START_DT).Where(d => d.HasValue).Select(d => d!.Value.Date).ToList();
            sum.Cell(sr, 1).Value = j.Code;
            sum.Cell(sr, 2).Value = j.First.TS_CUSTOMER_NAME;
            sum.Cell(sr, 3).Value = j.First.TS_LOCATION;
            sum.Cell(sr, 4).Value = j.First.TS_JOB_STATUS;
            sum.Cell(sr, 5).Value = string.Join(", ", j.Rows.Select(x => x.TS_SHEET_NO).Distinct());
            if (dates.Count > 0) { sum.Cell(sr, 6).Value = dates.Min(); sum.Cell(sr, 7).Value = dates.Max(); }
            sum.Cell(sr, 8).Value = j.Rows.Select(x => x.TL_TECH_CODE).Distinct().Count();
            sum.Cell(sr, 9).Value = j.Rows.Count;
            WriteJobFigures(sum, sr, 10, JobKpis(j.Rows, [j.Rows]), Sum(j.Rows));
            sr++;
        }
        sum.Cell(sr, 2).Value = "Grand Total";
        sum.Cell(sr, 8).Value = rows.Select(x => x.TL_TECH_CODE).Distinct().Count();
        sum.Cell(sr, 9).Value = rows.Count;
        WriteJobFigures(sum, sr, 10, JobKpis(rows, byJob.Select(j => (IReadOnlyList<TaskTimeReportRow>)j.Rows)), Sum(rows));
        sum.Range(sr, 1, sr, headers.Length).Style.Font.Bold = true;
        sum.Range(6, 6, sr, 7).Style.DateFormat.Format = DateFmt;
        sum.Range(6, 10, sr, headers.Length).Style.NumberFormat.Format = NumFmt;
        sum.SheetView.FreezeRows(5);
        sum.Columns().AdjustToContents();
        sum.Column(1).Width = Math.Max(sum.Column(1).Width, 10);
        sum.Column(2).Width = Math.Min(sum.Column(2).Width, 40);

        // ---- One tab per job ----
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Summary", "Definitions" };
        foreach (var j in byJob)
        {
            var ws = wb.AddWorksheet(SheetName("Job " + j.Code, j.First.TS_CUSTOMER_NAME ?? "", usedNames));
            var k = JobKpis(j.Rows, [j.Rows]);
            ws.Cell(1, 1).Value = $"Job {j.Code} — {j.First.TS_CUSTOMER_NAME}";
            ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 13;
            ws.Cell(2, 1).Value = $"{filterSummary} · {j.First.TS_LOCATION} · {j.First.TS_JOB_STATUS} · " +
                                  $"Std {k.Std:N2} h · Work {k.Work:N2} h · Efficiency {(k.Efficiency is { } e ? e.ToString("N1") + "%" : "—")}";
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.DimGray;

            var r = WriteHeader(ws, 4);
            foreach (var tech in j.Rows.GroupBy(x => x.TL_TECH_CODE ?? "")
                         .OrderBy(g => g.Select(x => x.TL_TECH_NAME).FirstOrDefault()))
            {
                var name = tech.Select(x => x.TL_TECH_NAME).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? "";
                ws.Cell(r, 1).Value = $"{tech.Key} — {name}";
                ws.Range(r, 1, r, Columns.Length).Merge().Style
                    .Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#E8EEF9"));
                r++;
                foreach (var x in tech.OrderBy(x => x.TL_WORK_DATE ?? x.TL_START_DT).ThenBy(x => x.TL_START_DT))
                    WriteRow(ws, r++, x);
                WriteTotals(ws, r++, $"{name} total", Sum(tech.ToList()), bold: true, fill: "#F5F7FB");
            }
            WriteTotals(ws, r, $"Job {j.Code} total", Sum(j.Rows), bold: true, fill: "#FFF3CD");
            ws.SheetView.FreezeRows(4);
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = Math.Min(ws.Column(1).Width, 22);
        }

        AddDefinitionsSheet(wb);
        return Save(wb);
    }

    /// <summary>Standard vs actual work hours (travel and idle are excluded from the efficiency base).</summary>
    private sealed record JobKpi(decimal Std, decimal Work, decimal Travel, decimal Idle)
    {
        public decimal? Efficiency => Work > 0 ? Math.Round(Std / Work * 100m, 1) : null;
    }

    /// <summary>KPIs for a set of lines given the standard hours earned on them.</summary>
    private static JobKpi Kpi(IReadOnlyList<TaskTimeReportRow> rows, decimal std)
    {
        var work = rows.Where(x => x.TL_SEGMENT_TYPE != "TRAVEL").Sum(x => x.TL_NET_HOURS - x.TL_IDLE_HOURS);
        var travel = rows.Where(x => x.TL_SEGMENT_TYPE == "TRAVEL").Sum(x => x.TL_NET_HOURS)
                   + rows.Where(x => x.TL_SEGMENT_TYPE != "TRAVEL").Sum(x => x.TL_TRAVEL_HOURS);
        return new JobKpi(std, work, travel, rows.Sum(x => x.TL_IDLE_HOURS));
    }

    private static JobKpi JobKpis(IReadOnlyList<TaskTimeReportRow> rows, IEnumerable<IReadOnlyList<TaskTimeReportRow>> jobs) =>
        Kpi(rows, jobs.Sum(j => StdGroups(j).Sum(g => g.Std)));

    /// <summary>
    /// Standard hours belong to the task, not to each line: per job + task the standard counts once
    /// (summing TL_STD_HOURS per line would inflate it when a task spans days or technicians).
    /// </summary>
    private static IEnumerable<(decimal Std, List<TaskTimeReportRow> Lines)> StdGroups(IEnumerable<TaskTimeReportRow> rows) => rows
        .Where(x => x.TL_SEGMENT_TYPE != "TRAVEL" && !string.IsNullOrWhiteSpace(x.TL_TASK_NAME))
        .GroupBy(x => (Job: x.TL_JOB_CODE ?? "", Task: x.TL_TASK_NAME!.ToUpperInvariant()))
        .Select(g => (g.Max(x => x.TL_STD_HOURS), g.ToList()));

    /// <summary>Each job + task standard split among its technicians by their share of the work hours.</summary>
    internal static Dictionary<string, decimal> EarnedStdByTech(IEnumerable<TaskTimeReportRow> rows)
    {
        var earned = new Dictionary<string, decimal>();
        foreach (var (std, lines) in StdGroups(rows))
        {
            var byTech = lines.GroupBy(x => x.TL_TECH_CODE ?? "")
                .Select(g => (Tech: g.Key, Work: g.Sum(x => x.TL_NET_HOURS - x.TL_IDLE_HOURS))).ToList();
            var total = byTech.Sum(x => x.Work);
            foreach (var (tech, work) in byTech)
            {
                var share = total > 0 ? std * work / total : std / byTech.Count;
                earned[tech] = earned.GetValueOrDefault(tech) + share;
            }
        }
        return earned.ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value, 2, MidpointRounding.AwayFromZero));
    }

    private static void WriteJobFigures(IXLWorksheet ws, int row, int col, JobKpi k, ReportTotals t)
    {
        ws.Cell(row, col).Value = k.Std;
        ws.Cell(row, col + 1).Value = k.Work;
        if (k.Efficiency is { } e) ws.Cell(row, col + 2).Value = e;
        ws.Cell(row, col + 3).Value = k.Travel;
        ws.Cell(row, col + 4).Value = k.Idle;
        ws.Cell(row, col + 5).Value = t.Net;
        ws.Cell(row, col + 6).Value = t.Normal;
        ws.Cell(row, col + 7).Value = t.Ot;
        ws.Cell(row, col + 8).Value = t.Cost;
        ws.Cell(row, col + 9).Value = t.Food;
        ws.Cell(row, col + 10).Value = t.Total;
    }

    // =====================================================================================
    //  Availability & utilisation + definitions (shared by the workbooks)
    // =====================================================================================

    /// <summary>
    /// "Utilisation" tab: one row per technician with the availability chain written as live Excel
    /// formulas (Scheduled = Calendar − Off − Holiday, Available = Scheduled − Leave − Absent,
    /// Available hrs = Available days × Std hrs/day, ratios) so the reader can click any figure and see
    /// how it is made. Inputs (days, booked hours) are values from TM / HRMS.
    /// </summary>
    public static void AddUtilisationSheet(XLWorkbook wb, UtilisationReport u, string filterSummary)
    {
        var ws = wb.AddWorksheet("Utilisation");
        ws.Cell(1, 1).Value = "Availability & Utilisation";
        ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(2, 1).Value = $"Period {u.PeriodText} · Std day {u.StdHoursPerDay:0.00} h · Weekend {u.WeekendText} · {filterSummary}";
        ws.Cell(3, 1).Value = "Blue columns are calculated by Excel formulas from the inputs to their left — see the Definitions tab.";
        ws.Range(2, 1, 3, 1).Style.Font.FontColor = XLColor.DimGray;

        string[] h = ["Tech Code", "Technician", "HRMS Code", "Data Source",                         // A-D
            "Calendar Days", "Weekend / Off", "Holidays", "Scheduled Days",                            // E-H
            "Leave Days", "Absent Days", "Available Days", "Std Hrs / Day", "Available Hrs",           // I-M
            "Booked Hrs (Net)", "Booked Normal Hrs", "Booked OT Hrs", "Paid Hrs", "Unbooked Hrs",      // N-R
            "Work Hrs", "Travel Hrs", "Idle Hrs", "Std Hrs (earned)",                                  // S-V
            "Coverage %", "Utilisation %", "Efficiency %", "Productivity %",                          // W-Z
            "Clocked Hrs (HRMS)", "Clocked − Booked", "Leave Detail", "Calculation"];                  // AA-AD
        const int hr = 5;
        for (var c = 0; c < h.Length; c++) ws.Cell(hr, c + 1).Value = h[c];
        StyleHeader(ws.Range(hr, 1, hr, h.Length));
        int[] calc = [8, 11, 13, 17, 18, 23, 24, 25, 26, 28];
        foreach (var c in calc) ws.Cell(hr, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#CFE2FF");

        var r = hr + 1;
        foreach (var t in u.Technicians)
        {
            var a = t.Kpi.Avail;
            ws.Cell(r, 1).Value = t.TechCode;
            ws.Cell(r, 2).Value = t.TechName;
            ws.Cell(r, 3).Value = t.HrmsCode ?? "not mapped";
            ws.Cell(r, 4).Value = a.Source;
            ws.Cell(r, 5).Value = a.CalendarDays;
            ws.Cell(r, 6).Value = a.OffDays;
            ws.Cell(r, 7).Value = a.HolidayDays;
            ws.Cell(r, 8).FormulaA1 = $"E{r}-F{r}-G{r}";
            ws.Cell(r, 9).Value = a.LeaveDays;
            ws.Cell(r, 10).Value = a.AbsentDays;
            ws.Cell(r, 11).FormulaA1 = $"MAX(0,H{r}-I{r}-J{r})";
            ws.Cell(r, 12).Value = a.StdHoursPerDay;
            ws.Cell(r, 13).FormulaA1 = $"K{r}*L{r}";
            ws.Cell(r, 14).Value = t.Kpi.BookedNet;
            ws.Cell(r, 15).Value = t.Kpi.BookedNormal;
            ws.Cell(r, 16).Value = t.Kpi.BookedOt;
            ws.Cell(r, 17).FormulaA1 = $"M{r}+P{r}";
            ws.Cell(r, 18).FormulaA1 = $"MAX(0,M{r}-O{r})";
            ws.Cell(r, 19).Value = t.Kpi.WorkHours;
            ws.Cell(r, 20).Value = t.Kpi.TravelHours;
            ws.Cell(r, 21).Value = t.Kpi.IdleHours;
            ws.Cell(r, 22).Value = t.Kpi.StdEarned;
            WritePctFormulas(ws, r);
            if (a.HrmsDays > 0)
            {
                ws.Cell(r, 27).Value = a.AttendanceHours;
                ws.Cell(r, 28).FormulaA1 = $"AA{r}-N{r}";
            }
            ws.Cell(r, 29).Value = a.LeaveText;
            ws.Cell(r, 30).Value = a.Formula;
            if (!t.Mapped) ws.Range(r, 1, r, 4).Style.Font.FontColor = XLColor.FromHtml("#8A6100");
            r++;
        }

        // Totals: sums of the inputs, ratios recomputed from the summed columns (not averaged).
        var first = hr + 1;
        var last = r - 1;
        ws.Cell(r, 2).Value = "Total";
        if (last >= first)
        {
            foreach (var c in new[] { 5, 6, 7, 8, 9, 10, 11, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 27, 28 })
            {
                var col = ws.Column(c).ColumnLetter();
                ws.Cell(r, c).FormulaA1 = $"SUM({col}{first}:{col}{last})";
            }
            WritePctFormulas(ws, r);
        }
        ws.Range(r, 1, r, h.Length).Style.Font.Bold = true;
        ws.Range(r, 1, r, h.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3CD");

        ws.Range(first, 5, r, 28).Style.NumberFormat.Format = NumFmt;
        ws.Range(first, 23, r, 26).Style.NumberFormat.Format = "0.0\"%\"";
        ws.Range(first, 5, r, 7).Style.NumberFormat.Format = "0.##";
        ws.Range(first, 8, r, 11).Style.NumberFormat.Format = "0.##";
        ws.SheetView.FreezeRows(hr);
        ws.SheetView.FreezeColumns(2);
        ws.Columns().AdjustToContents();
        ws.Column(30).Width = Math.Min(ws.Column(30).Width, 90);
        ws.Column(29).Width = Math.Min(ws.Column(29).Width, 40);

        var n = r + 2;
        ws.Cell(n, 1).Value = "Notes";
        ws.Cell(n, 1).Style.Font.Bold = true;
        ws.Cell(n + 1, 1).Value = "• Days come from HRMS attendance (V_ATT_TRAN, company 39) for technicians mapped under HRMS Sync; days without HRMS data use the duty-timing weekend only (no holidays/leave known).";
        ws.Cell(n + 2, 1).Value = "• Std Hrs (earned): each job + task standard counted once and shared among its technicians by their work hours.";
        ws.Cell(n + 3, 1).Value = "• Brown codes = technician not mapped to HRMS yet: holidays, leave and absence are unknown, so availability is overstated.";
    }

    private static void WritePctFormulas(IXLWorksheet ws, int r)
    {
        ws.Cell(r, 23).FormulaA1 = $"IF(M{r}>0,O{r}/M{r}*100,\"\")";   // coverage     = booked normal / available
        ws.Cell(r, 24).FormulaA1 = $"IF(Q{r}>0,S{r}/Q{r}*100,\"\")";   // utilisation  = work / paid
        ws.Cell(r, 25).FormulaA1 = $"IF(S{r}>0,V{r}/S{r}*100,\"\")";   // efficiency   = std / work
        ws.Cell(r, 26).FormulaA1 = $"IF(Q{r}>0,V{r}/Q{r}*100,\"\")";   // productivity = std / paid
    }

    /// <summary>Definitions tab — what every figure in these workbooks means and how it is calculated.</summary>
    public static void AddDefinitionsSheet(XLWorkbook wb, decimal? stdHoursPerDay = null)
    {
        var ws = wb.AddWorksheet("Definitions");
        ws.Cell(1, 1).Value = "Definitions & Formulas";
        ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 14;
        string[,] d =
        {
            { "Term", "Meaning", "Formula" },
            { "Calendar Days", "Every day in the report period.", "To − From + 1" },
            { "Weekend / Off", "Rostered off days (HRMS 'OFF'), or the Duty Timing weekend days when there is no HRMS data.", "" },
            { "Holidays", "Public holidays from HRMS (half holidays count 0.5).", "" },
            { "Scheduled Days", "Days the technician was expected to work.", "Calendar − Weekend/Off − Holidays" },
            { "Leave Days", "Approved leave from HRMS attendance (half days count 0.5); Leave Detail lists the types.", "" },
            { "Absent Days", "Scheduled days with no attendance and no leave (HRMS 'ABSENT').", "" },
            { "Available Days", "Days the technician was actually available for work.", "Scheduled − Leave − Absent" },
            { "Std Hrs / Day", $"One standard working day (Duty Timing master{(stdHoursPerDay is { } s ? $", currently {s:0.00} h" : "")}).", "" },
            { "Available Hrs", "Paid normal hours the technician could have booked.", "Available Days × Std Hrs/Day" },
            { "Net Hrs", "Hours booked on a line: end − start − lunch (includes idle and travel time).", "Normal + OT" },
            { "Normal / OT Hrs", "Net split by the duty window: inside the window on a working day = Normal; before/after it, weekends and holidays = OT.", "" },
            { "Booked Normal Hrs", "Normal hours booked on job time sheets in the period.", "Σ Normal" },
            { "Paid Hrs", "Hours the technician was on the clock: available normal hours plus overtime worked.", "Available Hrs + Booked OT" },
            { "Unbooked Hrs", "Available normal hours that no job carries.", "MAX(0, Available Hrs − Booked Normal)" },
            { "Work Hrs", "Productive hours on Work segments/lines.", "Σ (Net − Idle) on non-travel lines" },
            { "Travel Hrs", "Travel segments (field reports) plus travel time recorded on lines.", "" },
            { "Idle Hrs", "Waiting time inside a segment (no parts, customer not ready, permit…). Paid, not productive.", "" },
            { "Std Hrs (earned)", "Standard hours of the tasks done. Each job + task standard counts once and is shared among its technicians by their work hours.", "Σ task std × (tech work ÷ all work on that task)" },
            { "Coverage %", "How much of the available day is accounted for by job bookings.", "Booked Normal ÷ Available Hrs × 100" },
            { "Utilisation %", "Share of paid time spent on productive job work.", "Work Hrs ÷ Paid Hrs × 100" },
            { "Efficiency %", "Speed against standard: over 100 % = faster than standard.", "Std Hrs (earned) ÷ Work Hrs × 100" },
            { "Productivity %", "Overall output against paid time.", "Std Hrs (earned) ÷ Paid Hrs × 100  (= Utilisation × Efficiency)" },
            { "Clocked Hrs (HRMS)", "Hours punched in HRMS attendance.", "" },
            { "Clocked − Booked", "Time at work that is not on any job (positive) or booked beyond attendance (negative).", "Clocked − Booked Net" },
            { "Labour Cost", "Normal × Rate + OT × OT Rate.", "" },
            { "Total Cost", "Labour cost + food allowance (field vehicle cost is shown on the Field Service Report).", "" },
        };
        for (var i = 0; i < d.GetLength(0); i++)
            for (var j = 0; j < 3; j++)
                ws.Cell(i + 3, j + 1).Value = d[i, j];
        StyleHeader(ws.Range(3, 1, 3, 3));
        ws.Column(1).Width = 22;
        ws.Column(2).Width = 95;
        ws.Column(3).Width = 60;
        ws.Range(4, 1, d.GetLength(0) + 2, 1).Style.Font.Bold = true;
        ws.Range(4, 2, d.GetLength(0) + 2, 3).Style.Alignment.WrapText = true;
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
