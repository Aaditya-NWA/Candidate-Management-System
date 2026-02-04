using Microsoft.EntityFrameworkCore;
using CandidateService.Models;

namespace CandidateService.Data
{
    public class CandidateDbContext : DbContext
    {
        public CandidateDbContext(DbContextOptions<CandidateDbContext> options)
            : base(options)
        {
        }

        public DbSet<Candidate> Candidates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Candidate>()
                .HasIndex(c => new { c.MailId, c.SkillSet, c.AvailabilityDate })
                .IsUnique();
        }
    }
}
