namespace InternshipManagement.Api.DTOs.Companies;

// Shape of the JSON body for PUT /api/companies/me - deliberately no IsApproved here;
// only an admin can change that (Phase 7's admin approve endpoint / Phase 11).
public class UpdateCompanyProfileDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
}
