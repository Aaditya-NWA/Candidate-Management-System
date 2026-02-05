using Bogus;
using CandidateService.Models;

namespace CandidateService.Tests.TestData;

public static class CandidateFaker
{
    public static Faker<Candidate> Create()
    {
        return new Faker<Candidate>()
            .RuleFor(x => x.Name, f => f.Name.FullName())
            .RuleFor(x => x.MailId, f => f.Internet.Email())
            .RuleFor(x => x.SkillSet, f => f.PickRandom(
                "C#", ".NET", "ASP.NET Core", "SQL", "Azure"))
            .RuleFor(x => x.ExperienceMonths, f => f.Random.Int(0, 600))
            .RuleFor(x => x.AvailabilityDate, f => f.Date.Future())
            .RuleFor(x => x.PrimarySkillLevel, f => f.PickRandom(
                "P0", "P1", "P2", "P3", "P4", "P5"));
    }
}
