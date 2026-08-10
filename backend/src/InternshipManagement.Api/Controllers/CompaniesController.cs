using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.DTOs.Internships;
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
            return Unauthorized();
        }

        var profile = await _companyService.GetMyProfileAsync(userId.Value);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateCompanyProfileDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _companyService.UpdateMyProfileAsync(userId.Value, dto);
        return updated ? NoContent() : NotFound();
    }

    // Needed because the public GET /api/internships listing only shows Open posts
    // (Phase 8) - a company otherwise has no way to see its own Draft/Closed posts.
    [HttpGet("me/internships")]
    public async Task<ActionResult<List<InternshipListDto>>> GetMyInternships()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var internships = await _internshipService.GetByCompanyUserIdAsync(userId.Value);
        return Ok(internships);
    }

    // Every applicant across all of the company's internships, in one list.
    [HttpGet("me/applications")]
    public async Task<ActionResult<List<ApplicantDto>>> GetMyApplications()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var applicants = await _applicationService.GetApplicantsForCompanyAsync(userId.Value);
        return Ok(applicants);
    }
}
