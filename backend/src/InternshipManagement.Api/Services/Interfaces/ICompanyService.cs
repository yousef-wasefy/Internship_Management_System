using InternshipManagement.Api.DTOs.Companies;

namespace InternshipManagement.Api.Services.Interfaces;

// Company self-service only. Admin actions on a company (approve/reject) live in
// IAdminService (Phase 11) - a company can't approve itself.
public interface ICompanyService
{
    Task<CompanyProfileDto?> GetMyProfileAsync(int userId);
    Task<bool> UpdateMyProfileAsync(int userId, UpdateCompanyProfileDto dto);
}
