using API_Audit.DTOs;

namespace API_Audit.Services;

public interface IAuditService
{
    Task<AuditLogResponseDto> CreateAsync(CreateAuditLogDto dto);
    Task<AuditLogResponseDto?> GetByIdAsync(Guid id);
    Task<PagedResultDto<AuditLogResponseDto>> GetByEntityAsync(
        string entityType, string entityId, int pageNumber, int pageSize);
    Task<PagedResultDto<AuditLogResponseDto>> GetByUserAsync(
        string performedBy, int pageNumber, int pageSize);
}
