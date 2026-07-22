using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Shape returned by GET /api/internships - a summary for listing, not full details.
public class InternshipListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public InternshipStatus Status { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}
