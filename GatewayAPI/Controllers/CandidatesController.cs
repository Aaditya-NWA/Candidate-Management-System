using GatewayAPI.DTOs.Candidates;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/candidates")]
public class CandidatesController : ControllerBase
{
    private readonly CandidateClient _client;

    public CandidatesController(CandidateClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreateCandidateRequest request)
    {
        var response = await _client.CreateAsync(request);

        if (response.StatusCode == HttpStatusCode.Conflict)
            return Conflict();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<CandidateResponse>();

        return Created(string.Empty, body);
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _client.GetAllAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<List<CandidateResponse>>();

        return Ok(body);
    }
}
    