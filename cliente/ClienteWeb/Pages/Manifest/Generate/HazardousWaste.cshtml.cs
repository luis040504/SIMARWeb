using ClienteWeb.Pages.Manifest.Consult;
using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Generate;

public class HazardousWasteModel : PageModel
{
    private readonly ManifestApiService _api;
    private readonly ClientesApiService _clientes;
    private readonly VehiculosApiService _vehiculos;
    private readonly ContratosApiService _contratos;
    private readonly EmpleadosApiService _empleados;

    public HazardousWasteModel(ManifestApiService api, ClientesApiService clientes, VehiculosApiService vehiculos, ContratosApiService contratos, EmpleadosApiService empleados)
    {
        _api = api;
        _clientes = clientes;
        _vehiculos = vehiculos;
        _contratos = contratos;
        _empleados = empleados;
    }

    public List<VehiculoDto> Vehiculos { get; private set; } = [];
    public List<ChoferDto>   Choferes  { get; private set; } = [];

    // ── Vinculación con contrato / cliente ────────────────────────────────────
    [BindProperty] public int IdCliente  { get; set; }
    [BindProperty] public int ContratoId { get; set; }

    public string ClienteNombre { get; private set; } = string.Empty;

    // ===================== GENERADOR =====================
    [BindProperty]
    [Required(ErrorMessage = "El número de registro ambiental es obligatorio.")]
    public string EnvironmentalRegistrationNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "La razón social es obligatoria.")]
    public string SocialReason { get; set; } = string.Empty;

    [BindProperty] public string? PostalCode     { get; set; }
    [BindProperty] public string? Street         { get; set; }
    [BindProperty] public string? ExteriorNumber { get; set; }
    [BindProperty] public string? InteriorNumber { get; set; }
    [BindProperty] public string? Colony         { get; set; }
    [BindProperty] public string? Municipality   { get; set; }
    [BindProperty] public string? State          { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? Email { get; set; }

    [BindProperty] public string?   SafeHandlingInstructions { get; set; }
    [BindProperty] public string?   GeneratorResponsibleName { get; set; }
    [BindProperty] public DateOnly? GeneratorSignDate        { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // ===================== RESIDUOS =====================
    [BindProperty]
    public List<HazardousResidueItem> Residues { get; set; } = new() { new HazardousResidueItem() };

    // ===================== TRANSPORTISTA =====================
    [BindProperty] public string?   TransporterSocialReason        { get; set; }
    [BindProperty] public string?   TransporterPostalCode          { get; set; }
    [BindProperty] public string?   TransporterStreet              { get; set; }
    [BindProperty] public string?   TransporterExteriorNumber      { get; set; }
    [BindProperty] public string?   TransporterInteriorNumber      { get; set; }
    [BindProperty] public string?   TransporterColony              { get; set; }
    [BindProperty] public string?   TransporterMunicipality        { get; set; }
    [BindProperty] public string?   TransporterState               { get; set; }
    [BindProperty] public string?   TransporterPhone               { get; set; }
    [BindProperty][EmailAddress] public string? TransporterEmail   { get; set; }
    [BindProperty] public string?   TransporterAuthorizationNumber { get; set; }
    [BindProperty] public string?   TransporterSCTPermit           { get; set; }
    [BindProperty] public string?   VehicleType                    { get; set; }
    [BindProperty] public string?   VehiclePlate                   { get; set; }
    [BindProperty] public string?   DriverName                     { get; set; }
    [BindProperty] public string?   DriverLicense                  { get; set; }
    [BindProperty] public string?   TransportRoute                 { get; set; }
    [BindProperty] public string?   TransporterResponsibleName     { get; set; }
    [BindProperty] public DateOnly? TransporterSignDate            { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // ===================== DESTINATARIO =====================
    [BindProperty] public string?   ReceiverSocialReason           { get; set; }
    [BindProperty] public string?   ReceiverPostalCode             { get; set; }
    [BindProperty] public string?   ReceiverStreet                 { get; set; }
    [BindProperty] public string?   ReceiverExteriorNumber         { get; set; }
    [BindProperty] public string?   ReceiverInteriorNumber         { get; set; }
    [BindProperty] public string?   ReceiverColony                 { get; set; }
    [BindProperty] public string?   ReceiverMunicipality           { get; set; }
    [BindProperty] public string?   ReceiverState                  { get; set; }
    [BindProperty] public string?   ReceiverPhone                  { get; set; }
    [BindProperty][EmailAddress] public string? ReceiverEmail      { get; set; }
    [BindProperty] public string?   ReceiverAuthorizationNumber    { get; set; }
    [BindProperty] public string?   ReceiverPersonName             { get; set; }
    [BindProperty] public string?   ReceiverObservations           { get; set; }
    [BindProperty] public string?   ReceiverResponsibleName        { get; set; }
    [BindProperty] public DateOnly? ReceiverSignDate               { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync(int clienteId = 0, int contratoId = 0)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        IdCliente  = clienteId;
        ContratoId = contratoId;

        var clienteTask   = clienteId > 0 ? _clientes.GetByIdAsync(clienteId) : Task.FromResult<ClienteDto?>(null);
        var vehiculosTask = _vehiculos.GetAllAsync();
        var contratoTask  = contratoId > 0 ? _contratos.GetDetailAsync(contratoId) : Task.FromResult<ContratoDetailDto?>(null);
        var choferesTask  = _empleados.GetChoferesAsync();
        await Task.WhenAll(clienteTask, vehiculosTask, contratoTask, choferesTask);

        var cliente  = clienteTask.Result;
        var contrato = contratoTask.Result;

        if (cliente is not null)
        {
            ClienteNombre                   = cliente.Name;
            SocialReason                    = cliente.Name;
            Street                          = cliente.Address ?? string.Empty;
            PhoneNumber                     = cliente.Phone ?? string.Empty;
            EnvironmentalRegistrationNumber = cliente.SemarnatNum ?? string.Empty;
        }

        if (contrato?.Services is { Count: > 0 })
        {
            Residues = contrato.Services
                .Select(s => new HazardousResidueItem { ResidueName = s.WasteType })
                .ToList();
        }

        Vehiculos = vehiculosTask.Result;
        Choferes  = choferesTask.Result;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        if (!ModelState.IsValid)
            return Page();

        try
        {
            var numeroManifiesto = await _api.CreateHazardousAsync(this);
            TempData["SuccessMessage"] = $"Manifiesto {numeroManifiesto} guardado como borrador. Puede editarlo y enviarlo a tránsito desde el listado.";
            return RedirectToPage("/Manifest/Consult/Index");
        }
        catch (BillingApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "No se pudo conectar con el servidor. Intente de nuevo.");
            return Page();
        }
    }
}

