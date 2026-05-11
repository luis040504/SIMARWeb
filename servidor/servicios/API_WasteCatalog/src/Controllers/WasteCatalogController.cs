using API_WasteCatalog.DTOs;
using API_WasteCatalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_WasteCatalog.Controllers;

[ApiController]
[Route("api/catalog")]
public class WasteCatalogController : ControllerBase
{
    private readonly IWasteCatalogService _service;

    public WasteCatalogController(IWasteCatalogService service) => _service = service;

    // ── READ ──────────────────────────────────────────────────────────────────

    /// <summary>List active waste types with optional filters and pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WasteFilterDto filters)
    {
        var result = await _service.GetAllAsync(filters);
        return Ok(result);
    }

    /// <summary>Get a single waste type by its numeric ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null)
            return NotFound(new { message = $"Waste type with id {id} not found." });

        return Ok(item);
    }

    /// <summary>Get a single waste type by its regulatory code (e.g. "RP-RPBI-001").</summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var item = await _service.GetByCodeAsync(code);
        if (item is null)
            return NotFound(new { message = $"Waste type '{code}' not found." });

        return Ok(item);
    }

    // ── WRITE ─────────────────────────────────────────────────────────────────

    /// <summary>Create a new waste type.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WasteTypeUpsertDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Conflict(new { message = $"A waste type with code '{dto.Code.ToUpper()}' already exists." });
        }
    }

    /// <summary>Update an existing waste type by its numeric ID.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] WasteTypeUpsertDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(new { message = $"Waste type with id {id} not found." });

            return Ok(updated);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Conflict(new { message = $"A waste type with code '{dto.Code.ToUpper()}' already exists." });
        }
    }

    /// <summary>Soft-delete a waste type by its numeric ID.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Waste type with id {id} not found." });

        return NoContent();
    }
}
