using GatewayAPI.DTOs.Requirements;
using GatewayAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GatewayAPI.Controllers;

[ApiController]
[Route("api/requirements")]
public class RequirementsController : ControllerBase
{
    private readonly RequirementClient _client;

    public RequirementsController(RequirementClient client)
    {
        _client = client;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequirementRequest request)
    {
        var response = await _client.CreateAsync(request);

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<RequirementResponse>();

        return Created(string.Empty, body);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _client.GetAllAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<List<RequirementResponse>>();

        return Ok(body);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _client.GetByIdAsync(id);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var body = await response.Content
            .ReadFromJsonAsync<RequirementResponse>();

        return Ok(body);
    }
}
