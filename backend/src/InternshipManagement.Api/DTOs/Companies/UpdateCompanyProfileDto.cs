using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.DTOs.Companies;

// Shape of the JSON body for PUT /api/companies/me - deliberately no IsApproved here;
// only an admin can change that (Phase 7's admin approve endpoint / Phase 11).
public class UpdateCompanyProfileDto
{
    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Industry { get; set; }

    [Url, StringLength(500)]
    public string? WebsiteUrl { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }
}
