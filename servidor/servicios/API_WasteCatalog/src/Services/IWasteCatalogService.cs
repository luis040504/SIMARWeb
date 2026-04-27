using API_WasteCatalog.DTOs;

namespace API_WasteCatalog.Services;

public interface IWasteCatalogService
{
    Task<PagedResultDto<WasteTypeDto>> GetAllAsync(WasteFilterDto filters);
    Task<WasteTypeDto?> GetByCodeAsync(string code);
}
