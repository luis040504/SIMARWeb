using API_Audit.Data;
using API_Audit.DTOs;
using API_Audit.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Audit.Services;

public class AuditService : IAuditService
{
    private readonly AuditDbContext _db;

    public AuditService(AuditDbContext db)
    {
        _db = db;
    }

    public async Task<AuditLogResponseDto> CreateAsync(CreateAuditLogDto dto)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            Action = dto.Action,
            PerformedBy = dto.PerformedBy,
            Timestamp = DateTime.UtcNow,
            Payload = dto.Payload,
            IpAddress = dto.IpAddress,
            Status = dto.Status,
            ErrorMessage = dto.ErrorMessage
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();

        return ToResponse(log);
    }

    public async Task<AuditLogResponseDto?> GetByIdAsync(Guid id)
    {
        var log = await _db.AuditLogs.FindAsync(id);
        return log is null ? null : ToResponse(log);
    }

    public async Task<PagedResultDto<AuditLogResponseDto>> GetByEntityAsync(
        string entityType, string entityId, int pageNumber, int pageSize)
    {
        var query = _db.AuditLogs
            .Where(l => l.EntityType == entityType && l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp);

        return await BuildPagedResult(query, pageNumber, pageSize);
    }

    public async Task<PagedResultDto<AuditLogResponseDto>> GetByUserAsync(
        string performedBy, int pageNumber, int pageSize)
    {
        var query = _db.AuditLogs
            .Where(l => l.PerformedBy == performedBy)
            .OrderByDescending(l => l.Timestamp);

        return await BuildPagedResult(query, pageNumber, pageSize);
    }

    private static async Task<PagedResultDto<AuditLogResponseDto>> BuildPagedResult(
        IQueryable<AuditLog> query, int pageNumber, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(l => ToResponse(l))
            .ToListAsync();

        return new PagedResultDto<AuditLogResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static AuditLogResponseDto ToResponse(AuditLog log) => new()
    {
        Id = log.Id,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        Action = log.Action,
        PerformedBy = log.PerformedBy,
        Timestamp = log.Timestamp,
        Payload = log.Payload,
        IpAddress = log.IpAddress,
        Status = log.Status,
        ErrorMessage = log.ErrorMessage
    };
}
