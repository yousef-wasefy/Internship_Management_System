using System.ComponentModel.DataAnnotations;

namespace InternshipManagement.Api.DTOs.Auth;

public class RegisterCompanyDto
{
    [Required, EmailAddress, StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;
}
