using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Shape of the JSON body a company sends to create a new internship post.
// No CompanyId or Status here - the server assigns those (see InternshipService).
public class CreateInternshipDto
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
