using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Shape of the JSON body used to edit an existing internship post's descriptive fields.
// Status changes (Draft/Open/Closed/Cancelled) are handled separately in Phase 8, via
// dedicated open/close endpoints rather than this general-purpose update.
public class UpdateInternshipDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public string? Responsibilities { get; set; }
    public string? Location { get; set; }
    public WorkMode WorkMode { get; set; }
    public string? Duration { get; set; }
    public DateTime ApplicationDeadline { get; set; }
}
