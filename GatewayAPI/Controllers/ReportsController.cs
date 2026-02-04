using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportClient _client;

    public ReportsController(ReportClient client)
    {
        _client = client;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var report = await _client.GetSummaryAsync();

        if (report == null)
            return StatusCode(502, "Report service unavailable");

        return Ok(report);
    }
}
