using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Generate;

public class HazardousWasteModel : PageModel
{
    // ── Datos generales ────────────────────────────────────────────────────

    [BindProperty]
    public string ManifestNumber { get; set; } = GenerateManifestNumber();

    // ── Generador ──────────────────────────────────────────────────────────

    [BindProperty]
    [Required(ErrorMessage = "El número de registro ambiental es obligatorio.")]
    public string EnvironmentalRegistrationNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "La razón social es obligatoria.")]
    public string SocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string PostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string Street { get; set; } = string.Empty;

    [BindProperty]
    public string ExteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string InteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string Colony { get; set; } = string.Empty;

    [BindProperty]
    public string Municipality { get; set; } = string.Empty;

    [BindProperty]
    public string State { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly ManifestDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public TimeOnly ManifestTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [BindProperty]
    public string SafeHandlingInstructions { get; set; } = string.Empty;

    [BindProperty]
    public string RegulatoryFramework { get; set; } = string.Empty;

    [BindProperty]
    public string GeneratorResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? GeneratorSignDate { get; set; }

    // ── Lista de residuos peligrosos ───────────────────────────────────────

    [BindProperty]
    public List<HazardousResidueItem> Residues { get; set; } = new()
    {
        new HazardousResidueItem()
    };

    // ── Transportista ──────────────────────────────────────────────────────

    [BindProperty]
    public string TransporterSocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterAuthorizationNumber { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterSCTPermit { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterPostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterStreet { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterExteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterMunicipality { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterState { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterPhone { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterEmail { get; set; } = string.Empty;

    [BindProperty]
    public string VehicleType { get; set; } = string.Empty;

    [BindProperty]
    public string VehiclePlate { get; set; } = string.Empty;

    [BindProperty]
    public string TransportRoute { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public string DriverName { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? TransporterReceptionDate { get; set; }

    // ── Destinatario ───────────────────────────────────────────────────────

    [BindProperty]
    public string ReceiverSocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverAuthorizationNumber { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverStreet { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverExteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverInteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverColony { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverMunicipality { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverState { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPhone { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverEmail { get; set; } = string.Empty;

    [BindProperty]
    public string DisposalType { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? ReceiverDate { get; set; }

    [BindProperty]
    public string ReceiverObservations { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPersonName { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverResponsibleName { get; set; } = string.Empty;

    // ── Archivos ───────────────────────────────────────────────────────────

    public IFormFile? SignedManifestFile { get; set; }
    public IFormFile? ReceiverSealFile { get; set; }

    // ── Handlers ──────────────────────────────────────────────────────────

    public void OnGet() { }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        // TODO: Enviar datos al backend / API de SIMAR

        TempData["SuccessMessage"] = $"Manifiesto {ManifestNumber} generado correctamente.";
        return RedirectToPage("/Index");
    }

    private static string GenerateManifestNumber()
    {
        int year = DateTime.Today.Year;
        int seq  = Random.Shared.Next(1, 9999);
        return $"{seq:D3}/{year}";
    }
}

/// <summary>Residuo peligroso con clasificación CRETIBM.</summary>
public class HazardousResidueItem
{
    public string ResidueName       { get; set; } = string.Empty;

    // Clasificación CRETIBM
    public bool IsCorrosive         { get; set; }
    public bool IsReactive          { get; set; }
    public bool IsExplosive         { get; set; }
    public bool IsToxic             { get; set; }
    public bool IsFlammable         { get; set; }
    public bool IsBiological        { get; set; }
    public bool IsMutagenic         { get; set; }

    // Envase
    public string Container         { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;

    // Cantidad y etiqueta
    public float  AmountKg          { get; set; }
    public bool?  HasLabel          { get; set; }
}
