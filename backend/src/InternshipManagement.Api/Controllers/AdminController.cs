using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

// Phase 7 stub: just enough for an admin to approve a company. The full admin module
// (dashboard, pending-companies list, reject, user management) is built in Phase 11 -
// more actions get added to this same controller then.
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public AdminController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPatch("companies/{id:int}/approve")]
    public async Task<ActionResult<CompanyProfileDto>> ApproveCompany(int id)
    {
        var result = await _companyService.ApproveAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
