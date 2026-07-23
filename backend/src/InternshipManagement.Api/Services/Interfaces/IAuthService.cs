using InternshipManagement.Api.DTOs.Auth;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IAuthService
{
    // Each returns null to signal a business-rule failure (email taken / bad
    // credentials) - the controller maps null to the appropriate HTTP status, same
    // pattern InternshipService already uses for "not found" (Phase 5).
    Task<AuthResponseDto?> RegisterStudentAsync(RegisterStudentDto dto);
    Task<AuthResponseDto?> RegisterCompanyAsync(RegisterCompanyDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<CurrentUserDto?> GetCurrentUserAsync(int userId);
}
