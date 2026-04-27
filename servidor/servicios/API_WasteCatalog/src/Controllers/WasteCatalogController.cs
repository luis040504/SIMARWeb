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

    /// <summary>List waste types with optional filters and pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] WasteFilterDto filters)
    {
        var result = await _service.GetAllAsync(filters);
        return Ok(result);
    }

    /// <summary>Get a single waste type by its regulatory code.</summary>
    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var item = await _service.GetByCodeAsync(code);
        if (item is null)
            return NotFound(new { message = $"Waste type '{code}' not found." });

        return Ok(item);
    }
}
