using API_WasteCatalog.DTOs;

namespace API_WasteCatalog.Services;

public interface IWasteCatalogService
{
    Task<PagedResultDto<WasteTypeDto>> GetAllAsync(WasteFilterDto filters);
    Task<WasteTypeDto?> GetByIdAsync(int id);
    Task<WasteTypeDto?> GetByCodeAsync(string code);
    Task<WasteTypeDto> CreateAsync(WasteTypeUpsertDto dto);
    Task<WasteTypeDto?> UpdateAsync(int id, WasteTypeUpsertDto dto);
    Task<bool> DeleteAsync(int id);
}
