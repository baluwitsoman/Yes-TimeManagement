using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace YesTm.Web.Features.Hrms;

/// <summary>A proposed TM → HRMS employee match.</summary>
public sealed record EmpMatch(string TmCode, string? HrmsCode, string MatchType, decimal Confidence)
{
    /// <summary>Exact identifier matches are safe to use without review; name matches need a human.</summary>
    public bool AutoConfirm => MatchType is MatchTypes.Code or MatchTypes.EmploymentNo;
}

public static class MatchTypes
{
    public const string Code = "CODE";
    public const string EmploymentNo = "EMPLOYMENT_NO";
    public const string Name = "NAME";
    public const string FuzzyName = "FUZZY_NAME";
    public const string Manual = "MANUAL";
    public const string None = "NONE";
}

/// <summary>
/// Suggests which HRMS employee a TM technician is, in order of reliability:
///   1. same code (ignoring case, separators and zero padding: YES027 = YES0027)   100
///   2. HRMS employment number = TM code                                           100
///   3. same name (ignoring case, punctuation, word order)                           95
///   4. similar name (token overlap + edit distance), only when ≥ 80 and unambiguous
/// A HRMS employee is never proposed for two technicians: the higher-confidence claim wins.
/// Pure (no I/O) so it is unit-tested.
/// </summary>
public static class EmployeeMatcher
{
    public const decimal FuzzyThreshold = 80m;

    public static IReadOnlyList<EmpMatch> Match(
        IEnumerable<(string Code, string? Name)> tmEmployees, IReadOnlyList<HrmsEmployeeDto> hrms)
    {
        var byCode = hrms.Where(h => !string.IsNullOrWhiteSpace(h.EMP_CODE))
            .GroupBy(h => NormCode(h.EMP_CODE)).ToDictionary(g => g.Key, g => g.First());
        var byEmpNo = hrms.Where(h => !string.IsNullOrWhiteSpace(h.EMPLOYMENT_NO))
            .GroupBy(h => NormCode(h.EMPLOYMENT_NO)).Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => g.First());
        var byName = hrms.Where(h => !string.IsNullOrWhiteSpace(h.EMP_NAME))
            .GroupBy(h => NormName(h.EMP_NAME)).Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => g.First());

        var proposals = new List<EmpMatch>();
        foreach (var (code, name) in tmEmployees)
        {
            var nc = NormCode(code);
            if (byCode.TryGetValue(nc, out var h)) { proposals.Add(new(code, h.EMP_CODE, MatchTypes.Code, 100m)); continue; }
            if (byEmpNo.TryGetValue(nc, out h)) { proposals.Add(new(code, h.EMP_CODE, MatchTypes.EmploymentNo, 100m)); continue; }
            var nn = NormName(name);
            if (nn.Length > 0 && byName.TryGetValue(nn, out h)) { proposals.Add(new(code, h.EMP_CODE, MatchTypes.Name, 95m)); continue; }

            // Fuzzy: best and second-best similarity; ambiguous ties are not proposed.
            var scored = nn.Length == 0 ? [] : hrms
                .Where(x => !string.IsNullOrWhiteSpace(x.EMP_NAME))
                .Select(x => (x, Score: Similarity(nn, NormName(x.EMP_NAME))))
                .OrderByDescending(t => t.Score).Take(2).ToList();
            if (scored.Count > 0 && scored[0].Score >= FuzzyThreshold
                && (scored.Count == 1 || scored[0].Score - scored[1].Score >= 5m))
                proposals.Add(new(code, scored[0].x.EMP_CODE, MatchTypes.FuzzyName, scored[0].Score));
            else
                proposals.Add(new(code, null, MatchTypes.None, 0m));
        }

        // One HRMS employee per technician: keep the strongest claim, demote the rest to NONE.
        var winners = proposals.Where(p => p.HrmsCode is not null)
            .GroupBy(p => p.HrmsCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Confidence).First().TmCode, StringComparer.OrdinalIgnoreCase);
        return proposals.Select(p => p.HrmsCode is null || winners[p.HrmsCode] == p.TmCode
            ? p : p with { HrmsCode = null, MatchType = MatchTypes.None, Confidence = 0m }).ToList();
    }

    /// <summary>Upper-case letters/digits only, and zero padding removed from every digit run.</summary>
    public static string NormCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return string.Empty;
        var s = Regex.Replace(code.ToUpperInvariant(), "[^A-Z0-9]", "");
        return Regex.Replace(s, "[0-9]+", m => m.Value.TrimStart('0') is { Length: > 0 } t ? t : "0");
    }

    /// <summary>Accents/punctuation removed, upper-case, words sorted ("L Hariharan" = "Hariharan L.").</summary>
    public static string NormName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        var plain = new string(name.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray());
        var words = Regex.Replace(plain.ToUpperInvariant(), "[^A-Z ]", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words.OrderBy(w => w, StringComparer.Ordinal));
    }

    /// <summary>0-100: average of word-overlap (initials count as a match) and character edit similarity.</summary>
    public static decimal Similarity(string a, string b)
    {
        if (a.Length == 0 || b.Length == 0) return 0m;
        if (a == b) return 100m;
        var ta = a.Split(' ');
        var tb = b.Split(' ');
        static bool Same(string x, string y) =>
            x == y || (x.Length == 1 && y.StartsWith(x, StringComparison.Ordinal)) || (y.Length == 1 && x.StartsWith(y, StringComparison.Ordinal));
        var hits = ta.Count(x => tb.Any(y => Same(x, y)));
        var overlap = 2m * hits / (ta.Length + tb.Length);
        var edit = 1m - (decimal)Levenshtein(a, b) / Math.Max(a.Length, b.Length);
        return Math.Round((overlap + edit) / 2m * 100m, 1);
    }

    private static int Levenshtein(string a, string b)
    {
        var prev = new int[b.Length + 1];
        var cur = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) prev[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            cur[0] = i;
            for (var j = 1; j <= b.Length; j++)
                cur[j] = Math.Min(Math.Min(cur[j - 1] + 1, prev[j] + 1), prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            (prev, cur) = (cur, prev);
        }
        return prev[b.Length];
    }
}
