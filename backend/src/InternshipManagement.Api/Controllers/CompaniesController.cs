using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize(Roles = "Company")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IInternshipService _internshipService;
    private readonly IApplicationService _applicationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CompaniesController(
        ICompanyService companyService,
        IInternshipService internshipService,
        IApplicationService applicationService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _companyService = companyService;
        _internshipService = internshipService;
        _applicationService = applicationService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("me")]
    public async Task<ActionResult<CompanyProfileDto>> GetMe()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var profile = await _companyService.GetMyProfileAsync(userId.Value);
        return profile is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Company profile not found.")
            : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateCompanyProfileDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var updated = await _companyService.UpdateMyProfileAsync(userId.Value, dto);
        return updated
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status404NotFound, detail: "Company profile not found.");
    }

    /// <summary>
    /// Every internship post owned by the logged-in company, any status. Needed because the
    /// public GET /api/internships listing only shows Open posts (Phase 8) - a company
    /// otherwise has no way to see its own Draft/Closed posts. Optionally filter to one
    /// status via <c>?status=Draft</c> (Phase 12).
    /// </summary>
    [HttpGet("me/internships")]
    public async Task<ActionResult<List<InternshipListDto>>> GetMyInternships([FromQuery] InternshipStatus? status)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var internships = await _internshipService.GetByCompanyUserIdAsync(userId.Value, status);
        return Ok(internships);
    }

    /// <summary>
    /// One of the company's own internship posts, full details, regardless of status.
    /// Needed because GET /api/internships/{id} only ever returns Open posts (Phase 8) -
    /// a company otherwise has no way to fetch its own Draft/Closed post to pre-fill an
    /// edit form.
    /// </summary>
    [HttpGet("me/internships/{id:int}")]
    public async Task<ActionResult<InternshipDetailsDto>> GetMyInternshipById(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, internship) = await _internshipService.GetOwnedByIdAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own this internship post."),
            _ => Ok(internship)
        };
    }

    // Every applicant across all of the company's internships, in one list.
    [HttpGet("me/applications")]
    public async Task<ActionResult<List<ApplicantDto>>> GetMyApplications()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var applicants = await _applicationService.GetApplicantsForCompanyAsync(userId.Value);
        return Ok(applicants);
    }
}
