using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RequirementService.Data;
using RequirementService.DTOs;
using RequirementService.Models;

namespace RequirementService.Controllers;

[ApiController]
[Route("api/requirements")]
public class RequirementsController : ControllerBase
{
    private readonly RequirementDbContext _context;

    public RequirementsController(RequirementDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequirementRequest request)
    {
        var requirement = new Requirement
        {
            Title = request.Title,
            SkillSet = request.SkillSet,
            ExperienceMonths = request.ExperienceMonths,
            OpenPositions = request.OpenPositions
        };

        _context.Requirements.Add(requirement);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = requirement.Id }, requirement);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Requirements.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var req = await _context.Requirements.FindAsync(id);
        return req == null ? NotFound() : Ok(req);
    }
}
