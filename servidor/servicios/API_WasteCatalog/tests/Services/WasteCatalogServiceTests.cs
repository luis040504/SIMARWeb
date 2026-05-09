using API_WasteCatalog.Data;
using API_WasteCatalog.DTOs;
using API_WasteCatalog.Models;
using API_WasteCatalog.Services;
using Microsoft.EntityFrameworkCore;

namespace API_WasteCatalog.Tests.Services;

public class WasteCatalogServiceTests : IDisposable
{
    private readonly CatalogDbContext _db;
    private readonly WasteCatalogService _sut;

    public WasteCatalogServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new CatalogDbContext(opts);
        _sut = new WasteCatalogService(_db);

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        _db.WasteTypes.AddRange(
            new WasteType
            {
                Id = 1, Code = "RP-RPBI-001", Name = "Objetos punzocortantes",
                Type = "peligroso", IsBiological = true, IsToxic = true, IsFlammable = false,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsMutagenic = false,
                ValidUnits = "kg,contenedores", IsActive = true
            },
            new WasteType
            {
                Id = 2, Code = "RP-SOL-001", Name = "Solventes halogenados gastados",
                Type = "peligroso", IsBiological = false, IsFlammable = true, IsToxic = true,
                ValidUnits = "lt,kg", IsActive = true
            },
            new WasteType
            {
                Id = 3, Code = "RME-CAR-001", Name = "Cartón y papel",
                Type = "especial", LgpgirCategory = "Materiales recuperables",
                ValidUnits = "kg,ton", IsActive = true
            },
            new WasteType
            {
                Id = 4, Code = "RME-ELE-001", Name = "Residuos eléctricos y electrónicos",
                Type = "especial", LgpgirCategory = "Tecnológicos",
                ValidUnits = "kg,pza", IsActive = true
            },
            new WasteType
            {
                Id = 5, Code = "RP-OLD-001", Name = "Inactive waste",
                Type = "peligroso", ValidUnits = "kg", IsActive = false
            }
        );
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    // ── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_NoFilters_ReturnsOnlyActiveItems()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto());

        Assert.Equal(4, result.TotalCount);
        Assert.DoesNotContain(result.Items, i => i.Code == "RP-OLD-001");
    }

    [Fact]
    public async Task GetAll_FilterByType_ReturnsMatchingItems()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { Type = "especial" });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, i => Assert.Equal("especial", i.Type));
    }

    [Fact]
    public async Task GetAll_FilterByType_Peligroso_ReturnsOnlyHazardous()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { Type = "peligroso" });

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, i => Assert.Equal("peligroso", i.Type));
    }

    [Fact]
    public async Task GetAll_SearchByName_ReturnsCaseInsensitiveMatch()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { Search = "CARTÓN" });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("RME-CAR-001", result.Items.First().Code);
    }

    [Fact]
    public async Task GetAll_SearchByCode_ReturnsMatchingItem()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { Search = "RP-SOL" });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("RP-SOL-001", result.Items.First().Code);
    }

    [Fact]
    public async Task GetAll_FilterIsBiological_ReturnsOnlyBiological()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { IsBiological = true });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("RP-RPBI-001", result.Items.First().Code);
    }

    [Fact]
    public async Task GetAll_FilterByLgpgirCategory_ReturnsMatchingItems()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { LgpgirCategory = "Tecnológicos" });

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("RME-ELE-001", result.Items.First().Code);
    }

    [Fact]
    public async Task GetAll_Pagination_ReturnsCorrectPage()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { PageNumber = 2, PageSize = 2 });

        Assert.Equal(4, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public async Task GetAll_PageSizeClamped_DoesNotExceed100()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { PageSize = 9999 });

        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetAll_PageSizeClamped_MinimumIsOne()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { PageSize = 0 });

        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task GetAll_NoMatches_ReturnsEmptyCollection()
    {
        var result = await _sut.GetAllAsync(new WasteFilterDto { Search = "nonexistent-xyz" });

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // ── GetByCodeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCode_ExistingCode_ReturnsDto()
    {
        var result = await _sut.GetByCodeAsync("RME-CAR-001");

        Assert.NotNull(result);
        Assert.Equal("RME-CAR-001", result.Code);
        Assert.Equal("Cartón y papel", result.Name);
        Assert.Contains("kg", result.Units);
        Assert.Contains("ton", result.Units);
    }

    [Fact]
    public async Task GetByCode_InactiveCode_ReturnsNull()
    {
        var result = await _sut.GetByCodeAsync("RP-OLD-001");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCode_NonexistentCode_ReturnsNull()
    {
        var result = await _sut.GetByCodeAsync("DOES-NOT-EXIST");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByCode_MapsAllCretibFields()
    {
        var result = await _sut.GetByCodeAsync("RP-RPBI-001");

        Assert.NotNull(result);
        Assert.True(result.IsBiological);
        Assert.True(result.IsToxic);
        Assert.False(result.IsFlammable);
    }
}
