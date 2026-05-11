using API_Audit.Data;
using API_Audit.DTOs;
using API_Audit.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace API_Audit.Tests.Services;

public class AuditServiceTests
{
    private static AuditDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AuditDbContext(options);
    }

    private static CreateAuditLogDto BuildDto(
        string entityType = "Manifest",
        string entityId = "1",
        string action = "CREATE",
        string performedBy = "user@simar.com",
        string status = "Success") => new()
    {
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        PerformedBy = performedBy,
        Status = status
    };

    // ─── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsResponseWithNewId()
    {
        using var db = CreateContext(nameof(CreateAsync_ValidDto_ReturnsResponseWithNewId));
        var service = new AuditService(db);

        var result = await service.CreateAsync(BuildDto());

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Manifest", result.EntityType);
        Assert.Equal("CREATE", result.Action);
        Assert.Equal("user@simar.com", result.PerformedBy);
    }

    [Fact]
    public async Task CreateAsync_PersistsRecordInDatabase()
    {
        using var db = CreateContext(nameof(CreateAsync_PersistsRecordInDatabase));
        var service = new AuditService(db);

        var result = await service.CreateAsync(BuildDto(entityType: "Contract", entityId: "456"));
        var saved = await db.AuditLogs.FindAsync(result.Id);

        Assert.NotNull(saved);
        Assert.Equal("Contract", saved.EntityType);
        Assert.Equal("456", saved.EntityId);
    }

    [Fact]
    public async Task CreateAsync_FailureStatus_PersistsErrorMessage()
    {
        using var db = CreateContext(nameof(CreateAsync_FailureStatus_PersistsErrorMessage));
        var service = new AuditService(db);
        var dto = BuildDto(status: "Failure");
        dto.ErrorMessage = "Cliente sin contrato vigente.";

        var result = await service.CreateAsync(dto);

        Assert.Equal("Failure", result.Status);
        Assert.Equal("Cliente sin contrato vigente.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateAsync_SetsTimestampToUtcNow()
    {
        using var db = CreateContext(nameof(CreateAsync_SetsTimestampToUtcNow));
        var service = new AuditService(db);
        var before = DateTime.UtcNow.AddSeconds(-1);

        var result = await service.CreateAsync(BuildDto());

        Assert.True(result.Timestamp >= before);
        Assert.True(result.Timestamp <= DateTime.UtcNow.AddSeconds(1));
    }

    // ─── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectLog()
    {
        using var db = CreateContext(nameof(GetByIdAsync_ExistingId_ReturnsCorrectLog));
        var service = new AuditService(db);
        var created = await service.CreateAsync(BuildDto(entityType: "Client", action: "DELETE"));

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Client", result.EntityType);
        Assert.Equal("DELETE", result.Action);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        using var db = CreateContext(nameof(GetByIdAsync_NonExistingId_ReturnsNull));
        var service = new AuditService(db);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ─── GetByEntityAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEntityAsync_ReturnsOnlyMatchingEntityLogs()
    {
        using var db = CreateContext(nameof(GetByEntityAsync_ReturnsOnlyMatchingEntityLogs));
        var service = new AuditService(db);

        await service.CreateAsync(BuildDto(entityType: "Manifest", entityId: "1", action: "CREATE"));
        await service.CreateAsync(BuildDto(entityType: "Manifest", entityId: "1", action: "UPDATE"));
        await service.CreateAsync(BuildDto(entityType: "Contract", entityId: "1", action: "CREATE"));

        var result = await service.GetByEntityAsync("Manifest", "1", 1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item =>
        {
            Assert.Equal("Manifest", item.EntityType);
            Assert.Equal("1", item.EntityId);
        });
    }

    [Fact]
    public async Task GetByEntityAsync_NoMatches_ReturnsTotalCountZero()
    {
        using var db = CreateContext(nameof(GetByEntityAsync_NoMatches_ReturnsTotalCountZero));
        var service = new AuditService(db);

        var result = await service.GetByEntityAsync("Manifest", "nonexistent", 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetByEntityAsync_PaginationReturnsCorrectSlices()
    {
        using var db = CreateContext(nameof(GetByEntityAsync_PaginationReturnsCorrectSlices));
        var service = new AuditService(db);

        for (var i = 0; i < 5; i++)
            await service.CreateAsync(BuildDto(entityId: "paginate"));

        var page1 = await service.GetByEntityAsync("Manifest", "paginate", 1, 3);
        var page2 = await service.GetByEntityAsync("Manifest", "paginate", 2, 3);

        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(3, page1.Items.Count());
        Assert.Equal(2, page2.Items.Count());
        Assert.Equal(2, page1.TotalPages);
    }

    [Fact]
    public async Task GetByEntityAsync_ReturnsLogsOrderedByTimestampDescending()
    {
        using var db = CreateContext(nameof(GetByEntityAsync_ReturnsLogsOrderedByTimestampDescending));
        var service = new AuditService(db);

        await service.CreateAsync(BuildDto(action: "CREATE"));
        await Task.Delay(10);
        await service.CreateAsync(BuildDto(action: "UPDATE"));

        var result = await service.GetByEntityAsync("Manifest", "1", 1, 20);
        var items = result.Items.ToList();

        Assert.True(items[0].Timestamp >= items[1].Timestamp);
        Assert.Equal("UPDATE", items[0].Action);
    }

    // ─── GetByUserAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyLogsForGivenUser()
    {
        using var db = CreateContext(nameof(GetByUserAsync_ReturnsOnlyLogsForGivenUser));
        var service = new AuditService(db);

        await service.CreateAsync(BuildDto(entityId: "1", performedBy: "user@simar.com"));
        await service.CreateAsync(BuildDto(entityId: "2", performedBy: "user@simar.com"));
        await service.CreateAsync(BuildDto(entityId: "3", performedBy: "other@simar.com"));

        var result = await service.GetByUserAsync("user@simar.com", 1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal("user@simar.com", item.PerformedBy));
    }

    [Fact]
    public async Task GetByUserAsync_UnknownUser_ReturnsEmptyResult()
    {
        using var db = CreateContext(nameof(GetByUserAsync_UnknownUser_ReturnsEmptyResult));
        var service = new AuditService(db);

        var result = await service.GetByUserAsync("nobody@simar.com", 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}
