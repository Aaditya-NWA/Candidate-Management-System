namespace GatewayAPI.DTOs.Requirements;

public class RequirementResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SkillSet { get; set; } = string.Empty;
    public int ExperienceMonths { get; set; }
    public int OpenPositions { get; set; }
    public string Status { get; set; } = string.Empty;
}
