using InternshipManagement.Api.DTOs.Admin;
using InternshipManagement.Api.DTOs.Companies;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Platform-wide statistics: users, companies, internships, and applications by status.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        return Ok(await _adminService.GetDashboardAsync());
    }

    [HttpGet("companies/pending")]
    public async Task<ActionResult<List<CompanyProfileDto>>> GetPendingCompanies()
    {
        return Ok(await _adminService.GetPendingCompaniesAsync());
    }

    [HttpPatch("companies/{id:int}/approve")]
    public async Task<ActionResult<CompanyProfileDto>> ApproveCompany(int id)
    {
        var result = await _adminService.ApproveCompanyAsync(id);
        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Company not found.")
            : Ok(result);
    }

    /// <summary>
    /// Rejects a company: keeps it unapproved and disables its account outright (the schema
    /// has no separate "Rejected" state - see docs/DECISIONS.md D16).
    /// </summary>
    [HttpPatch("companies/{id:int}/reject")]
    public async Task<ActionResult<CompanyProfileDto>> RejectCompany(int id)
    {
        var result = await _adminService.RejectCompanyAsync(id);
        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Company not found.")
            : Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        return Ok(await _adminService.GetUsersAsync());
    }

    /// <summary>
    /// Disables a user, blocking future logins. Does not revoke tokens already issued -
    /// see the note in docs/API_SPEC.md.
    /// </summary>
    [HttpPatch("users/{id:int}/disable")]
    public async Task<ActionResult<AdminUserDto>> DisableUser(int id)
    {
        var result = await _adminService.DisableUserAsync(id);
        return result is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "User not found.")
            : Ok(result);
    }
}
