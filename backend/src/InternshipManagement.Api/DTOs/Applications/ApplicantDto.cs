using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Applications;

// The company-facing view of an application - shows who the student is, not just the
// application itself. Used by GET /api/companies/me/applications and
// GET /api/internships/{id}/applications, and returned after a status update.
public class ApplicantDto
{
    public int Id { get; set; }
    public int InternshipPostId { get; set; }
    public string InternshipTitle { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentUniversity { get; set; }
    public string? StudentMajor { get; set; }
    public string? StudentSkills { get; set; }
    public string? StudentLinkedInUrl { get; set; }
    public string? StudentGitHubUrl { get; set; }
    public string? CoverLetter { get; set; }
    public string? CVUrl { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? CompanyNotes { get; set; }
}
