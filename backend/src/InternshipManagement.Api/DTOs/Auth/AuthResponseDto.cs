using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.DTOs.Auth;

// Returned by register and login alike - both leave the caller "logged in" with a token.
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
