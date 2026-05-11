using API_Audit.Controllers;
using API_Audit.DTOs;
using API_Audit.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace API_Audit.Tests.Controllers;

public class AuditControllerTests
{
    private readonly Mock<IAuditService> _serviceMock = new();
    private readonly Mock<ILogger<AuditController>> _loggerMock = new();
    private readonly AuditController _controller;

    public AuditControllerTests()
    {
        _controller = new AuditController(_serviceMock.Object, _loggerMock.Object);
    }

    private static CreateAuditLogDto ValidDto() => new()
    {
        EntityType = "Manifest",
        EntityId = "123",
        Action = "CREATE",
        PerformedBy = "user@simar.com",
        Status = "Success"
    };

    private static AuditLogResponseDto SampleResponse(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        EntityType = "Manifest",
        EntityId = "123",
        Action = "CREATE",
        PerformedBy = "user@simar.com",
        Status = "Success",
        Timestamp = DateTime.UtcNow
    };

    // ─── POST /api/audit/log ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateLog_ValidDto_Returns201Created()
    {
        var dto = ValidDto();
        var response = SampleResponse();
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(response);

        var result = await _controller.CreateLog(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(response, created.Value);
    }

    [Fact]
    public async Task CreateLog_ServiceThrowsException_Returns500()
    {
        var dto = ValidDto();
        _serviceMock.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var result = await _controller.CreateLog(dto);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, problem.StatusCode);
    }

    [Fact]
    public async Task CreateLog_InvalidModelState_DoesNotCallService()
    {
        _controller.ModelState.AddModelError("Action", "Action debe ser CREATE, UPDATE, DELETE o READ.");

        await _controller.CreateLog(ValidDto());

        // Con ModelState inválido el servicio nunca debe ser invocado
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<CreateAuditLogDto>()), Times.Never);
    }

    // ─── GET /api/audit/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_Returns200WithLog()
    {
        var id = Guid.NewGuid();
        var response = SampleResponse(id);
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(response);

        var result = await _controller.GetById(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task GetById_NonExistingId_Returns404()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((AuditLogResponseDto?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ─── GET /api/audit/entity/{entityType}/{entityId} ────────────────────────

    [Fact]
    public async Task GetByEntity_ValidParams_Returns200WithPagedResult()
    {
        var paged = new PagedResultDto<AuditLogResponseDto>
        {
            Items = [SampleResponse()],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };
        _serviceMock.Setup(s => s.GetByEntityAsync("Manifest", "1", 1, 20)).ReturnsAsync(paged);

        var result = await _controller.GetByEntity("Manifest", "1", 1, 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task GetByEntity_PageSizeOver100_ClampsTo100()
    {
        var paged = new PagedResultDto<AuditLogResponseDto>
            { Items = [], TotalCount = 0, PageNumber = 1, PageSize = 100 };
        _serviceMock.Setup(s => s.GetByEntityAsync("Manifest", "1", 1, 100)).ReturnsAsync(paged);

        var result = await _controller.GetByEntity("Manifest", "1", 1, 999);

        Assert.IsType<OkObjectResult>(result);
        _serviceMock.Verify(s => s.GetByEntityAsync("Manifest", "1", 1, 100), Times.Once);
    }

    [Fact]
    public async Task GetByEntity_PageNumberZero_ClampsTo1()
    {
        var paged = new PagedResultDto<AuditLogResponseDto>
            { Items = [], TotalCount = 0, PageNumber = 1, PageSize = 20 };
        _serviceMock.Setup(s => s.GetByEntityAsync("Manifest", "1", 1, 20)).ReturnsAsync(paged);

        var result = await _controller.GetByEntity("Manifest", "1", 0, 20);

        Assert.IsType<OkObjectResult>(result);
        _serviceMock.Verify(s => s.GetByEntityAsync("Manifest", "1", 1, 20), Times.Once);
    }

    // ─── GET /api/audit/user/{performedBy} ────────────────────────────────────

    [Fact]
    public async Task GetByUser_ValidUser_Returns200()
    {
        var paged = new PagedResultDto<AuditLogResponseDto>
            { Items = [SampleResponse()], TotalCount = 1, PageNumber = 1, PageSize = 20 };
        _serviceMock.Setup(s => s.GetByUserAsync("user@simar.com", 1, 20)).ReturnsAsync(paged);

        var result = await _controller.GetByUser("user@simar.com", 1, 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
    }

    [Fact]
    public async Task GetByUser_UnknownUser_Returns200WithEmptyItems()
    {
        var paged = new PagedResultDto<AuditLogResponseDto>
            { Items = [], TotalCount = 0, PageNumber = 1, PageSize = 20 };
        _serviceMock.Setup(s => s.GetByUserAsync("nobody", 1, 20)).ReturnsAsync(paged);

        var result = await _controller.GetByUser("nobody", 1, 20);

        var ok = Assert.IsType<OkObjectResult>(result);
        var body = Assert.IsType<PagedResultDto<AuditLogResponseDto>>(ok.Value);
        Assert.Equal(0, body.TotalCount);
    }
}
