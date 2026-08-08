using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Applications;

// Shape returned by apply, GET /api/applications/my, and withdraw - one shape covers
// all three since there's no separate "details" endpoint yet.
public class ApplicationDto
{
    public int Id { get; set; }
    public int InternshipPostId { get; set; }
    public string InternshipTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public string? CVUrl { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? CompanyNotes { get; set; }
}
