using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Generate;

public class SelectContractModel : PageModel
{
    private readonly ContratosApiService _contratos;
    private readonly ClientesApiService _clientes;

    public SelectContractModel(ContratosApiService contratos, ClientesApiService clientes)
    {
        _contratos = contratos;
        _clientes = clientes;
    }

    // tipo: "especial" o "peligroso"
    [BindProperty(SupportsGet = true)]
    public string Tipo { get; set; } = "especial";

    public List<ContratoConClienteVm> Contratos { get; private set; } = [];

    public string? ApiError { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        try
        {
            var contratos = await _contratos.GetAllAsync();
            var clientes  = await _clientes.GetAllAsync();

            var clienteMap = clientes.ToDictionary(c => c.Id);

            Contratos = contratos
                .Where(c => c.Status.Equals("activo", StringComparison.OrdinalIgnoreCase))
                .Select(c => new ContratoConClienteVm
                {
                    ContratoId     = c.Id,
                    Folio          = c.Folio,
                    Status         = c.Status,
                    ExpirationDate = c.ExpirationDate,
                    ClienteId      = c.ClientId,
                    ClienteNombre  = clienteMap.TryGetValue(c.ClientId, out var cl) ? cl.Name : $"Cliente #{c.ClientId}"
                }).ToList();
        }
        catch (Exception)
        {
            ApiError = "No se pudo cargar la lista de contratos. Verifique que el servicio esté en línea.";
        }

        return Page();
    }
}

public class ContratoConClienteVm
{
    public int ContratoId { get; set; }
    public string Folio { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime? ExpirationDate { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";
}
