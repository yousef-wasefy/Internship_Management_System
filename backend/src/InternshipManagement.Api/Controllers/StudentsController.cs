using InternshipManagement.Api.DTOs.Students;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

[ApiController]
[Route("api/students")]
[Authorize(Roles = "Student")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public StudentsController(IStudentService studentService, ICurrentUserAccessor currentUserAccessor)
    {
        _studentService = studentService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("me")]
    public async Task<ActionResult<StudentProfileDto>> GetMe()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var profile = await _studentService.GetMyProfileAsync(userId.Value);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateStudentProfileDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _studentService.UpdateMyProfileAsync(userId.Value, dto);
        return updated ? NoContent() : NotFound();
    }
}
