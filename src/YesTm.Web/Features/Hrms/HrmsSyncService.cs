using System.Globalization;
using Microsoft.Extensions.Options;

namespace YesTm.Web.Features.Hrms;

public sealed record SyncResult(bool Ok, int Employees, int AttendanceRows, int NewMatches, string Message);

/// <summary>
/// Pulls employees + daily attendance (V_ATT_TRAN) for company 39 from HRMS into TM_HRMS_EMPLOYEE /
/// TM_ATTENDANCE, then refreshes the TM → HRMS code suggestions. Every run is logged in TM_SYNC_LOG.
/// One run at a time (manual and nightly share the lock).
/// </summary>
public sealed class HrmsSyncService
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly HrmsClient _client;
    private readonly IHrmsRepository _repo;
    private readonly HrmsOptions _opt;
    private readonly ILogger<HrmsSyncService> _logger;

    public HrmsSyncService(HrmsClient client, IHrmsRepository repo, IOptions<HrmsOptions> opt, ILogger<HrmsSyncService> logger)
    {
        _client = client;
        _repo = repo;
        _opt = opt.Value;
        _logger = logger;
    }

    public bool IsConfigured => _opt.IsConfigured;
    public HrmsOptions Options => _opt;

    public async Task<SyncResult> SyncAsync(DateTime from, DateTime to, string trigger, decimal? userId, CancellationToken ct)
    {
        if (!_opt.IsConfigured)
            return new(false, 0, 0, 0, "HRMS sync is not configured — set TimeManagement:Hrms BaseUrl, CompanyShortCode and ApiKey.");
        if (to < from) (from, to) = (to, from);
        if (!await Gate.WaitAsync(0, ct))
            return new(false, 0, 0, 0, "A sync is already running — try again in a minute.");

        decimal logId = 0;
        try
        {
            logId = await _repo.StartLogAsync(trigger, from, to, userId, ct);

            var employees = await _client.GetEmployeesAsync(ct);
            await _repo.UpsertEmployeesAsync(employees, ct);

            var attendance = await _client.GetAttendanceAsync(from, to, ct);
            var rows = attendance.Where(a => !string.IsNullOrWhiteSpace(a.EMP_CODE))
                .Select(a => ToEntity(a, logId)).ToList();
            var written = await _repo.ReplaceAttendanceAsync(from, to, rows, ct);

            var matches = EmployeeMatcher.Match(await _repo.GetTmEmployeesAsync(ct), employees);
            var newMatches = await _repo.SaveSuggestionsAsync(matches, ct);

            var msg = $"{employees.Count} employees, {written} attendance days ({from:dd-MMM-yyyy} – {to:dd-MMM-yyyy}), {newMatches} mapping suggestion(s) updated.";
            await _repo.FinishLogAsync(logId, "OK", employees.Count, written, newMatches, msg, ct);
            _logger.LogInformation("HRMS sync ({Trigger}) OK: {Message}", trigger, msg);
            return new(true, employees.Count, written, newMatches, msg);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "HRMS sync ({Trigger}) failed", trigger);
            if (logId > 0)
                await _repo.FinishLogAsync(logId, "FAILED", 0, 0, 0, ex.Message, CancellationToken.None);
            return new(false, 0, 0, 0, "Sync failed: " + ex.Message);
        }
        finally
        {
            Gate.Release();
        }
    }

    /// <summary>HRMS employee-day → TM row: day fractions for leave / holiday / absence, hours as numbers.</summary>
    public static TM_ATTENDANCE ToEntity(HrmsAttendanceDto a, decimal syncId)
    {
        var status = (a.DAY_STATUS ?? "OTHER").Trim().ToUpperInvariant();
        static decimal Frac(decimal? v) => v is > 0m and <= 1m ? v.Value : 1m;
        return new TM_ATTENDANCE
        {
            ATT_HRMS_EMP_CODE = a.EMP_CODE!.Trim(),
            ATT_DATE = a.ATT_DATE.Date,
            ATT_DAY_STATUS = status,
            ATT_LEAVE_ID = a.LEAVE_ID,
            ATT_LEAVE_NAME = a.LEAVE_NAME,
            ATT_HOLIDAY_NAME = a.HOL_NAME,
            ATT_LEAVE_DAYS = status == "LEAVE" ? Frac(a.LEAVE_COUNT) : 0m,
            ATT_HOLIDAY_DAYS = status == "HOLIDAY" ? Frac(a.HOLIDAY_COUNT) : 0m,
            ATT_ABSENT_DAYS = status == "ABSENT" ? Frac(a.ABSENT_FACTOR) : 0m,
            ATT_SHIFT_HOURS = ParseHoursMinutes(a.SHIFT_TOT_HOURS),
            ATT_ACTUAL_HOURS = Math.Round((a.ACTUAL_TOT_MINUTES ?? 0m) / 60m, 2),
            ATT_OT_HOURS = a.OT_HOURS ?? 0m,
            ATT_FIRST_IN = a.FIRST_IN,
            ATT_LAST_OUT = a.LAST_OUT,
            ATT_SYNC_ID = syncId
        };
    }

    /// <summary>HRMS stores hours as "H.MM" / "H:MM" (minutes after the separator): "8.30" = 8.5 h.</summary>
    public static decimal ParseHoursMinutes(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0m;
        var parts = s.Trim().Replace(':', '.').Split('.');
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var h)) return 0m;
        var m = parts.Length > 1 && int.TryParse(parts[1].PadRight(2, '0')[..2], out var mm) ? mm : 0;
        return Math.Round(h + m / 60m, 2);
    }
}

/// <summary>Runs the HRMS sync once a night (TimeManagement:Hrms:NightlySyncHour) for the last SyncDaysBack days.</summary>
public sealed class HrmsNightlySync : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly HrmsOptions _opt;
    private readonly ILogger<HrmsNightlySync> _logger;

    public HrmsNightlySync(IServiceScopeFactory scopes, IOptions<HrmsOptions> opt, ILogger<HrmsNightlySync> logger)
    {
        _scopes = scopes;
        _opt = opt.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opt.IsConfigured || _opt.NightlySyncHour is not (>= 0 and <= 23))
        {
            _logger.LogInformation("HRMS nightly sync disabled (not configured or no NightlySyncHour).");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var next = now.Date.AddHours(_opt.NightlySyncHour.Value);
            if (next <= now) next = next.AddDays(1);
            try { await Task.Delay(next - now, stoppingToken); }
            catch (OperationCanceledException) { return; }

            using var scope = _scopes.CreateScope();
            var sync = scope.ServiceProvider.GetRequiredService<HrmsSyncService>();
            var today = DateTime.Today;
            await sync.SyncAsync(today.AddDays(-Math.Max(1, _opt.SyncDaysBack)), today, "SCHEDULED", null, stoppingToken);
        }
    }
}
