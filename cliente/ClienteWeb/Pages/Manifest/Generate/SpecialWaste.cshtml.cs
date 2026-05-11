using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Generate;

public class SpecialWasteModel : PageModel
{
    private readonly ManifestApiService _api;
    private readonly ClientesApiService _clientes;
    private readonly VehiculosApiService _vehiculos;
    private readonly ContratosApiService _contratos;

    public SpecialWasteModel(ManifestApiService api, ClientesApiService clientes, VehiculosApiService vehiculos, ContratosApiService contratos)
    {
        _api = api;
        _clientes = clientes;
        _vehiculos = vehiculos;
        _contratos = contratos;
    }

    public List<VehiculoDto> Vehiculos { get; private set; } = [];

    // ── Vinculación con contrato / cliente ────────────────────────────────────
    [BindProperty] public int IdCliente  { get; set; }
    [BindProperty] public int ContratoId { get; set; }

    // Nombre del cliente y folio del contrato, solo para mostrar en el encabezado
    public string ClienteNombre  { get; private set; } = string.Empty;
    public string ContratoFolio  { get; private set; } = string.Empty;

    // ===================== GENERADOR =====================
    [BindProperty]
    [Required(ErrorMessage = "El número de registro SEDEMA es obligatorio.")]
    public string EnvironmentalRegistrationNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "La razón social es obligatoria.")]
    public string SocialReason { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El domicilio es obligatorio.")]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    public string PostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string Municipality { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly ManifestDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public TimeOnly ManifestTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [BindProperty]
    public string GeneratorObservations { get; set; } = string.Empty;

    [BindProperty]
    public string GeneratorResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public List<ResidueItem> Residues { get; set; } = new() { new ResidueItem() };

    // ===================== TRANSPORTISTA =====================
    [BindProperty] public string TransporterAuthorizationNumber { get; set; } = string.Empty;
    [BindProperty] public string TransporterSocialReason        { get; set; } = string.Empty;
    [BindProperty] public string TransporterAddress             { get; set; } = string.Empty;
    [BindProperty] public string TransporterPostalCode          { get; set; } = string.Empty;
    [BindProperty] public string TransporterMunicipality        { get; set; } = string.Empty;
    [BindProperty] public string TransporterPhone               { get; set; } = string.Empty;
    [BindProperty] public DateOnly? TransporterDate             { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty] public TimeOnly? TransporterTime             { get; set; }
    [BindProperty] public string VehicleType                    { get; set; } = string.Empty;
    [BindProperty] public string VehiclePlate                   { get; set; } = string.Empty;
    [BindProperty] public string DriverName                     { get; set; } = string.Empty;
    [BindProperty] public string DriverLicense                  { get; set; } = string.Empty;
    [BindProperty] public string TransportRoute                 { get; set; } = string.Empty;
    [BindProperty] public string TransporterObservations        { get; set; } = string.Empty;
    [BindProperty] public string TransporterResponsibleName     { get; set; } = string.Empty;

    // ===================== DESTINATARIO =====================
    [BindProperty] public string ReceiverAuthorizationNumber    { get; set; } = string.Empty;
    [BindProperty] public string ReceiverSocialReason           { get; set; } = string.Empty;
    [BindProperty] public string ReceiverAddress                { get; set; } = string.Empty;
    [BindProperty] public string ReceiverPostalCode             { get; set; } = string.Empty;
    [BindProperty] public string ReceiverMunicipality           { get; set; } = string.Empty;
    [BindProperty] public string ReceiverPhone                  { get; set; } = string.Empty;
    [BindProperty] public DateOnly? ReceiverDate                { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty] public TimeOnly? ReceiverTime                { get; set; }
    [BindProperty] public string DisposalType                   { get; set; } = string.Empty;
    [BindProperty] public string ReceiverObservations           { get; set; } = string.Empty;
    [BindProperty] public string ReceiverResponsibleName        { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int clienteId = 0, int contratoId = 0)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        IdCliente  = clienteId;
        ContratoId = contratoId;

        var clienteTask   = clienteId > 0 ? _clientes.GetByIdAsync(clienteId) : Task.FromResult<ClienteDto?>(null);
        var vehiculosTask = _vehiculos.GetAllAsync();
        var contratoTask  = contratoId > 0 ? _contratos.GetDetailAsync(contratoId) : Task.FromResult<ContratoDetailDto?>(null);
        await Task.WhenAll(clienteTask, vehiculosTask, contratoTask);

        var cliente  = clienteTask.Result;
        var contrato = contratoTask.Result;

        if (cliente is not null)
        {
            ClienteNombre                   = cliente.Name;
            SocialReason                    = cliente.Name;
            Address                         = cliente.Address ?? string.Empty;
            PhoneNumber                     = cliente.Phone ?? string.Empty;
            EnvironmentalRegistrationNumber = cliente.SemarnatNum ?? string.Empty;
        }

        if (contrato?.Services is { Count: > 0 })
        {
            ContratoFolio = contrato.Folio;
            Residues = contrato.Services
                .Select(s => new ResidueItem
                {
                    ResidueName = s.WasteType,
                    Unit        = string.IsNullOrWhiteSpace(s.WasteUnit) ? "kg" : s.WasteUnit
                })
                .ToList();
        }

        Vehiculos = vehiculosTask.Result;
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
            var numeroManifiesto = await _api.CreateSpecialAsync(this);
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

public class ResidueItem
{
    public string ResidueKey      { get; set; } = string.Empty;
    public string ResidueName     { get; set; } = string.Empty;
    public string ContainerType   { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;
    public decimal Weight         { get; set; }
    public string Unit            { get; set; } = "kg";
}
