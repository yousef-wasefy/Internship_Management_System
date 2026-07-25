namespace InternshipManagement.Api.DTOs.Students;

// Shape of the JSON body for PUT /api/students/me - no Email, Id, or timestamps here;
// those aren't the student's to change through this endpoint.
public class UpdateStudentProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string? University { get; set; }
    public string? Faculty { get; set; }
    public string? Major { get; set; }
    public string? AcademicYear { get; set; }
    public string? Skills { get; set; }
    public string? CVUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
}
