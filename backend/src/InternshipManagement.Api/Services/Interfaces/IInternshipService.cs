using InternshipManagement.Api.DTOs.Internships;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IInternshipService
{
    Task<List<InternshipListDto>> GetAllAsync();
    Task<InternshipDetailsDto?> GetByIdAsync(int id);
    Task<InternshipDetailsDto> CreateAsync(CreateInternshipDto dto);
    Task<bool> UpdateAsync(int id, UpdateInternshipDto dto);
    Task<bool> DeleteAsync(int id);
}
