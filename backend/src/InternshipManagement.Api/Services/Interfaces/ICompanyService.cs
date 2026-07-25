using InternshipManagement.Api.DTOs.Companies;

namespace InternshipManagement.Api.Services.Interfaces;

public interface ICompanyService
{
    Task<CompanyProfileDto?> GetMyProfileAsync(int userId);
    Task<bool> UpdateMyProfileAsync(int userId, UpdateCompanyProfileDto dto);

    // Admin-only action (Phase 7 stub - full admin module in Phase 11). Looked up by the
    // CompanyProfile's own id, since an admin is acting on "a company" from a list, not
    // "themselves".
    Task<CompanyProfileDto?> ApproveAsync(int companyProfileId);
}
