namespace InternshipManagement.Api.Entities;

public class CompanyProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string CompanyName { get; set; } = string.Empty;
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }

    // Companies start unapproved (REQUIREMENTS.md CO-3); an admin flips this in Phase 11.
    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<InternshipPost> InternshipPosts { get; set; } = new List<InternshipPost>();
}
