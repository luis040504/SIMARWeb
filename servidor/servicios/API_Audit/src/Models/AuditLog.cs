namespace API_Audit.Models;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Payload { get; set; }
    public string? IpAddress { get; set; }
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
}
