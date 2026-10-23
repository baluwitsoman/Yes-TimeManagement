namespace YesTm.Web.Common.TimeCalc;

/// <summary>One booked time range of a technician (a line being posted, or one already on a sheet).</summary>
public sealed record BookedRange(string TechCode, string? TechName, DateTime Start, DateTime End, string Where);

/// <summary>
/// A technician can have several bookings on the same day (e.g. 08:00–12:00 leave, 13:00–17:30 a job),
/// but never two at the same time. Touching ranges (12:00–13:00 then 13:00–…) are fine.
/// Pure so it is unit-tested; the pages feed it the posted lines plus the technicians' lines on other sheets.
/// </summary>
public static class BookingOverlap
{
    public static IReadOnlyList<string> Find(IReadOnlyList<BookedRange> posted, IReadOnlyList<BookedRange> existing)
    {
        var problems = new List<string>();

        // Within the sheet being posted.
        for (var i = 0; i < posted.Count; i++)
            for (var j = i + 1; j < posted.Count; j++)
                if (Same(posted[i], posted[j]) && Overlaps(posted[i], posted[j]))
                    problems.Add($"{Name(posted[i])}: {Range(posted[i])} ({posted[i].Where}) overlaps {Range(posted[j])} ({posted[j].Where}).");

        // Against every other active sheet.
        foreach (var p in posted)
            foreach (var e in existing.Where(e => Same(p, e) && Overlaps(p, e)))
                problems.Add($"{Name(p)}: {Range(p)} ({p.Where}) overlaps {Range(e)} already booked on {e.Where}.");

        return problems;
    }

    private static bool Same(BookedRange a, BookedRange b) => string.Equals(a.TechCode, b.TechCode, StringComparison.OrdinalIgnoreCase);
    private static bool Overlaps(BookedRange a, BookedRange b) => a.Start < b.End && b.Start < a.End;
    private static string Name(BookedRange r) => string.IsNullOrWhiteSpace(r.TechName) ? r.TechCode : r.TechName!;
    private static string Range(BookedRange r) =>
        $"{r.Start:dd-MMM HH:mm}–{(r.End.Date == r.Start.Date ? r.End.ToString("HH:mm") : r.End.ToString("dd-MMM HH:mm"))}";
}
