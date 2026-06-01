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
    private readonly EmpleadosApiService _empleados;

    public SpecialWasteModel(ManifestApiService api, ClientesApiService clientes, VehiculosApiService vehiculos, ContratosApiService contratos, EmpleadosApiService empleados)
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
    public string? PostalCode { get; set; }

    [BindProperty]
    public string? Municipality { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? Email { get; set; }

    [BindProperty][DataType(DataType.Date)]
    public DateOnly ManifestDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty][DataType(DataType.Time)]
    public TimeOnly ManifestTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [BindProperty]
    public string? GeneratorObservations { get; set; }

    [BindProperty]
    public string? GeneratorResponsibleName { get; set; }

    [BindProperty]
    public List<ResidueItem> Residues { get; set; } = new() { new ResidueItem() };

    // ===================== TRANSPORTISTA =====================
    [BindProperty] public string? TransporterAuthorizationNumber { get; set; }
    [BindProperty] public string? TransporterSocialReason        { get; set; }
    [BindProperty] public string? TransporterAddress             { get; set; }
    [BindProperty] public string? TransporterPostalCode          { get; set; }
    [BindProperty] public string? TransporterMunicipality        { get; set; }
    [BindProperty] public string? TransporterPhone               { get; set; }
    [BindProperty][DataType(DataType.Date)] public DateOnly? TransporterDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty][DataType(DataType.Time)] public TimeOnly? TransporterTime { get; set; }
    [BindProperty] public string? VehicleType                    { get; set; }
    [BindProperty] public string? VehiclePlate                   { get; set; }
    [BindProperty] public string? DriverName                     { get; set; }
    [BindProperty] public string? DriverLicense                  { get; set; }
    [BindProperty] public string? TransportRoute                 { get; set; }
    [BindProperty] public string? TransporterObservations        { get; set; }
    [BindProperty] public string? TransporterResponsibleName     { get; set; }

    // ===================== DESTINATARIO =====================
    [BindProperty] public string? ReceiverAuthorizationNumber    { get; set; }
    [BindProperty] public string? ReceiverSocialReason           { get; set; }
    [BindProperty] public string? ReceiverAddress                { get; set; }
    [BindProperty] public string? ReceiverPostalCode             { get; set; }
    [BindProperty] public string? ReceiverMunicipality           { get; set; }
    [BindProperty] public string? ReceiverPhone                  { get; set; }
    [BindProperty][DataType(DataType.Date)] public DateOnly? ReceiverDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [BindProperty][DataType(DataType.Time)] public TimeOnly? ReceiverTime { get; set; }
    [BindProperty] public string? DisposalType                   { get; set; }
    [BindProperty] public string? ReceiverObservations           { get; set; }
    [BindProperty] public string? ReceiverResponsibleName        { get; set; }

    public async Task<IActionResult> OnGetAsync(int clienteId = 0, int contratoId = 0)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        IdCliente  = clienteId;
        ContratoId = contratoId;

        var clienteTask      = clienteId > 0  ? _clientes.GetByIdAsync(clienteId)           : Task.FromResult<ClienteDto?>(null);
        var vehiculosTask    = _vehiculos.GetAllAsync();
        var manifestDataTask = contratoId > 0 ? _contratos.GetManifestDataAsync(contratoId) : Task.FromResult<ContratoManifestDataDto?>(null);
        var choferesTask     = _empleados.GetChoferesAsync();
        await Task.WhenAll(clienteTask, vehiculosTask, manifestDataTask, choferesTask);

        var cliente      = clienteTask.Result;
        var manifestData = manifestDataTask.Result;

        if (cliente is not null)
        {
            ClienteNombre                   = cliente.Name;
            SocialReason                    = cliente.Name;
            Address                         = cliente.Address ?? string.Empty;
            PhoneNumber                     = cliente.Phone ?? string.Empty;
            Email                           = cliente.ContactEmail;
            EnvironmentalRegistrationNumber = cliente.SedemaNum ?? cliente.SemarnatNum ?? string.Empty;
        }

        if (manifestData is not null)
        {
            ContratoFolio = manifestData.Folio;

            var residuosEspeciales = manifestData.Wastes
                .Where(w => w.Type.Equals("especial", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (residuosEspeciales.Count > 0)
                Residues = residuosEspeciales
                    .Select(w => new ResidueItem
                    {
                        ResidueKey  = w.Code,
                        ResidueName = w.Name,
                        Unit        = string.IsNullOrWhiteSpace(w.Unit) ? "kg" : w.Unit
                    })
                    .ToList();

            // Ubicación del generador desde servicio de recolección
            var collection = manifestData.Services
                .FirstOrDefault(s => s.Activity.Equals("collection", StringComparison.OrdinalIgnoreCase));
            if (collection?.Location is { } loc)
            {
                Address      = loc.Street ?? Address;
                PostalCode   = loc.Cp;
                Municipality = loc.Municipality;
            }

            // Ubicación del destinatario desde servicio de disposición final
            var disposal = manifestData.Services
                .FirstOrDefault(s => s.Activity.Equals("final_disposal", StringComparison.OrdinalIgnoreCase));
            if (disposal?.Location is { } dLoc)
            {
                ReceiverAddress      = dLoc.Street;
                ReceiverPostalCode   = dLoc.Cp;
                ReceiverMunicipality = dLoc.Municipality;
            }
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
            var id = await _api.CreateSpecialAsync(this);
            TempData["SuccessMessage"] = "Manifiesto guardado exitosamente como borrador.";
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
    public string? ResidueKey       { get; set; }
    public string? ResidueName      { get; set; }
    public string? ContainerType    { get; set; }
    public string? ContainerCapacity { get; set; }
    public decimal Weight           { get; set; }
    public string Unit              { get; set; } = "kg";
}
