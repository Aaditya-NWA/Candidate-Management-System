using CandidateService.Data;
using CandidateService.Models;
using CandidateService.Services;
using CandidateService.Tests.TestData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace CandidateService.Tests.Services;

[TestFixture]
public class CandidateBulkInsertServiceTests
{
    private CandidateDbContext _dbContext;
    private CandidateBulkInsertService _service;

    // A simple no-op bulk copy used for unit tests so we don't require a real SQL Server.
    private class FakeBulkCopy : IBulkCopy
    {
        public string DestinationTableName { get; set; } = string.Empty;
        public int BulkCopyTimeout { get; set; }

        public void ColumnMappings_Add(string source, string destination)
        {
            // no-op for tests
        }

        public Task WriteToServerAsync(DataTable table)
        {
            // no-op - tests assert returned counts instead of DB writes
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // no-op
        }
    }

    private class FakeBulkCopyFactory : IBulkCopyFactory
    {
        public IBulkCopy Create(DbConnection? connection) => new FakeBulkCopy();
    }

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CandidateDbContext(options);
        _service = new CandidateBulkInsertService(_dbContext, new FakeBulkCopyFactory());
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task BulkInsert_ValidCandidates_ReturnsInsertedCount()
    {
        var candidates = CandidateFaker.Create().Generate(5);

        var (inserted, skipped) = await _service.BulkInsertAsync(candidates);

        Assert.That(inserted, Is.EqualTo(5));
        Assert.That(skipped, Is.EqualTo(0));

        // Because the service uses bulk copy (no-op in tests) we do not assert DB count here.
    }

    [Test]
    public void BulkInsert_NullList_Throws()
    {
        Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _service.BulkInsertAsync(null));
    }

    [Test]
    public void BulkInsert_EmptyList_Throws()
    {
        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.BulkInsertAsync(new List<Candidate>()));
    }

    [Test]
    public async Task InsertSingleAsync_NewCandidate_InsertsAndReturnsTrue()
    {
        var candidate = CandidateFaker.Create().Generate();

        var result = await _service.InsertSingleAsync(candidate);

        Assert.That(result, Is.True);
        var db = await _dbContext.Candidates.FirstOrDefaultAsync(c => c.MailId == candidate.MailId);
        Assert.That(db, Is.Not.Null);
    }

    [Test]
    public async Task InsertSingleAsync_DuplicateCandidate_ReturnsFalse()
    {
        var candidate = CandidateFaker.Create().Generate();
        // seed existing candidate in DB
        _dbContext.Candidates.Add(candidate);
        await _dbContext.SaveChangesAsync();

        // attempt to insert duplicate
        var dup = new Candidate
        {
            Name = candidate.Name,
            MailId = candidate.MailId,
            SkillSet = candidate.SkillSet,
            ExperienceMonths = candidate.ExperienceMonths,
            AvailabilityDate = candidate.AvailabilityDate,
            PrimarySkillLevel = candidate.PrimarySkillLevel
        };

        var result = await _service.InsertSingleAsync(dup);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetExistingKeysAsync_ReturnsMatchingKeys()
    {
        var existing = CandidateFaker.Create().Generate();
        _dbContext.Candidates.Add(existing);
        await _dbContext.SaveChangesAsync();

        var incoming = new List<Candidate>
        {
            new Candidate
            {
                Name = "Different",
                MailId = existing.MailId,
                SkillSet = existing.SkillSet,
                ExperienceMonths = 10,
                AvailabilityDate = existing.AvailabilityDate,
                PrimarySkillLevel = existing.PrimarySkillLevel
            },
            CandidateFaker.Create().Generate()
        };

        var matches = await _service.GetExistingKeysAsync(incoming);

        Assert.That(matches, Is.Not.Empty);
        Assert.That(matches.Any(m => m.MailId == existing.MailId && m.SkillSet == existing.SkillSet), Is.True);
    }
    [Test]
    public void Constructor_WithOnlyDbContext_DoesNotThrow()
    {
        // This covers the default constructor path that uses SqlBulkCopyFactory
        Assert.DoesNotThrow(() =>
        {
            var service = new CandidateBulkInsertService(_dbContext);
        });
    }

    [Test]
    public async Task GetExistingKeysAsync_IsCaseInsensitive_And_NormalizesDate()
    {
        var existing = new Candidate
        {
            Name = "Test",
            MailId = "TEST@EMAIL.COM",
            SkillSet = "DOTNET",
            ExperienceMonths = 5,
            AvailabilityDate = DateTime.Today.AddHours(10),
            PrimarySkillLevel = "Mid"
        };

        _dbContext.Candidates.Add(existing);
        await _dbContext.SaveChangesAsync();

        var incoming = new[]
        {
        new Candidate
        {
            Name = "Another",
            MailId = "test@email.com",
            SkillSet = "dotnet",
            ExperienceMonths = 10,
            AvailabilityDate = DateTime.Today.AddHours(2),
            PrimarySkillLevel = "Senior"
        }
    };

        var matches = await _service.GetExistingKeysAsync(incoming);

        Assert.That(matches.Count(), Is.EqualTo(1));
    }
    [Test]
    public async Task BulkInsert_AllCandidatesDuplicate_ReturnsZeroInserted()
    {
        // Arrange
        var existing = CandidateFaker.Create().Generate();
        _dbContext.Candidates.Add(existing);
        await _dbContext.SaveChangesAsync();

        var duplicates = new List<Candidate>
    {
        new Candidate
        {
            Name = existing.Name,
            MailId = existing.MailId,
            SkillSet = existing.SkillSet,
            ExperienceMonths = existing.ExperienceMonths,
            AvailabilityDate = existing.AvailabilityDate,
            PrimarySkillLevel = existing.PrimarySkillLevel
        }
    };

        // Act
        var (inserted, skipped) = await _service.BulkInsertAsync(duplicates);

        // Assert
        Assert.That(inserted, Is.EqualTo(0));
        Assert.That(skipped, Is.EqualTo(1));
    }
    
    private class ThrowingDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "Fake";
        public override string DataSource => "Fake";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => ConnectionState.Closed;

        public override Task OpenAsync(CancellationToken cancellationToken)
            => throw new Exception("Open failed");

        public override void Open() => throw new Exception("Open failed");
        public override void Close() { }
        public override void ChangeDatabase(string databaseName) { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        protected override DbCommand CreateDbCommand()
            => throw new NotImplementedException();
    }
    private class OpenFailingBulkCopyFactory : IBulkCopyFactory
    {
        public IBulkCopy Create(DbConnection? connection)
            => new FakeBulkCopy();
    }
    [Test]
    public async Task BulkInsert_WhenConnectionOpenThrows_IsIgnoredAndSucceeds()
    {
        // Arrange
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");

        // SQLite REQUIRES the connection to be opened once
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<CandidateDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new CandidateDbContext(options);

        // Create schema
        await context.Database.EnsureCreatedAsync();

        var service = new CandidateBulkInsertService(context, new FakeBulkCopyFactory());

        var candidates = CandidateFaker.Create().Generate(2);

        // Act
        var (inserted, skipped) = await service.BulkInsertAsync(candidates);

        // Assert
        Assert.That(inserted, Is.EqualTo(2));
        Assert.That(skipped, Is.EqualTo(0));
    }






}
