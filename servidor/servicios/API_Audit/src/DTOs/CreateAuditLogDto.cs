using System.ComponentModel.DataAnnotations;

namespace API_Audit.DTOs;

public class CreateAuditLogDto
{
    [Required(ErrorMessage = "EntityType es requerido.")]
    [MaxLength(100, ErrorMessage = "EntityType no puede superar 100 caracteres.")]
    public string EntityType { get; set; } = string.Empty;

    [Required(ErrorMessage = "EntityId es requerido.")]
    [MaxLength(100, ErrorMessage = "EntityId no puede superar 100 caracteres.")]
    public string EntityId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Action es requerido.")]
    [RegularExpression("^(CREATE|UPDATE|DELETE|READ)$",
        ErrorMessage = "Action debe ser CREATE, UPDATE, DELETE o READ.")]
    public string Action { get; set; } = string.Empty;

    [Required(ErrorMessage = "PerformedBy es requerido.")]
    [MaxLength(200, ErrorMessage = "PerformedBy no puede superar 200 caracteres.")]
    public string PerformedBy { get; set; } = string.Empty;

    public string? Payload { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [RegularExpression("^(Success|Failure)$",
        ErrorMessage = "Status debe ser Success o Failure.")]
    public string Status { get; set; } = "Success";

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}
