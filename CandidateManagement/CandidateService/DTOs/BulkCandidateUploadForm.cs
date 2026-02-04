using Microsoft.AspNetCore.Http;

namespace CandidateService.DTOs
{
    public class BulkCandidateUploadForm
    {
        public IFormFile File { get; set; } = null!;
    }
}
