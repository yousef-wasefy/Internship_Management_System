using System.ComponentModel.DataAnnotations;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Internships;

// Shape of the JSON body a company sends to create a new internship post.
// No CompanyId or Status here - the server assigns those (see InternshipService).
//
// ApplicationDeadline has no [Required]/"must be in the future" attribute here on
// purpose: a Draft can legitimately be created with a deadline still to be decided.
// "The deadline must be in the future" is a *publishing* rule, checked in
// InternshipService.OpenAsync (Phase 8), not a DTO shape rule - those are different
// concerns (can this request even be parsed vs. is this business action allowed).
public class CreateInternshipDto
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
