using API_Audit.DTOs;
using API_Audit.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_Audit.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _service;
    private readonly ILogger<AuditController> _logger;

    public AuditController(IAuditService service, ILogger<AuditController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Registra un nuevo evento de auditoría.</summary>
    [HttpPost("log")]
    [ProducesResponseType(typeof(AuditLogResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLog([FromBody] CreateAuditLogDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error al registrar auditoría para {EntityType}/{EntityId}",
                dto.EntityType, dto.EntityId);
            return Problem(
                detail: "Error interno al registrar la auditoría.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>Obtiene un registro de auditoría por su ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null)
            return NotFound(new
            {
                code = "NOT_FOUND",
                message = $"Registro de auditoría con ID '{id}' no encontrado."
            });

        return Ok(result);
    }

    /// <summary>Retorna el historial de auditoría de una entidad específica, paginado.</summary>
    [HttpGet("entity/{entityType}/{entityId}")]
    [ProducesResponseType(typeof(PagedResultDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        string entityId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(1, pageNumber);

        var result = await _service.GetByEntityAsync(entityType, entityId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>Retorna el historial de auditoría de un usuario, paginado.</summary>
    [HttpGet("user/{performedBy}")]
    [ProducesResponseType(typeof(PagedResultDto<AuditLogResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(
        string performedBy,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(1, pageNumber);

        var result = await _service.GetByUserAsync(performedBy, pageNumber, pageSize);
        return Ok(result);
    }
}
