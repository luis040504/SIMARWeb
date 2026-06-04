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

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        try
        {
            var all = await _api.GetAllAsync(
                FilterManifestNumber,
                FilterSocialReason,
                FilterType,
                FilterStatus,
                FilterDateFrom,
                FilterDateTo,
                FilterClienteId,
                FilterContratoId);

            // FILTRO SOLICITADO: Mostrar los vinculados a contratos por defecto
            if (string.IsNullOrEmpty(FilterStatus) && string.IsNullOrEmpty(FilterManifestNumber))
            {
                Results = all.Where(m => m.ContratoId.HasValue && m.Status != "cancelado").ToList();
            }
            else
            {
                Results = all;
            }
        }
        catch (BillingApiException ex)
        {
            ApiError = ex.Message;
        }
        catch (Exception)
        {
            ApiError = "No se pudo conectar con el servidor. Verifique que el servicio esté en línea.";
        }

        return Page();
    }
}
