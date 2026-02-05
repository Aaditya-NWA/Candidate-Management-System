using CandidateService.Models;

namespace CandidateService.Services
{
    public interface ICandidateBulkInsertService
    {
        Task<bool> InsertSingleAsync(Candidate candidate);

        Task<(int inserted, int skipped)> BulkInsertAsync(List<Candidate> candidates);

        Task<IEnumerable<(string MailId, string SkillSet, DateTime AvailabilityDate)>>
            GetExistingKeysAsync(IEnumerable<Candidate> candidates);
    }
}
    