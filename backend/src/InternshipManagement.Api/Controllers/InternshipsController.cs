using InternshipManagement.Api.DTOs.Internships;
using InternshipManagement.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManagement.Api.Controllers;

// Controllers stay thin: they translate HTTP <-> DTOs and delegate all business logic
// (including the temporary placeholder-company assignment) to IInternshipService.
[ApiController]
[Route("api/internships")]
public class InternshipsController : ControllerBase
{
    private readonly IInternshipService _internshipService;

    public InternshipsController(IInternshipService internshipService)
    {
        _internshipService = internshipService;
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
    public async Task<ActionResult<InternshipDetailsDto>> Create(CreateInternshipDto dto)
    {
        var created = await _internshipService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateInternshipDto dto)
    {
        var updated = await _internshipService.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _internshipService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
