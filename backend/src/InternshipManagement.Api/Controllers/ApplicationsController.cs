using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

// No controller-level [Authorize] here - unlike StudentsController/CompaniesController,
// this controller mixes Student-only actions (GetMy, Withdraw) with a Company-only
// action (UpdateStatus), so each action declares its own required role.
[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ApplicationsController(IApplicationService applicationService, ICurrentUserAccessor currentUserAccessor)
    {
        _applicationService = applicationService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<List<ApplicationDto>>> GetMy()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var applications = await _applicationService.GetMyApplicationsAsync(userId.Value);
        return Ok(applications);
    }

    [HttpPatch("{id:int}/withdraw")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApplicationDto>> Withdraw(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var (result, error, application) = await _applicationService.WithdrawAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.Forbidden => Forbid(),
            OperationResult.ValidationFailed => BadRequest(new { message = error }),
            _ => Ok(application)
        };
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<ApplicantDto>> UpdateStatus(int id, UpdateApplicationStatusDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var (result, error, applicant) = await _applicationService.UpdateStatusAsync(id, userId.Value, dto);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.Forbidden => Forbid(),
            OperationResult.ValidationFailed => BadRequest(new { message = error }),
            _ => Ok(applicant)
        };
    }
}
