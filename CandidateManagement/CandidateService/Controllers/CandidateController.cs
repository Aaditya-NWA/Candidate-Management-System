using CandidateService.Data;
using CandidateService.DTOs;
using CandidateService.Models;
using CandidateService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;

namespace CandidateService.Controllers
{
    [ApiController]
    [Route("api/candidates")]
    public class CandidateController : ControllerBase
    {
        private readonly CandidateDbContext _context;
        private readonly ICandidateBulkInsertService _service;

        public CandidateController(
            CandidateDbContext context,
            ICandidateBulkInsertService service)
        {
            _context = context;
            _service = service;
        }


        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CreateCandidateRequest request)
        {
            var candidate = new Candidate
            {
                Name = request.Name,
                MailId = request.MailId,
                SkillSet = request.SkillSet,
                ExperienceMonths = request.ExperienceMonths,
                AvailabilityDate = request.AvailabilityDate,
                PrimarySkillLevel = request.PrimarySkillLevel
            };

            var inserted = await _service.InsertSingleAsync(candidate);

            if (!inserted)
                return Conflict("Duplicate candidate (MailId + SkillSet + AvailabilityDate)");

            return Ok(candidate);
        }

        // -------- BULK CANDIDATES (FILE UPLOAD) --------
        [HttpPost("bulk")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BulkCreate([FromForm] BulkCandidateUploadForm form)
        {
            var file = form.File;

            if (file == null || file.Length == 0)
                return BadRequest("File is missing.");

            List<CreateCandidateRequest>? dtoList;

            try
            {
                using var stream = file.OpenReadStream();
                dtoList = await JsonSerializer.DeserializeAsync<List<CreateCandidateRequest>>(
                    stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (JsonException ex)
            {
                return BadRequest($"Invalid JSON file: {ex.Message}");
            }

            if (dtoList == null || dtoList.Count == 0)
                return BadRequest("No candidates found in file.");

            var candidates = dtoList.Select(c => new Candidate
            {
                Name = c.Name,
                MailId = c.MailId,
                SkillSet = c.SkillSet,
                ExperienceMonths = c.ExperienceMonths,
                AvailabilityDate = c.AvailabilityDate,
                PrimarySkillLevel = c.PrimarySkillLevel
            }).ToList();

            var sw = Stopwatch.StartNew();

            var result = await _service.BulkInsertAsync(candidates);

            sw.Stop();

            return Ok(new
            {
                totalReceived = candidates.Count,
                inserted = result.inserted,
                skipped = result.skipped,
                timeTakenMs = sw.ElapsedMilliseconds,
                reasonForSkip = "Duplicate MailId + SkillSet + AvailabilityDate"
            });
        }


        // READ BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
                return NotFound();

            return Ok(candidate);
        }

        // READ ALL (PAGINATED)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;

            var totalCount = await _context.Candidates.CountAsync();

            var candidates = await _context.Candidates
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = candidates
            });
        }



        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateCandidateRequest request)
        {
            var candidate = await _context.Candidates.FindAsync(id);
            if (candidate == null)
                return NotFound();

            // 🔐 DUPLICATE VALIDATION (ADD HERE)
            var tempCandidate = new Candidate
            {
                MailId = request.MailId,
                SkillSet = request.SkillSet,
                AvailabilityDate = request.AvailabilityDate
            };

            var exists = await _service.GetExistingKeysAsync(new[] { tempCandidate });

            // If duplicate exists AND it is NOT the same record
            if (exists.Any() &&
                (candidate.MailId != request.MailId ||
                 candidate.SkillSet != request.SkillSet ||
                 candidate.AvailabilityDate.Date != request.AvailabilityDate.Date))
            {
                return Conflict("Duplicate candidate (MailId + SkillSet + AvailabilityDate)");
            }

            // ✅ SAFE TO UPDATE
            candidate.Name = request.Name;
            candidate.MailId = request.MailId;
            candidate.SkillSet = request.SkillSet;
            candidate.ExperienceMonths = request.ExperienceMonths;
            candidate.AvailabilityDate = request.AvailabilityDate;
            candidate.PrimarySkillLevel = request.PrimarySkillLevel;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var candidate = await _context.Candidates.FindAsync(id);    
            if (candidate == null)
                return NotFound();

            _context.Candidates.Remove(candidate);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
