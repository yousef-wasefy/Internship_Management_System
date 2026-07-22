using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Shape returned by GET /api/internships/{id} - full details for a single post.
public class InternshipDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }
    public string? Duration { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public InternshipStatus Status { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
