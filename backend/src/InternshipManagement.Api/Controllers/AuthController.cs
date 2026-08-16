using InternshipManagement.Api.DTOs.Auth;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AuthController(IAuthService authService, ICurrentUserAccessor currentUserAccessor)
    {
        _authService = authService;
        _currentUserAccessor = currentUserAccessor;
    }

    /// <summary>Registers a new Student account and returns a token, same as logging in right after.</summary>
    [HttpPost("register-student")]
    public async Task<ActionResult<AuthResponseDto>> RegisterStudent(RegisterStudentDto dto)
    {
        var result = await _authService.RegisterStudentAsync(dto);
        return result is null
            ? Problem(statusCode: StatusCodes.Status409Conflict, detail: "An account with this email already exists.")
            : Ok(result);
    }

    /// <summary>Registers a new Company account (starts unapproved) and returns a token.</summary>
    [HttpPost("register-company")]
    public async Task<ActionResult<AuthResponseDto>> RegisterCompany(RegisterCompanyDto dto)
    {
        var result = await _authService.RegisterCompanyAsync(dto);
        return result is null
            ? Problem(statusCode: StatusCodes.Status409Conflict, detail: "An account with this email already exists.")
            : Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return result is null
            ? Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "Invalid email or password.")
            : Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _authService.GetCurrentUserAsync(userId.Value);
        return result is null ? Problem(statusCode: StatusCodes.Status401Unauthorized) : Ok(result);
    }
}
