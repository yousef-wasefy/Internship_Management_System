using InternshipManagement.Api.DTOs.Admin;
using InternshipManagement.Api.DTOs.Companies;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();

    Task<List<CompanyProfileDto>> GetPendingCompaniesAsync();
    Task<CompanyProfileDto?> ApproveCompanyAsync(int companyProfileId);
    Task<CompanyProfileDto?> RejectCompanyAsync(int companyProfileId);

    Task<List<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto?> DisableUserAsync(int userId);
}
