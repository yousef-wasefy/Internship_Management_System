namespace InternshipManagement.Api.DTOs.Students;

// Shape returned by GET /api/students/me.
public class StudentProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? University { get; set; }
    public string? Faculty { get; set; }
    public string? Major { get; set; }
    public string? AcademicYear { get; set; }
    public string? Skills { get; set; }
    public string? CVUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
