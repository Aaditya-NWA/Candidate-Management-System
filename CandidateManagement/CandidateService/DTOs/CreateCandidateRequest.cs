using System.ComponentModel.DataAnnotations;

namespace CandidateService.DTOs
{
    public class CreateCandidateRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string MailId { get; set; } = string.Empty;

        [Required]
        public string SkillSet { get; set; } = string.Empty;

        public int ExperienceMonths { get; set; }

        public DateTime AvailabilityDate { get; set; }


        [Required]
        public string PrimarySkillLevel { get; set; } = "P0";
    }
}
