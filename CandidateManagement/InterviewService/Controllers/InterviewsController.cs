using InterviewService.Data;
using InterviewService.DTOs;
using InterviewService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewService.Controllers;

[ApiController]
[Route("api/interviews")]
public class InterviewsController : ControllerBase
{
    private readonly InterviewDbContext _context;

    public InterviewsController(InterviewDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInterviewRequest request)
    {
        var interview = new Interview
        {
            CandidateId = request.CandidateId,
            InterviewDate = request.InterviewDate,
            InterviewType = request.InterviewType,
            Interviewer = request.Interviewer
        };

        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = interview.Id }, interview);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var interview = await _context.Interviews.FindAsync(id);
        if (interview == null)
            return NotFound();

        return Ok(interview);
    }

    [HttpGet("candidate/{candidateId}")]
    public async Task<IActionResult> GetByCandidate(int candidateId)
    {
        var interviews = await _context.Interviews
            .Where(i => i.CandidateId == candidateId)
            .ToListAsync();

        return Ok(interviews);
    }
}
