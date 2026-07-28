using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IInternshipService
{
    // Public listing/details - only ever returns Open posts (Phase 8).
    Task<List<InternshipListDto>> GetAllAsync();
    Task<InternshipDetailsDto?> GetByIdAsync(int id);

    // A company's own listing - every status, not just Open (added this phase, since
    // the public listing above no longer shows a company its own drafts).
    Task<List<InternshipListDto>> GetByCompanyUserIdAsync(int userId);

    Task<InternshipDetailsDto> CreateAsync(CreateInternshipDto dto, int userId);
    Task<OperationResult> UpdateAsync(int id, UpdateInternshipDto dto, int userId);
    Task<OperationResult> DeleteAsync(int id, int userId);

    Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)> OpenAsync(int id, int userId);
    Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)> CloseAsync(int id, int userId);
}
