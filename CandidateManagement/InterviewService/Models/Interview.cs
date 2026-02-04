namespace InterviewService.Models;

public class Interview
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public DateTime InterviewDate { get; set; }

    public string InterviewType { get; set; } = string.Empty;

    public string Interviewer { get; set; } = string.Empty;

    public string Status { get; set; } = "Scheduled";

    public string? Feedback { get; set; }
}
