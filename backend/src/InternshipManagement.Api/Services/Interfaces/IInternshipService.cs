using InternshipManagement.Api.DTOs.Common;
using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IInternshipService
{
    // Public listing/details - only ever returns Open posts (Phase 8). Paginated,
    // filterable (location/workMode), and searchable (title) since Phase 12.
    Task<PagedResult<InternshipListDto>> GetAllAsync(InternshipQueryParameters query);
    Task<InternshipDetailsDto?> GetByIdAsync(int id);

    // A company's own listing - every status, not just Open (added Phase 8, since the
    // public listing above no longer shows a company its own drafts). Optional status
    // filter added Phase 12 (safe here, unlike on the public listing, since the caller
    // already sees every status regardless).
    Task<List<InternshipListDto>> GetByCompanyUserIdAsync(int userId, InternshipStatus? status);

    Task<InternshipDetailsDto> CreateAsync(CreateInternshipDto dto, int userId);
    Task<OperationResult> UpdateAsync(int id, UpdateInternshipDto dto, int userId);
    Task<OperationResult> DeleteAsync(int id, int userId);

    Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)> OpenAsync(int id, int userId);
    Task<(OperationResult Result, string? ErrorMessage, InternshipDetailsDto? Internship)> CloseAsync(int id, int userId);
}
