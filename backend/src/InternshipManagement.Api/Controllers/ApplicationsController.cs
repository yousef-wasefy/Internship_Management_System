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
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var applications = await _applicationService.GetMyApplicationsAsync(userId.Value);
        return Ok(applications);
    }

    /// <summary>Withdraws the logged-in student's own application. Owner only, and only while Pending.</summary>
    [HttpPatch("{id:int}/withdraw")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApplicationDto>> Withdraw(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, error, application) = await _applicationService.WithdrawAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Application not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "This is not your application."),
            OperationResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest, detail: error),
            _ => Ok(application)
        };
    }

    /// <summary>
    /// Lets the owning company shortlist, accept, or reject an application. Cannot be used
    /// on a Withdrawn application, regardless of the requested status.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<ApplicantDto>> UpdateStatus(int id, UpdateApplicationStatusDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, error, applicant) = await _applicationService.UpdateStatusAsync(id, userId.Value, dto);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Application not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own the internship this application is for."),
            OperationResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest, detail: error),
            _ => Ok(applicant)
        };
    }
}
