using InternshipManagement.Api.DTOs.Applications;
using InternshipManagement.Api.DTOs.Common;
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

    /// <summary>
    /// Public listing of Open internships. Supports pagination (<c>page</c>, <c>pageSize</c>,
    /// max 50), filtering (<c>location</c>, <c>workMode</c>), and a title search
    /// (<c>search</c>) - see docs/API_SPEC.md for examples.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<InternshipListDto>>> GetAll([FromQuery] InternshipQueryParameters query)
    {
        return Ok(await _internshipService.GetAllAsync(query));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InternshipDetailsDto>> GetById(int id)
    {
        var internship = await _internshipService.GetByIdAsync(id);
        return internship is null
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found.")
            : Ok(internship);
    }

    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<InternshipDetailsDto>> Create(CreateInternshipDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
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
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _internshipService.UpdateAsync(id, dto, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own this internship post."),
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
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _internshipService.DeleteAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own this internship post."),
            _ => NoContent()
        };
    }

    /// <summary>
    /// Publishes a Draft (or reopens a Closed) internship. Requires: the caller owns it,
    /// their company is approved, Title and Description are non-empty, the deadline is in
    /// the future, and it isn't Cancelled. See docs/API_SPEC.md for the exact error per case.
    /// </summary>
    [HttpPatch("{id:int}/open")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<InternshipDetailsDto>> Open(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, error, internship) = await _internshipService.OpenAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own this internship post."),
            OperationResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest, detail: error),
            _ => Ok(internship)
        };
    }

    /// <summary>Closes an Open internship. Owner only.</summary>
    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<InternshipDetailsDto>> Close(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, error, internship) = await _internshipService.CloseAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own this internship post."),
            OperationResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest, detail: error),
            _ => Ok(internship)
        };
    }

    /// <summary>
    /// Applies to an internship as the logged-in Student. Requires: it's Open, the deadline
    /// hasn't passed, and the student hasn't already applied.
    /// </summary>
    [HttpPost("{id:int}/apply")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<ApplicationDto>> Apply(int id, ApplyToInternshipDto dto)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, error, application) = await _applicationService.ApplyAsync(id, userId.Value, dto);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.ValidationFailed => Problem(statusCode: StatusCodes.Status400BadRequest, detail: error),
            // 201 with no Location header: there's no GET /api/applications/{id} endpoint
            // yet to point at (see docs/API_SPEC.md), so the created resource is
            // returned directly instead of linked.
            _ => StatusCode(StatusCodes.Status201Created, application)
        };
    }

    /// <summary>Every applicant for one specific internship, from the owning company's perspective.</summary>
    [HttpGet("{id:int}/applications")]
    [Authorize(Roles = "Company")]
    public async Task<ActionResult<List<ApplicantDto>>> GetApplicants(int id)
    {
        var userId = _currentUserAccessor.GetUserId(User);
        if (userId is null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var (result, error, applicants) = await _applicationService.GetApplicantsForInternshipAsync(id, userId.Value);
        return result switch
        {
            OperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, detail: "Internship not found."),
            OperationResult.Forbidden => Problem(statusCode: StatusCodes.Status403Forbidden, detail: "You do not own this internship post."),
            _ => Ok(applicants)
        };
    }
}
