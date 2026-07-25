using InternshipManagement.Api.DTOs.Companies;
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
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CompaniesController(ICompanyService companyService, ICurrentUserAccessor currentUserAccessor)
    {
        _companyService = companyService;
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
}
