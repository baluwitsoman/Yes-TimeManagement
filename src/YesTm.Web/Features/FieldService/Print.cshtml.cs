using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using YesTm.Web.Common.Security;
using YesTm.Web.Features.TimeBooking;

namespace YesTm.Web.Features.FieldService;

/// <summary>Printable Field Service Report in the company FSR layout (one A4 page per report).</summary>
public class PrintModel : PageModel
{
    private readonly ITimeBookingRepository _repo;
    private readonly IFieldServiceRepository _fsr;

    public PrintModel(ITimeBookingRepository repo, IFieldServiceRepository fsr)
    {
        _repo = repo;
        _fsr = fsr;
    }

    public TimeSheetEditDto Sheet { get; private set; } = new();
    public TM_FIELD_REPORT Report { get; private set; } = new();
    public IReadOnlyList<TM_FIELD_REPORT_CREW> Crew { get; private set; } = [];
    /// <summary>Distinct crew segments (the crew shares times; one row per date + segment + from/to).</summary>
    public IReadOnlyList<SegmentRow> Segments { get; private set; } = [];
    public IReadOnlyList<CrewHours> Hours { get; private set; } = [];

    public sealed record SegmentRow(DateTime Date, string Type, DateTime From, DateTime To, decimal Lunch, decimal Duration,
        decimal Normal, decimal Ot, decimal Idle, string? Task, int Technicians);
    public sealed record CrewHours(string Code, string? Name, string? FaCode, decimal Work, decimal Travel, decimal Idle,
        decimal Normal, decimal Ot, decimal Labour, decimal Food);

    public async Task<IActionResult> OnGetAsync(decimal tsId, CancellationToken ct)
    {
        var dto = await _repo.GetTimeSheetForEditAsync(tsId, ct);
        if (dto is null) return NotFound();

        var isAdmin = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.SiteAdmin);
        var emp = User.EmpCode();
        if (!isAdmin && dto.TS_CREATION_USER_ID != User.UserId()
            && !(emp is not null && dto.Lines.Any(l => string.Equals(l.TL_TECH_CODE, emp, StringComparison.OrdinalIgnoreCase))))
            return Forbid();

        Sheet = dto;
        Report = await _fsr.GetReportAsync(tsId, ct) ?? new TM_FIELD_REPORT();
        Crew = await _fsr.GetCrewAsync(tsId, ct);

        Segments = dto.Lines
            .GroupBy(l => (Date: l.TL_WORK_DATE ?? l.TL_START_DT.Date, Type: l.TL_SEGMENT_TYPE ?? SegmentTypes.Work,
                           l.TL_START_DT, l.TL_END_DT, l.TL_TASK_NAME))
            .Select(g =>
            {
                var f = g.First();
                return new SegmentRow(g.Key.Date, g.Key.Type, f.TL_START_DT, f.TL_END_DT, f.TL_LUNCH_HOURS, f.TL_NET_HOURS,
                    f.TL_NORMAL_HOURS, f.TL_OT_HOURS, g.Max(x => x.TL_IDLE_HOURS),
                    g.Key.Type == SegmentTypes.Travel ? null : f.TL_TASK_NAME, g.Count());
            })
            .OrderBy(s => s.From).ThenBy(s => s.Type)
            .ToList();

        var names = Crew.ToDictionary(c => c.FRC_EMP_CODE, StringComparer.OrdinalIgnoreCase);
        var order = Crew.Select((c, i) => (c.FRC_EMP_CODE, i)).ToDictionary(x => x.FRC_EMP_CODE, x => x.i, StringComparer.OrdinalIgnoreCase);
        Hours = dto.Lines.Where(l => !string.IsNullOrWhiteSpace(l.TL_TECH_CODE))
            .GroupBy(l => l.TL_TECH_CODE!, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                names.TryGetValue(g.Key, out var c);
                return new CrewHours(g.Key, c?.FRC_EMP_NAME ?? g.First().TL_TECH_NAME, c?.FRC_FA_CODE,
                    g.Where(l => l.TL_SEGMENT_TYPE != SegmentTypes.Travel).Sum(l => l.TL_NET_HOURS),
                    g.Where(l => l.TL_SEGMENT_TYPE == SegmentTypes.Travel).Sum(l => l.TL_NET_HOURS)
                        + g.Where(l => l.TL_SEGMENT_TYPE != SegmentTypes.Travel).Sum(l => l.TL_TRAVEL_HOURS),
                    g.Sum(l => l.TL_IDLE_HOURS), g.Sum(l => l.TL_NORMAL_HOURS), g.Sum(l => l.TL_OT_HOURS),
                    g.Sum(l => l.TL_LABOUR_COST), g.Sum(l => l.TL_FOOD_ALLOWANCE));
            })
            .OrderBy(h => order.TryGetValue(h.Code, out var i) ? i : int.MaxValue)
            .ToList();
        return Page();
    }
}
