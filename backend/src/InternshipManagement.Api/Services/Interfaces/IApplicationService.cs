using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.Enums;

namespace InternshipManagement.Api.Services.Interfaces;

public interface IApplicationService
{
    // Student-facing
    Task<List<ApplicationDto>> GetMyApplicationsAsync(int userId);

    Task<(OperationResult Result, string? ErrorMessage, ApplicationDto? Application)> ApplyAsync(
        int internshipPostId, int userId, ApplyToInternshipDto dto);

    Task<(OperationResult Result, string? ErrorMessage, ApplicationDto? Application)> WithdrawAsync(
        int applicationId, int userId);

    // Company-facing (Phase 10)
    Task<List<ApplicantDto>> GetApplicantsForCompanyAsync(int userId);

    Task<(OperationResult Result, string? ErrorMessage, List<ApplicantDto>? Applicants)> GetApplicantsForInternshipAsync(
        int internshipPostId, int userId);

    Task<(OperationResult Result, string? ErrorMessage, ApplicantDto? Applicant)> UpdateStatusAsync(
        int applicationId, int userId, UpdateApplicationStatusDto dto);
}
