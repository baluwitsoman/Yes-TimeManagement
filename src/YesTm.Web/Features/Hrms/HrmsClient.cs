using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace YesTm.Web.Features.Hrms;

/// <summary>Typed HttpClient for the HRMS TmSync feed (GET {base}/{short}/api/TmSync/...).</summary>
public sealed class HrmsClient
{
    private readonly HttpClient _http;
    private readonly HrmsOptions _opt;

    public HrmsClient(HttpClient http, IOptions<HrmsOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
        _http.Timeout = TimeSpan.FromSeconds(Math.Max(10, _opt.TimeoutSeconds));
    }

    public Task<List<HrmsEmployeeDto>> GetEmployeesAsync(CancellationToken ct) =>
        GetAsync<HrmsEmployeeDto>("Employees", ct);

    public Task<List<HrmsAttendanceDto>> GetAttendanceAsync(DateTime from, DateTime to, CancellationToken ct) =>
        GetAsync<HrmsAttendanceDto>($"Attendance?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}", ct);

    private async Task<List<T>> GetAsync<T>(string action, CancellationToken ct)
    {
        if (!_opt.IsConfigured)
            throw new InvalidOperationException("HRMS sync is not configured (TimeManagement:Hrms BaseUrl / CompanyShortCode / ApiKey).");

        var url = $"{_opt.BaseUrl!.TrimEnd('/')}/{Uri.EscapeDataString(_opt.CompanyShortCode!)}/api/TmSync/{action}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("X-Api-Key", _opt.ApiKey);
        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"HRMS {action.Split('?')[0]} returned {(int)res.StatusCode} {res.ReasonPhrase}: {Truncate(body, 300)}");
        }
        return await res.Content.ReadFromJsonAsync<List<T>>(ct) ?? [];
    }

    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n] + "…";
}
