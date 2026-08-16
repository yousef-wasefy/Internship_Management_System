using System.ComponentModel.DataAnnotations;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Shape of the JSON body used to edit an existing internship post's descriptive fields.
// Status changes (Draft/Open/Closed/Cancelled) are handled separately in Phase 8, via
// dedicated open/close endpoints rather than this general-purpose update.
public class UpdateInternshipDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(4000)]
    public string? Requirements { get; set; }

    [StringLength(4000)]
    public string? Responsibilities { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public WorkMode WorkMode { get; set; }

    [StringLength(100)]
    public string? Duration { get; set; }

    public DateTime ApplicationDeadline { get; set; }
}
