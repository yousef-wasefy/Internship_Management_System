using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Admin;

// Shape returned by GET /api/admin/users and the disable-user action.
public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    // The student's full name, the company's name, or null for an Admin account -
    // resolved from whichever profile (if any) belongs to this user.
    public string? DisplayName { get; set; }

    public UserRole Role { get; set; }
    public bool IsDisabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
