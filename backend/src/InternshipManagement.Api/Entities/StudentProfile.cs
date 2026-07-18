namespace InternshipManagement.Api.Entities;

public class StudentProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string? University { get; set; }
    public string? Faculty { get; set; }
    public string? Major { get; set; }
    public string? AcademicYear { get; set; }

    // Comma-separated free text for v1 - see DATABASE_DESIGN.md D10 for why this isn't
    // a normalized Skill table.
    public string? Skills { get; set; }

    public string? CVUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InternshipApplication> Applications { get; set; } = new List<InternshipApplication>();
}
