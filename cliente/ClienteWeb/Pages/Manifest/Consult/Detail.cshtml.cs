using ClienteWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Consult;

public class DetailModel : PageModel
{
    private readonly ManifestApiService _api;

    public DetailModel(ManifestApiService api) => _api = api;

    [BindProperty]
    public RegisterViewModel RegisterInput { get; set; } = new();

    public ManifestDetailViewModel Manifest { get; private set; } = new();

    public string? ApiError { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        try
        {
            var found = await _api.GetByIdAsync(id);
            if (found is null) return NotFound();
            Manifest = found;
            return Page();
        }
        catch (BillingApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
        catch (BillingApiException ex)
        {
            ApiError = ex.Message;
            return Page();
        }
        catch (Exception)
        {
            ApiError = "No se pudo conectar con el servidor. Verifique que el servicio esté en línea.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostRegisterAsync()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            return RedirectToPage("/Client_SimarUser/Client/Login");

        if (RegisterInput.SignedFile is null)
            ModelState.AddModelError(nameof(RegisterInput.SignedFile), "Debes subir el PDF firmado.");

        if (!ModelState.IsValid)
        {
            var current = await _api.GetByIdAsync(RegisterInput.Id);
            if (current is not null) Manifest = current;
            return Page();
        }

        try
        {
            await _api.UploadFirmaAsync(RegisterInput.Id, RegisterInput.SignedFile!);
            TempData["SuccessMessage"] = "El manifiesto ha sido marcado como completado con el PDF firmado.";
            return RedirectToPage(new { id = RegisterInput.Id });
        }
        catch (BillingApiException ex)
        {
            ApiError = ex.Message;
            var current = await _api.GetByIdAsync(RegisterInput.Id);
            if (current is not null) Manifest = current;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostTransitarAsync(string id)
    {
        ManifestDetailViewModel? found;
        try
        {
            found = await _api.GetByIdAsync(id);
        }
        catch (Exception)
        {
            TempData["ErrorTransitar"] = "No se pudo obtener el manifiesto del servidor.";
            return RedirectToPage(new { id });
        }

        if (found is null) return NotFound();
        if (found.Status != ManifestStatus.Borrador) return RedirectToPage(new { id });

        var faltantes = new List<string>();
        if (string.IsNullOrWhiteSpace(found.EnvironmentalRegistrationNumber))
            faltantes.Add("No. de registro ambiental");
        if (string.IsNullOrWhiteSpace(found.SocialReason))
            faltantes.Add("Razón social del generador");
        if (string.IsNullOrWhiteSpace(found.PhoneNumber))
            faltantes.Add("Teléfono del generador");
        if (string.IsNullOrWhiteSpace(found.TransporterSocialReason))
            faltantes.Add("Razón social del transportista");
        if (string.IsNullOrWhiteSpace(found.ReceiverSocialReason))
            faltantes.Add("Razón social del destinatario");

        if (faltantes.Count > 0)
        {
            TempData["ErrorTransitar"] = "Faltan campos obligatorios: " + string.Join(", ", faltantes) + ".";
            return RedirectToPage(new { id });
        }

        try
        {
            await _api.UpdateStatusAsync(id, "en_transito");
            TempData["SuccessMessage"] = $"El manifiesto {found.ManifestNumber} fue enviado a tránsito.";
        }
        catch (BillingApiException ex)
        {
            TempData["ErrorTransitar"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }
}

public class RegisterViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes subir el PDF firmado.")]
    public IFormFile? SignedFile { get; set; }

    public DateOnly SignedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public enum ManifestStatus
{
    Borrador,
    EnTransito,
    Completado
}

public class ManifestDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "especial";
    public ManifestStatus Status { get; set; } = ManifestStatus.Borrador;

    public string? SignedManifestFileName { get; set; }
    public DateOnly? SignedDate { get; set; }

    public string ManifestNumber { get; set; } = string.Empty;

    // GENERADOR
    public DateOnly? ManifestDate { get; set; }
    public TimeOnly? ManifestTime { get; set; }
    [Required(ErrorMessage = "El número de registro ambiental es obligatorio.")]
    public string EnvironmentalRegistrationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social del generador es obligatoria.")]
    public string SocialReason { get; set; } = string.Empty;

    // Especial
    public string Address { get; set; } = string.Empty;

    // Peligroso
    public string Street { get; set; } = string.Empty;
    public string ExteriorNumber { get; set; } = string.Empty;
    public string? InteriorNumber { get; set; }
    public string Colony { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? Email { get; set; }

    public DateOnly? GeneratorSignDate { get; set; }
    public string GeneratorResponsibleName { get; set; } = string.Empty;
    public string GeneratorObservations { get; set; } = string.Empty;
    public string SafeHandlingInstructions { get; set; } = string.Empty;

    public List<SpecialResidueItem> SpecialResidues { get; set; } = [];
    public List<HazardousResidueItem> HazardousResidues { get; set; } = [];

    // TRANSPORTISTA
    public string TransporterAuthorizationNumber { get; set; } = string.Empty;
    public string TransporterSCTPermit { get; set; } = string.Empty;
    [Required(ErrorMessage = "La razón social del transportista es obligatoria.")]
    public string TransporterSocialReason { get; set; } = string.Empty;

    // Especial
    public string TransporterAddress { get; set; } = string.Empty;

    // Peligroso
    public string TransporterStreet { get; set; } = string.Empty;
    public string TransporterExteriorNumber { get; set; } = string.Empty;
    public string? TransporterInteriorNumber { get; set; }
    public string TransporterColony { get; set; } = string.Empty;
    public string TransporterState { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? TransporterEmail { get; set; }

    public string TransporterPostalCode { get; set; } = string.Empty;
    public string TransporterMunicipality { get; set; } = string.Empty;
    public string TransporterPhone { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
    public string TransportRoute { get; set; } = string.Empty;
    public string TransporterObservations { get; set; } = string.Empty;
    public string TransporterResponsibleName { get; set; } = string.Empty;

    // Especial
    public DateOnly? TransporterDate { get; set; }
    public TimeOnly? TransporterTime { get; set; }

    // Peligroso
    public DateOnly? TransporterSignDate { get; set; }

    // DESTINATARIO
    public string ReceiverAuthorizationNumber { get; set; } = string.Empty;
    [Required(ErrorMessage = "La razón social del destinatario es obligatoria.")]
    public string ReceiverSocialReason { get; set; } = string.Empty;

    // Especial
    public string ReceiverAddress { get; set; } = string.Empty;
    public string DisposalType { get; set; } = string.Empty;
    public DateOnly? ReceiverDate { get; set; }

    // Peligroso
    public string ReceiverStreet { get; set; } = string.Empty;
    public string ReceiverExteriorNumber { get; set; } = string.Empty;
    public string? ReceiverInteriorNumber { get; set; }
    public string ReceiverColony { get; set; } = string.Empty;
    public string ReceiverState { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? ReceiverEmail { get; set; }
    public string ReceiverPersonName { get; set; } = string.Empty;
    public DateOnly? ReceiverSignDate { get; set; }

    public string ReceiverPostalCode { get; set; } = string.Empty;
    public string ReceiverMunicipality { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverObservations { get; set; } = string.Empty;
    public string ReceiverResponsibleName { get; set; } = string.Empty;

    // URL completa al PDF firmado (pasa por el gateway)
    public string? FirmaUrl { get; set; }
}

public class SpecialResidueItem
{
    public string ResidueKey { get; set; } = string.Empty;
    public string ResidueName { get; set; } = string.Empty;
    public string ContainerType { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "kg";
}

public class HazardousResidueItem
{
    public string ResidueName { get; set; } = string.Empty;
    public bool IsCorrosive { get; set; }
    public bool IsReactive { get; set; }
    public bool IsExplosive { get; set; }
    public bool IsToxic { get; set; }
    public bool IsFlammable { get; set; }
    public bool IsBiological { get; set; }
    public bool IsMutagenic { get; set; }
    public string ContainerType { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;
    public decimal AmountKg { get; set; }
    public bool? HasLabel { get; set; }
}
