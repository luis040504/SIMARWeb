using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Generate;

public class SelectClientModel : PageModel
{
    private readonly ClientesApiService _clientes;
    private readonly ContratosApiService _contratos;
    private readonly ManifestApiService _manifests;

    public SelectClientModel(ClientesApiService clientes, ContratosApiService contratos, ManifestApiService manifests)
    {
        _clientes  = clientes;
        _contratos = contratos;
        _manifests = manifests;
    }

    [BindProperty(SupportsGet = true)]
    public string? Q { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ClienteId { get; set; }

    public List<ClienteDto> ClienteResults { get; private set; } = [];
    public ClienteDto? ClienteSeleccionado { get; private set; }
    public List<ContratoConClienteVm> ContratosActivos { get; private set; } = [];

    // key: ContratoId  value: (cantidad RP, cantidad RME)
    public Dictionary<int, (int Rp, int Rme)> ManifiestosPorContrato { get; private set; } = new();

    public string? ApiError { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        if (ClienteId.HasValue)
        {
            try
            {
                var clienteTask   = _clientes.GetByIdAsync(ClienteId.Value);
                var contratosTask = _contratos.GetAllAsync();
                var manifestsTask = _manifests.GetAllAsync(clienteId: ClienteId.Value);
                await Task.WhenAll(clienteTask, contratosTask, manifestsTask);

                ClienteSeleccionado = clienteTask.Result;

                ContratosActivos = contratosTask.Result
                    .Where(c => c.ClientId == ClienteId.Value &&
                                c.Status.Equals("activo", StringComparison.OrdinalIgnoreCase))
                    .Select(c => new ContratoConClienteVm
                    {
                        ContratoId     = c.Id,
                        Folio          = c.Folio,
                        Status         = c.Status,
                        ExpirationDate = c.ExpirationDate,
                        ClienteId      = c.ClientId,
                        ClienteNombre  = c.ClientName
                    }).ToList();

                foreach (var m in manifestsTask.Result.Where(m => m.ContratoId.HasValue))
                {
                    var cid = m.ContratoId!.Value;
                    ManifiestosPorContrato.TryGetValue(cid, out var counts);
                    ManifiestosPorContrato[cid] = m.Type == "peligroso"
                        ? (counts.Rp + 1, counts.Rme)
                        : (counts.Rp, counts.Rme + 1);
                }
            }
            catch
            {
                ApiError = "No se pudo cargar la información del cliente.";
            }
        }
        else if (!string.IsNullOrWhiteSpace(Q))
        {
            try
            {
                ClienteResults = await _clientes.SearchByBusinessNameAsync(Q.Trim());
            }
            catch
            {
                ApiError = "No se pudo cargar la lista de clientes.";
            }
        }

        return Page();
    }
}
