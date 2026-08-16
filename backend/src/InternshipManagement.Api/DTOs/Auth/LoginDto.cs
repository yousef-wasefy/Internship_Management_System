using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.DTOs.Auth;

public class LoginDto
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;

    // No [MinLength] here on purpose - a login attempt just needs "was a password sent
    // at all," not the registration-time password policy. A too-short guess still fails
    // BCrypt.Verify normally, via the same generic "invalid email or password" message.
    [Required]
    public string Password { get; set; } = string.Empty;
}
