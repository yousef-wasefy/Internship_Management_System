using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IApplicationService
{
    Task<List<ApplicationDto>> GetMyApplicationsAsync(int userId);

    Task<(OperationResult Result, string? ErrorMessage, ApplicationDto? Application)> ApplyAsync(
        int internshipPostId, int userId, ApplyToInternshipDto dto);

    Task<(OperationResult Result, string? ErrorMessage, ApplicationDto? Application)> WithdrawAsync(
        int applicationId, int userId);
}
