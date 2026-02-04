using GatewayAPI.DTOs.Interviews;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/interviews")]
public class InterviewsController : ControllerBase
{
    private readonly InterviewClient _client;

    public InterviewsController(InterviewClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInterviewRequest request)
    {
        var response = await _client.CreateAsync(request);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<InterviewResponse>();

        return Created(string.Empty, body);
    }

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetByCandidate(int candidateId)
    {
        var response = await _client.GetByCandidateAsync(candidateId);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<List<InterviewResponse>>();

        return Ok(body);
    }
}
