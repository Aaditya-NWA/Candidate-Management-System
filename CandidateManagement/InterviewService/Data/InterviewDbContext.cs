using InterviewService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace InterviewService.Data;

public class InterviewDbContext : DbContext
{
    public InterviewDbContext(DbContextOptions<InterviewDbContext> options)
        : base(options) { }

    public DbSet<Interview> Interviews => Set<Interview>();
}
