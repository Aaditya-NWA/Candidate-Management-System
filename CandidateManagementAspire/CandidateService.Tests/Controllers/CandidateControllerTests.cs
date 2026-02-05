using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using CandidateService.Controllers;
using CandidateService.Data;
using CandidateService.DTOs;
using CandidateService.Models;
using CandidateService.Services;
using System.Text;
using System.Text.Json;

namespace CandidateService.Tests.Controllers;
                                                                                                                                            
[TestFixture]
public class CandidateControllerTests
{
    private CandidateDbContext _dbContext;
    private Mock<ICandidateBulkInsertService> _serviceMock;
    private CandidateController _controller;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CandidateDbContext(options);

        // mock the interface, not the concrete class, so Moq can setup the async methods
        _serviceMock = new Mock<ICandidateBulkInsertService>();

        _controller = new CandidateController(
            _dbContext,
            _serviceMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    // ---------- CREATE (single) ----------

    [Test]
    public async Task Create_ValidCandidate_ReturnsOk()
    {
        var request = new CreateCandidateRequest
        {
            Name = "John Doe",
            MailId = "john@test.com",
            SkillSet = "C#",
            ExperienceMonths = 36,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P3"
        };

        _serviceMock
            .Setup(s => s.InsertSingleAsync(It.IsAny<Candidate>()))
            .ReturnsAsync(true);

        var result = await _controller.Create(request);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Create_DuplicateCandidate_ReturnsConflict()
    {
        var request = new CreateCandidateRequest
        {
            Name = "John Doe",
            MailId = "john@test.com",
            SkillSet = "C#",
            ExperienceMonths = 36,
            AvailabilityDate = DateTime.Today,
            PrimarySkillLevel = "P3"
        };

        _serviceMock
            .Setup(s => s.InsertSingleAsync(It.IsAny<Candidate>()))
            .ReturnsAsync(false);

        var result = await _controller.Create(request);

        Assert.That(result, Is.InstanceOf<ConflictObjectResult>());
    }

    // ---------- BULK CREATE ----------

    [Test]
    public async Task BulkCreate_ValidFile_ReturnsOk()
    {
        var dtoList = new List<CreateCandidateRequest>
        {
            new()
            {
                Name = "A",
                MailId = "a@test.com",
                SkillSet = "C#",
                ExperienceMonths = 12,
                AvailabilityDate = DateTime.Today,
                PrimarySkillLevel = "P2"
            }
        };

        var json = JsonSerializer.Serialize(dtoList);
        var bytes = Encoding.UTF8.GetBytes(json);

        var file = new FormFile(
            new MemoryStream(bytes),
            0,
            bytes.Length,
            "file",
            "candidates.json"
        );

        var form = new BulkCandidateUploadForm
        {
            File = file
        };

        _serviceMock
            .Setup(s => s.BulkInsertAsync(It.IsAny<List<Candidate>>()))
            .ReturnsAsync((inserted: 1, skipped: 0));

        var result = await _controller.BulkCreate(form);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task BulkCreate_NoFile_ReturnsBadRequest()
    {
        var form = new BulkCandidateUploadForm
        {
            File = null
        };

        var result = await _controller.BulkCreate(form);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
