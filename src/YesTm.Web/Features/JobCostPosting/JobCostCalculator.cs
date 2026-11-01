using System.Globalization;

namespace YesTm.Web.Features.JobCostPosting;

/// <summary>
/// Turns a time sheet into its month-by-month absorption vouchers. Pure — no database — so the
/// rules are unit-tested:
/// <list type="bullet">
/// <item>WORK line: (net − idle) hours at the LABOUR rate of the line's location; its own travel hours at TRAVEL_HR.</item>
/// <item>TRAVEL segment (Field Service Report): (net − idle) hours at TRAVEL_HR.</item>
/// <item>Idle hours are never charged to the job; OT hours use the same flat rate; food allowance is not posted.</item>
/// <item>FSR vehicle km once per sheet at TRAVEL_KM, dated on the report date.</item>
/// <item>Dr one line per employee × cost code (job-tagged), Cr one line per absorbed account (no job).</item>
/// <item>Each calendar month is its own voucher, dated on the latest work date in that month.</item>
/// </list>
/// </summary>
public static class JobCostCalculator
{
    public static JobCostCalcResult Calculate(string sheetNo, string jobCode, string? sheetLocation,
        IReadOnlyList<CostLineInput> lines, CostKmInput? km, IReadOnlyList<TM_JCP_COST_CODE> codes, DateTime? startDate)
    {
        var result = new JobCostCalcResult();
        var start = startDate?.Date ?? DateTime.MinValue;
        // (month, tech, cost code id) → accumulated quantity; the code row carries rate + accounts.
        var acc = new Dictionary<(DateTime Month, string Tech, decimal CodeId), (decimal Qty, string? TechName, TM_JCP_COST_CODE Code)>();
        var latest = new Dictionary<DateTime, DateTime>();
        var uncosted = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        void Add(DateTime date, string? tech, string? techName, string type, string? location, decimal qty)
        {
            if (qty <= 0m || date.Date < start) return;
            var code = Pick(codes, type, location, date);
            var what = type == CostTypes.Labour ? $"{location ?? "(no location)"} labour" : CostTypes.Label(type);
            if (code is null || code.JCC_DR_VEL_CODE is null || code.JCC_CR_VEL_CODE is null)
            {
                var uom = type == CostTypes.TravelKm ? "km" : "h";
                var why = $"{uom} of {what} — {(code is null ? "no Job Cost rate" : "no GL account in Job Cost Posting Setup")}";
                uncosted[why] = uncosted.GetValueOrDefault(why) + qty;
                return;
            }
            var month = new DateTime(date.Year, date.Month, 1);
            var key = (month, tech ?? string.Empty, code.JCC_ID);
            var (curQty, curName, _) = acc.TryGetValue(key, out var cur) ? cur : (0m, techName, code);
            acc[key] = (curQty + qty, curName ?? techName, code);
            latest[month] = latest.TryGetValue(month, out var d) && d >= date.Date ? d : date.Date;
        }

        foreach (var l in lines)
        {
            var productive = Math.Max(0m, l.NetHours - Math.Max(0m, l.IdleHours));
            if (string.Equals(l.SegmentType, "TRAVEL", StringComparison.OrdinalIgnoreCase))
            {
                Add(l.WorkDate, l.TechCode, l.TechName, CostTypes.TravelHours, null, productive);
                continue;
            }
            var location = string.IsNullOrWhiteSpace(l.Location) ? sheetLocation : l.Location;
            Add(l.WorkDate, l.TechCode, l.TechName, CostTypes.Labour, location, productive);
            Add(l.WorkDate, l.TechCode, l.TechName, CostTypes.TravelHours, null, Math.Max(0m, l.TravelHours));
        }

        if (km is { TotalKm: > 0m })
        {
            var kmDate = km.ReportDate?.Date ?? (lines.Count > 0 ? lines.Max(l => l.WorkDate.Date) : (DateTime?)null);
            if (kmDate.HasValue) Add(kmDate.Value, null, null, CostTypes.TravelKm, null, km.TotalKm);
        }

        foreach (var month in acc.Keys.Select(k => k.Month).Distinct().OrderBy(m => m))
        {
            var draft = new JvDraft { Month = month, JvDate = latest[month] };
            var credits = new Dictionary<decimal, decimal>();
            foreach (var (key, v) in acc.Where(a => a.Key.Month == month)
                         .OrderBy(a => a.Value.Code.JCC_TYPE == CostTypes.TravelKm ? 1 : 0)
                         .ThenBy(a => a.Value.TechName ?? a.Key.Tech, StringComparer.OrdinalIgnoreCase)
                         .ThenBy(a => a.Value.Code.JCC_TYPE).ThenBy(a => a.Value.Code.JCC_DESC))
            {
                var qty = Math.Round(v.Qty, 2, MidpointRounding.AwayFromZero);
                var amount = Math.Round(qty * v.Code.JCC_RATE, 3, MidpointRounding.AwayFromZero);
                if (amount <= 0m) continue;
                var tech = key.Tech.Length == 0 ? null : key.Tech;
                draft.Items.Add(new JvCostItem(v.Code.JCC_DESC, tech, v.TechName, qty, v.Code.Uom, v.Code.JCC_RATE, amount));
                var who = v.Code.JCC_TYPE == CostTypes.TravelKm
                    ? string.Join(" ", new[] { km?.ReportNo is { Length: > 0 } r ? "FSR " + r : null, km?.PlateNo }.Where(s => !string.IsNullOrWhiteSpace(s)))
                    : $"{tech} {v.TechName}".Trim();
                draft.Lines.Add(new JvLine(v.Code.JCC_DR_VEL_CODE!.Value, amount, jobCode,
                    Join(sheetNo, v.Code.JCC_DESC, who, $"{Fmt(qty, 2)} {v.Code.Uom} x {Fmt(v.Code.JCC_RATE, 3)}")));
                credits[v.Code.JCC_CR_VEL_CODE!.Value] = credits.GetValueOrDefault(v.Code.JCC_CR_VEL_CODE!.Value) + amount;
            }
            foreach (var (vel, amount) in credits.OrderBy(c => c.Key))
                draft.Lines.Add(new JvLine(vel, -amount, null, Join(sheetNo, "Job cost absorbed", "job " + jobCode)));
            if (draft.Items.Count > 0) result.Vouchers.Add(draft);
        }

        foreach (var (why, qty) in uncosted)
            result.Warnings.Add($"{Fmt(qty, 2)} {why}: not posted.");
        return result;
    }

    /// <summary>The active rate row of this type (and location, for labour) in force on the date.</summary>
    public static TM_JCP_COST_CODE? Pick(IReadOnlyList<TM_JCP_COST_CODE> codes, string type, string? location, DateTime date) =>
        codes.Where(c => c.Active && c.JCC_TYPE == type && c.JCC_EFFECTIVE_FROM.Date <= date.Date
                         && (type != CostTypes.Labour || string.Equals(c.JCC_LOCATION?.Trim(), location?.Trim(), StringComparison.OrdinalIgnoreCase)))
             .OrderByDescending(c => c.JCC_EFFECTIVE_FROM).ThenByDescending(c => c.JCC_ID)
             .FirstOrDefault();

    private static string Join(params string?[] parts) =>
        string.Join(" · ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static string Fmt(decimal v, int dp) => v.ToString("N" + dp, CultureInfo.InvariantCulture);
}
