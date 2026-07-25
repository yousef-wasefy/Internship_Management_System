namespace InternshipManagement.Api.DTOs.Companies;

// Shape returned by GET /api/companies/me and by the admin approve endpoint.
public class CompanyProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
