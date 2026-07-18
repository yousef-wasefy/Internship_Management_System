using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Entities;

public class InternshipApplication
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public StudentProfile Student { get; set; } = null!;

    public int InternshipPostId { get; set; }
    public InternshipPost InternshipPost { get; set; } = null!;

    public string? CoverLetter { get; set; }

    // Lets a student attach a different CV than their profile default for this application.
    public string? CVUrl { get; set; }

    // Every application starts Pending (Phase 9 application workflow).
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public DateTime AppliedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Set when a company changes the status away from Pending (Phase 10).
    public DateTime? ReviewedAt { get; set; }
    public string? CompanyNotes { get; set; }

    // NOTE: the (StudentId, InternshipPostId) pair has a composite unique index configured
    // in AppDbContext.OnModelCreating - this is the DB-level enforcement of "a student
    // cannot apply twice to the same internship" (REQUIREMENTS.md §4.1 rule 4).
}
