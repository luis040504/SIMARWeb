using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class IndexModel : PageModel
{
    private readonly ManifestApiService _api;

    public IndexModel(ManifestApiService api) => _api = api;

    [BindProperty(SupportsGet = true)]
    public string? FilterManifestNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterSocialReason { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FilterDateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FilterDateTo { get; set; }

    // Filtros para uso programático (otros microservicios / páginas internas)
    [BindProperty(SupportsGet = true)]
    public int? FilterClienteId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? FilterContratoId { get; set; }

    public List<ManifestSummary> Results { get; private set; } = [];

    public string? ApiError { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Results = await _api.GetAllAsync(
                FilterManifestNumber,
                FilterSocialReason,
                FilterType,
                FilterStatus,
                FilterDateFrom,
                FilterDateTo,
                FilterClienteId,
                FilterContratoId);
        }
        catch (BillingApiException ex)
        {
            ApiError = ex.Message;
        }
        catch (Exception)
        {
            ApiError = "No se pudo conectar con el servidor. Verifique que el servicio esté en línea.";
        }
    }
}

public class ManifestSummary
{
    public string Id { get; set; } = string.Empty;
    public string ManifestNumber { get; set; } = string.Empty;
    public string Type { get; set; } = "especial";
    public string Status { get; set; } = "borrador";
    public string SocialReason { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public DateOnly ManifestDate { get; set; }
    public string ResidueSummary { get; set; } = string.Empty;
    public string TransporterName { get; set; } = string.Empty;
}
