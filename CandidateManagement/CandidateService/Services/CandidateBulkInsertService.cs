using CandidateService.Data;
using CandidateService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CandidateService.Services
{
    public class CandidateBulkInsertService
    {
        private readonly CandidateDbContext _context;

        public CandidateBulkInsertService(CandidateDbContext context)
        {
            _context = context;
        }

        // -------- SHARED KEY BUILDER (ONE SOURCE OF TRUTH) --------
        private static string BuildKey(string mailId, string skillSet, DateTime availabilityDate)
        {
            return $"{mailId.ToLower()}|{skillSet.ToLower()}|{availabilityDate.Date:yyyy-MM-dd}";
        }

        // -------- SHARED DUPLICATE CHECK (USED BY SINGLE + BULK) --------
        public async Task<HashSet<string>> GetExistingKeysAsync(IEnumerable<Candidate> candidates)
        {
            // Build incoming keys (IN MEMORY – SAFE)
            var incomingKeys = candidates
                .Select(c => BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate))
                .ToHashSet();

            // Pull only required columns from DB (NO string ops in SQL)
            var dbCandidates = await _context.Candidates
                .Select(c => new
                {
                    c.MailId,
                    c.SkillSet,
                    c.AvailabilityDate
                })
                .ToListAsync();

            // Build DB keys in memory
            var existingKeys = dbCandidates
                .Select(c => BuildKey(c.MailId, c.SkillSet, c.AvailabilityDate))
                .Where(k => incomingKeys.Contains(k))
                .ToHashSet();

            return existingKeys;
        }

        // -------- SINGLE INSERT (USES SAME DUPLICATION LOGIC) --------
        public async Task<bool> InsertSingleAsync(Candidate candidate)
        {
            var existingKeys = await GetExistingKeysAsync(new[] { candidate });

            if (existingKeys.Any())
                return false; // duplicate

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();
            return true;
        }

        // -------- BULK INSERT (USES SAME DUPLICATION LOGIC) --------
        public async Task<(int inserted, int skipped)> BulkInsertAsync(List<Candidate> candidates)
        {
            var existingKeys = await GetExistingKeysAsync(candidates);

            var newCandidates = candidates
                .Where(c => !existingKeys.Contains(
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

            var connection = (SqlConnection)_context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "Candidates",
                BulkCopyTimeout = 60
            };

            // EXPLICIT COLUMN MAPPING (MANDATORY)
            bulkCopy.ColumnMappings.Add("Name", "Name");
            bulkCopy.ColumnMappings.Add("MailId", "MailId");
            bulkCopy.ColumnMappings.Add("SkillSet", "SkillSet");
            bulkCopy.ColumnMappings.Add("ExperienceMonths", "ExperienceMonths");
            bulkCopy.ColumnMappings.Add("AvailabilityDate", "AvailabilityDate");
            bulkCopy.ColumnMappings.Add("PrimarySkillLevel", "PrimarySkillLevel");

            await bulkCopy.WriteToServerAsync(table);

            return (newCandidates.Count, candidates.Count - newCandidates.Count);
        }
    }
}
