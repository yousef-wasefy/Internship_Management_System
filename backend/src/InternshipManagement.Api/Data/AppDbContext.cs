using InternshipManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<InternshipPost> InternshipPosts => Set<InternshipPost>();
    public DbSet<InternshipApplication> InternshipApplications => Set<InternshipApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One account per email address - needed for login to be unambiguous.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // A unique FK is what turns a normal FK relationship into a 1-to-1: each user can
        // be linked from at most one StudentProfile / CompanyProfile row.
        modelBuilder.Entity<StudentProfile>()
            .HasIndex(s => s.UserId)
            .IsUnique();

        modelBuilder.Entity<CompanyProfile>()
            .HasIndex(c => c.UserId)
            .IsUnique();

        // The single most important constraint in this schema: a student can never apply
        // twice to the same internship (REQUIREMENTS.md §4.1 rule 4). Enforcing it here,
        // at the database level, means even a bug in the service layer can't let a
        // duplicate through - see DATABASE_DESIGN.md §6 for the full explanation.
        modelBuilder.Entity<InternshipApplication>()
            .HasIndex(a => new { a.StudentId, a.InternshipPostId })
            .IsUnique();
    }
}
