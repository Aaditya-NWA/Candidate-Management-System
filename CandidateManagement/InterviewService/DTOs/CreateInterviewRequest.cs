using System.ComponentModel.DataAnnotations;

namespace InterviewService.DTOs;

public class CreateInterviewRequest
{
    [Required]
    public int CandidateId { get; set; }

    [Required]
    public DateTime InterviewDate { get; set; }

    [Required]
    public string InterviewType { get; set; } = string.Empty;

    [Required]
    public string Interviewer { get; set; } = string.Empty;
}
