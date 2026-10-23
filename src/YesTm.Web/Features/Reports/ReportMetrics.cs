namespace YesTm.Web.Features.Reports;

/// <summary>
/// The hour buckets every report uses, from one set of lines — so Task &amp; Time, Job-wise,
/// Technician-wise and Utilisation can never disagree:
///   Work     = real job lines, not travel: Net − idle inside the segment
///   Travel   = travel segments + travel recorded on job lines
///   Idle     = idle inside job segments + lines on an IDLE non-job activity
///   Absence  = holiday / leave / week-off non-job lines (reduce availability)
///   NonProd  = non-productive non-job lines (training, housekeeping…)
/// </summary>
public sealed record ReportMetrics(decimal Work, decimal Travel, decimal Idle, decimal Absence, decimal NonProd)
{
    public static ReportMetrics Of(IEnumerable<TaskTimeReportRow> rows)
    {
        decimal work = 0, travel = 0, idle = 0, absence = 0, nonProd = 0;
        foreach (var x in rows)
        {
            if (x.IsAbsence) { absence += x.TL_NET_HOURS; continue; }
            if (x.NJ_CATEGORY == "IDLE") { idle += x.TL_NET_HOURS; continue; }
            if (x.NJ_CATEGORY == "NONPROD") { nonProd += x.TL_NET_HOURS; continue; }
            if (x.TL_SEGMENT_TYPE == "TRAVEL") { travel += x.TL_NET_HOURS; continue; }
            work += x.TL_NET_HOURS - x.TL_IDLE_HOURS;
            idle += x.TL_IDLE_HOURS;
            travel += x.TL_TRAVEL_HOURS;
        }
        return new ReportMetrics(work, travel, idle, absence, nonProd);
    }
}
