using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Set by an admin (REQUIREMENTS.md AD-5); disabled users must be blocked from
    // logging in / using protected endpoints (Phase 11).
    public bool IsDisabled { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Only one of these will ever be set, depending on Role - see DATABASE_DESIGN.md D9
    // for why User is kept separate from the role-specific profile tables.
    public StudentProfile? StudentProfile { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }
}
