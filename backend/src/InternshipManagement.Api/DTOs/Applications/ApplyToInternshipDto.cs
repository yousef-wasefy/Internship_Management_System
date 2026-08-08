namespace InternshipManagement.Api.DTOs.Applications;

// Shape of the JSON body for POST /api/internships/{id}/apply. Both fields optional -
// a student can apply with just a click, or attach a cover letter / a CV link that's
// different from their profile's default.
public class ApplyToInternshipDto
{
    public string? CoverLetter { get; set; }
    public string? CVUrl { get; set; }
}
