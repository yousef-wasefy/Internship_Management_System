using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Enums;
using InternshipManagement.Api.Helpers;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

// Controllers stay thin: they translate HTTP <-> DTOs and delegate all business logic
// (including ownership and publishing rules) to IInternshipService.
[ApiController]
[Route("api/internships")]
public class InternshipsController : ControllerBase
{
    private readonly IInternshipService _internshipService;
    private readonly IApplicationService _applicationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public InternshipsController(
        IInternshipService internshipService,
        IApplicationService applicationService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _internshipService = internshipService;
        _applicationService = applicationService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<List<InternshipListDto>>> GetAll()
    {
        var internships = await _internshipService.GetAllAsync();
        return Ok(internships);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InternshipDetailsDto>> GetById(int id)
    {
        var internship = await _internshipService.GetByIdAsync(id);
        return internship is null ? NotFound() : Ok(internship);
    }

    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<InternshipDetailsDto>> Create(CreateInternshipDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var created = await _internshipService.CreateAsync(dto, userId.Value);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Update(int id, UpdateInternshipDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _internshipService.UpdateAsync(id, dto, userId.Value);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _internshipService.DeleteAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.Forbidden => Forbid(),
            _ => NoContent()
        };
    }

    [HttpPatch("{id:int}/open")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<InternshipDetailsDto>> Open(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var (result, error, internship) = await _internshipService.OpenAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.Forbidden => Forbid(),
            OperationResult.ValidationFailed => BadRequest(new { message = error }),
            _ => Ok(internship)
        };
    }

    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<InternshipDetailsDto>> Close(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var (result, error, internship) = await _internshipService.CloseAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.Forbidden => Forbid(),
            OperationResult.ValidationFailed => BadRequest(new { message = error }),
            _ => Ok(internship)
        };
    }

    [HttpPost("{id:int}/apply")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApplicationDto>> Apply(int id, ApplyToInternshipDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized();
        }

        var (result, error, application) = await _applicationService.ApplyAsync(id, userId.Value, dto);
        return result switch
        {
            OperationResult.NotFound => NotFound(),
            OperationResult.ValidationFailed => BadRequest(new { message = error }),
            // 201 with no Location header: there's no GET /api/applications/{id} endpoint
            // yet to point at (see docs/API_SPEC.md), so the created resource is
            // returned directly instead of linked.
            _ => StatusCode(StatusCodes.Status201Created, application)
        };
    }
}
