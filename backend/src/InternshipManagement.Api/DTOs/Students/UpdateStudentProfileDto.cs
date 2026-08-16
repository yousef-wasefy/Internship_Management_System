using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.DTOs.Students;

// Shape of the JSON body for PUT /api/students/me - no Email, Id, or timestamps here;
// those aren't the student's to change through this endpoint.
public class UpdateStudentProfileDto
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? University { get; set; }

    [StringLength(200)]
    public string? Faculty { get; set; }

    [StringLength(200)]
    public string? Major { get; set; }

    [StringLength(50)]
    public string? AcademicYear { get; set; }

    [StringLength(500)]
    public string? Skills { get; set; }

    [Url, StringLength(500)]
    public string? CVUrl { get; set; }

    [Url, StringLength(500)]
    public string? LinkedInUrl { get; set; }

    [Url, StringLength(500)]
    public string? GitHubUrl { get; set; }
}
