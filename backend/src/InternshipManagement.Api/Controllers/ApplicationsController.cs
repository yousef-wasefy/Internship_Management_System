using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Roles = "Student")]
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
}
