using API_WasteCatalog.Data;
using API_WasteCatalog.DTOs;
using API_WasteCatalog.Models;
using Microsoft.EntityFrameworkCore;

namespace API_WasteCatalog.Services;

public class WasteCatalogService : IWasteCatalogService
{
    private readonly CatalogDbContext _db;

    public WasteCatalogService(CatalogDbContext db) => _db = db;

    public async Task<PagedResultDto<WasteTypeDto>> GetAllAsync(WasteFilterDto filters)
    {
        var query = _db.WasteTypes.Where(w => w.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Type))
            query = query.Where(w => w.Type == filters.Type);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var term = filters.Search.ToLower();
            query = query.Where(w =>
                w.Name.ToLower().Contains(term) ||
                w.Code.ToLower().Contains(term));
        }

        if (filters.IsBiological.HasValue)
            query = query.Where(w => w.IsBiological == filters.IsBiological.Value);

        if (!string.IsNullOrWhiteSpace(filters.LgpgirCategory))
            query = query.Where(w => w.LgpgirCategory == filters.LgpgirCategory);

        var totalCount = await query.CountAsync();

        var pageSize = Math.Clamp(filters.PageSize, 1, 100);
        var pageNumber = Math.Max(filters.PageNumber, 1);

        var items = await query
            .OrderBy(w => w.Type)
            .ThenBy(w => w.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(w => ToDto(w))
            .ToListAsync();

        return new PagedResultDto<WasteTypeDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<WasteTypeDto?> GetByCodeAsync(string code)
    {
        var waste = await _db.WasteTypes
            .Where(w => w.IsActive && w.Code == code)
            .FirstOrDefaultAsync();

        return waste is null ? null : ToDto(waste);
    }

    private static WasteTypeDto ToDto(WasteType w) => new()
    {
        Id = w.Id,
        Code = w.Code,
        Name = w.Name,
        Type = w.Type,
        Description = w.Description,
        IsCorrosive = w.IsCorrosive,
        IsReactive = w.IsReactive,
        IsExplosive = w.IsExplosive,
        IsToxic = w.IsToxic,
        IsFlammable = w.IsFlammable,
        IsBiological = w.IsBiological,
        IsMutagenic = w.IsMutagenic,
        PhysicalState = w.PhysicalState,
        LgpgirCategory = w.LgpgirCategory,
        ValidUnits = w.ValidUnits
    };
}
