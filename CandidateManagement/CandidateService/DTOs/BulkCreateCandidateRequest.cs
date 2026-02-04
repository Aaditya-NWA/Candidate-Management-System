namespace CandidateService.DTOs
{
    public class BulkCreateCandidateRequest
    {
        public List<CreateCandidateRequest> Candidates { get; set; } = new();
    }
}
