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

    public HazardousWasteModel(ManifestApiService api, ClientesApiService clientes, VehiculosApiService vehiculos)
    {
        _api = api;
        _clientes = clientes;
        _vehiculos = vehiculos;
    }

    public List<VehiculoDto> Vehiculos { get; private set; } = [];

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

    [BindProperty] public string  PostalCode    { get; set; } = string.Empty;
    [BindProperty] public string  Street        { get; set; } = string.Empty;
    [BindProperty] public string  ExteriorNumber { get; set; } = string.Empty;
    [BindProperty] public string? InteriorNumber { get; set; }
    [BindProperty] public string  Colony        { get; set; } = string.Empty;
    [BindProperty] public string  Municipality  { get; set; } = string.Empty;
    [BindProperty] public string  State         { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? Email { get; set; }

    [BindProperty] public string    SafeHandlingInstructions { get; set; } = string.Empty;
    [BindProperty] public string    GeneratorResponsibleName { get; set; } = string.Empty;
    [BindProperty] public DateOnly? GeneratorSignDate        { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // ===================== RESIDUOS =====================
    [BindProperty]
    public List<HazardousResidueItem> Residues { get; set; } = new() { new HazardousResidueItem() };

    // ===================== TRANSPORTISTA =====================
    [BindProperty] public string    TransporterSocialReason        { get; set; } = string.Empty;
    [BindProperty] public string    TransporterPostalCode          { get; set; } = string.Empty;
    [BindProperty] public string    TransporterStreet              { get; set; } = string.Empty;
    [BindProperty] public string    TransporterExteriorNumber      { get; set; } = string.Empty;
    [BindProperty] public string?   TransporterInteriorNumber      { get; set; }
    [BindProperty] public string    TransporterColony              { get; set; } = string.Empty;
    [BindProperty] public string    TransporterMunicipality        { get; set; } = string.Empty;
    [BindProperty] public string    TransporterState               { get; set; } = string.Empty;
    [BindProperty] public string    TransporterPhone               { get; set; } = string.Empty;
    [BindProperty][EmailAddress] public string? TransporterEmail   { get; set; }
    [BindProperty] public string    TransporterAuthorizationNumber { get; set; } = string.Empty;
    [BindProperty] public string    TransporterSCTPermit           { get; set; } = string.Empty;
    [BindProperty] public string    VehicleType                    { get; set; } = string.Empty;
    [BindProperty] public string    VehiclePlate                   { get; set; } = string.Empty;
    [BindProperty] public string    TransportRoute                 { get; set; } = string.Empty;
    [BindProperty] public string    TransporterResponsibleName     { get; set; } = string.Empty;
    [BindProperty] public DateOnly? TransporterSignDate            { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // ===================== DESTINATARIO =====================
    [BindProperty] public string    ReceiverSocialReason           { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverPostalCode             { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverStreet                 { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverExteriorNumber         { get; set; } = string.Empty;
    [BindProperty] public string?   ReceiverInteriorNumber         { get; set; }
    [BindProperty] public string    ReceiverColony                 { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverMunicipality           { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverState                  { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverPhone                  { get; set; } = string.Empty;
    [BindProperty][EmailAddress] public string? ReceiverEmail      { get; set; }
    [BindProperty] public string    ReceiverAuthorizationNumber    { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverPersonName             { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverObservations           { get; set; } = string.Empty;
    [BindProperty] public string    ReceiverResponsibleName        { get; set; } = string.Empty;
    [BindProperty] public DateOnly? ReceiverSignDate               { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task OnGetAsync(int clienteId = 0, int contratoId = 0)
    {
        IdCliente  = clienteId;
        ContratoId = contratoId;

        var clienteTask   = clienteId > 0 ? _clientes.GetByIdAsync(clienteId) : Task.FromResult<ClienteDto?>(null);
        var vehiculosTask = _vehiculos.GetAllAsync();
        await Task.WhenAll(clienteTask, vehiculosTask);

        var cliente = clienteTask.Result;
        if (cliente is not null)
        {
            ClienteNombre                   = cliente.Name;
            SocialReason                    = cliente.Name;
            Street                          = cliente.Address ?? string.Empty;
            PhoneNumber                     = cliente.Phone ?? string.Empty;
            EnvironmentalRegistrationNumber = cliente.SemarnatNum ?? string.Empty;
        }

        Vehiculos = vehiculosTask.Result;
    }

    public async Task<IActionResult> OnPostAsync()
    {
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

