using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Auth;

// Returned by GET /api/auth/me.
public class CurrentUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
