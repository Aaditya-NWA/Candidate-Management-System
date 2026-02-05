using CandidateService.Data;
using CandidateService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace CandidateService.Services
{
    public class CandidateBulkInsertService : ICandidateBulkInsertService
    {
        private readonly CandidateDbContext _context;
        private readonly IBulkCopyFactory _bulkCopyFactory;

        public CandidateBulkInsertService(CandidateDbContext context)
            : this(context, new SqlBulkCopyFactory())
        {
        }

        // Test-friendly constructor
        public CandidateBulkInsertService(CandidateDbContext context, IBulkCopyFactory bulkCopyFactory)
        {
            _context = context;
            _bulkCopyFactory = bulkCopyFactory;
        }

        // -------- SHARED KEY BUILDER (ONE SOURCE OF TRUTH) --------
        private static string BuildKey(string mailId, string skillSet, DateTime availabilityDate)
        {
            return $"{mailId.ToLower()}|{skillSet.ToLower()}|{availabilityDate.Date:yyyy-MM-dd}";
        }

        // -------- SHARED DUPLICATE CHECK (USED BY SINGLE + BULK) --------
        public async Task<IEnumerable<(string MailId, string SkillSet, DateTime AvailabilityDate)>>

            GetExistingKeysAsync(IEnumerable<Candidate> candidates)
        {
            // Build incoming keys (IN MEMORY – SAFE)
            var incomingKeys = candidates
                .Select(c => BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate))
                .ToHashSet();

            // Pull only required columns from DB
            var dbCandidates = await _context.Candidates
                .Select(c => new
                {
                    c.MailId,
                    c.SkillSet,
                    c.AvailabilityDate
                })
                .ToListAsync();

            // Match against incoming keys
            var matched = dbCandidates
                .Where(c => incomingKeys.Contains(
                    BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate)))
                .Select(c => (
                    c.MailId,
                    c.SkillSet,
                    c.AvailabilityDate.Date
                ))
                .ToList();

            return matched;
        }

        // -------- SINGLE INSERT --------
        public async Task<bool> InsertSingleAsync(Candidate candidate)
        {
            var existingKeys = await GetExistingKeysAsync(new[] { candidate });
                    
            if (existingKeys.Any())
                return false; // duplicate

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            return true;
        }

        // -------- BULK INSERT --------
        public async Task<(int inserted, int skipped)> BulkInsertAsync(List<Candidate> candidates)
        {
            if (candidates == null)
                throw new ArgumentNullException(nameof(candidates));

            if (!candidates.Any())
                throw new ArgumentException("Candidates list must not be empty.", nameof(candidates));

            var existingKeys = await GetExistingKeysAsync(candidates);

            // Convert tuple keys back to string keys for fast lookup
            var existingKeySet = existingKeys
                .Select(k => BuildKey(k.MailId, k.SkillSet, k.AvailabilityDate))
                .ToHashSet();

            var newCandidates = candidates
                .Where(c => !existingKeySet.Contains(
                    BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate)))
                .ToList();

            if (!newCandidates.Any())
                return (0, candidates.Count);

            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("MailId", typeof(string));
            table.Columns.Add("SkillSet", typeof(string));
            table.Columns.Add("ExperienceMonths", typeof(int));
            table.Columns.Add("AvailabilityDate", typeof(DateTime));
            table.Columns.Add("PrimarySkillLevel", typeof(string));

            foreach (var c in newCandidates)
            {
                table.Rows.Add(
                    c.Name,
                    c.MailId,
                    c.SkillSet,
                    c.ExperienceMonths,
                    c.AvailabilityDate,
                    c.PrimarySkillLevel
                );
            }

            DbConnection? connection = null;
            try
            {
                connection = _context.Database.GetDbConnection();
            }
            catch (InvalidOperationException)
            {
                // Non-relational provider (e.g. InMemory) - allow factory to accept null for test/no-op.
                connection = null;
            }

            if (connection != null)
            {
                try
                {
                    await connection.OpenAsync();
                }
                catch
                {
                    // Ignore open failures for providers that don't require manual open
                }
            }

            using var bulkCopy = _bulkCopyFactory.Create(connection);

            bulkCopy.DestinationTableName = "Candidates";
            bulkCopy.BulkCopyTimeout = 60;

            bulkCopy.ColumnMappings_Add("Name", "Name");
            bulkCopy.ColumnMappings_Add("MailId", "MailId");
            bulkCopy.ColumnMappings_Add("SkillSet", "SkillSet");
            bulkCopy.ColumnMappings_Add("ExperienceMonths", "ExperienceMonths");
            bulkCopy.ColumnMappings_Add("AvailabilityDate", "AvailabilityDate");
            bulkCopy.ColumnMappings_Add("PrimarySkillLevel", "PrimarySkillLevel");

            await bulkCopy.WriteToServerAsync(table);

            return (newCandidates.Count, candidates.Count - newCandidates.Count);
        }
    }
}
