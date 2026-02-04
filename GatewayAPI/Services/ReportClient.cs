using System.Net.Http.Json;
using GatewayAPI.DTOs.Reports;

namespace GatewayAPI.Services;

public class ReportClient
{
    private readonly HttpClient _http;

    public ReportClient(HttpClient http)
    {
        _http = http;
    }

    public Task<ReportSummaryResponse?> GetSummaryAsync()
    {
        return _http.GetFromJsonAsync<ReportSummaryResponse>(
            "/api/reports/summary"
        );
    }
}
