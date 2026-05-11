using API_WasteCatalog.Controllers;
using API_WasteCatalog.DTOs;
using API_WasteCatalog.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API_WasteCatalog.Tests.Controllers;

public class WasteCatalogControllerTests
{
    private readonly Mock<IWasteCatalogService> _serviceMock = new();
    private readonly WasteCatalogController _sut;

    public WasteCatalogControllerTests() => _sut = new WasteCatalogController(_serviceMock.Object);

    // ── GET /api/catalog ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var expected = new PagedResultDto<WasteTypeDto>
        {
            Items = [new WasteTypeDto { Code = "RP-RPBI-001", Name = "Objetos punzocortantes" }],
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };
        _serviceMock
            .Setup(s => s.GetAllAsync(It.IsAny<WasteFilterDto>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetAll(new WasteFilterDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task GetAll_PassesFiltersToService()
    {
        var filters = new WasteFilterDto { Type = "peligroso", Search = "solvent", PageSize = 5 };
        _serviceMock
            .Setup(s => s.GetAllAsync(It.IsAny<WasteFilterDto>()))
            .ReturnsAsync(new PagedResultDto<WasteTypeDto>());

        await _sut.GetAll(filters);

        _serviceMock.Verify(s => s.GetAllAsync(
            It.Is<WasteFilterDto>(f => f.Type == "peligroso" && f.Search == "solvent" && f.PageSize == 5)),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_EmptyResult_ReturnsOkWithEmptyItems()
    {
        _serviceMock
            .Setup(s => s.GetAllAsync(It.IsAny<WasteFilterDto>()))
            .ReturnsAsync(new PagedResultDto<WasteTypeDto> { Items = [], TotalCount = 0 });

        var result = await _sut.GetAll(new WasteFilterDto());

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<PagedResultDto<WasteTypeDto>>(ok.Value);
        Assert.Empty(dto.Items);
    }

    // ── GET /api/catalog/{code} ──────────────────────────────────────────────

    [Fact]
    public async Task GetByCode_ExistingCode_ReturnsOkWithDto()
    {
        var dto = new WasteTypeDto { Code = "RME-CAR-001", Name = "Cartón y papel" };
        _serviceMock
            .Setup(s => s.GetByCodeAsync("RME-CAR-001"))
            .ReturnsAsync(dto);

        var result = await _sut.GetByCode("RME-CAR-001");

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(dto, ok.Value);
    }

    [Fact]
    public async Task GetByCode_NotFound_Returns404()
    {
        _serviceMock
            .Setup(s => s.GetByCodeAsync("UNKNOWN"))
            .ReturnsAsync((WasteTypeDto?)null);

        var result = await _sut.GetByCode("UNKNOWN");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetByCode_NotFound_ResponseContainsMessage()
    {
        _serviceMock
            .Setup(s => s.GetByCodeAsync("UNKNOWN"))
            .ReturnsAsync((WasteTypeDto?)null);

        var result = await _sut.GetByCode("UNKNOWN");

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var body = notFound.Value?.ToString();
        Assert.Contains("UNKNOWN", body ?? string.Empty);
    }

    [Fact]
    public async Task GetByCode_CallsServiceWithExactCode()
    {
        _serviceMock
            .Setup(s => s.GetByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((WasteTypeDto?)null);

        await _sut.GetByCode("RP-BAT-001");

        _serviceMock.Verify(s => s.GetByCodeAsync("RP-BAT-001"), Times.Once);
    }

    [Fact]
    public async Task GetByCode_ReturnedDto_UnitsPropertyIsPopulated()
    {
        var dto = new WasteTypeDto { Code = "RP-BAT-001", ValidUnits = "kg,pza" };
        _serviceMock
            .Setup(s => s.GetByCodeAsync("RP-BAT-001"))
            .ReturnsAsync(dto);

        var result = await _sut.GetByCode("RP-BAT-001");

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<WasteTypeDto>(ok.Value);
        Assert.Contains("kg", returned.Units);
        Assert.Contains("pza", returned.Units);
    }
}
