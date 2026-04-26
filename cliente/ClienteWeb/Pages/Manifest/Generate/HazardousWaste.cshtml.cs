using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Generate;

public class HazardousWasteModel : PageModel
{
    // ===================== ENCABEZADO =====================

    [BindProperty]
    public string ManifestRegistrationNumber { get; set; } = string.Empty; // Núm. de registro ambiental

    [BindProperty]
    public string ManifestNumber { get; set; } = GenerateManifestNumber(); // 002/2026

    [BindProperty]
    public string ManifestPage { get; set; } = "1/1";

    // ===================== GENERADOR =====================

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
    public string? InteriorNumber { get; set; }

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
    public string? Email { get; set; }

    [BindProperty]
    public string SafeHandlingInstructions { get; set; } = string.Empty;

    [BindProperty]
    public string GeneratorResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? GeneratorSignDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // ===================== RESIDUOS =====================

    [BindProperty]
    public List<HazardousResidueItem> Residues { get; set; } = new()
    {
        new HazardousResidueItem()
    };

    // ===================== TRANSPORTISTA =====================

    [BindProperty]
    public string TransporterSocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterPostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterStreet { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterExteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string? TransporterInteriorNumber { get; set; }

    [BindProperty]
    public string TransporterColony { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterMunicipality { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterState { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterPhone { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? TransporterEmail { get; set; }

    [BindProperty]
    public string TransporterAuthorizationNumber { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterSCTPermit { get; set; } = string.Empty;

    [BindProperty]
    public string VehicleType { get; set; } = string.Empty;

    [BindProperty]
    public string VehiclePlate { get; set; } = string.Empty;

    [BindProperty]
    public string TransportRoute { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? TransporterSignDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // ===================== DESTINATARIO =====================

    [BindProperty]
    public string ReceiverSocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverStreet { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverExteriorNumber { get; set; } = string.Empty;

    [BindProperty]
    public string? ReceiverInteriorNumber { get; set; }

    [BindProperty]
    public string ReceiverColony { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverMunicipality { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverState { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPhone { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string? ReceiverEmail { get; set; }

    [BindProperty]
    public string ReceiverAuthorizationNumber { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPersonName { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverObservations { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? ReceiverSignDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        TempData["SuccessMessage"] = $"Manifiesto {ManifestNumber} guardado como borrador. Puede editarlo y enviarlo a tránsito desde el listado.";
        return RedirectToPage("/Manifest/Consult/Index");
    }

    private static string GenerateManifestNumber()
    {
        int year = DateTime.Today.Year;
        int seq = Random.Shared.Next(1, 999);
        return $"{seq:D3}/{year}";
    }
}

public class HazardousResidueItem
{
    [Display(Name = "Nombre del residuo")]
    public string ResidueName { get; set; } = string.Empty;

    // CRETIBM
    public bool IsCorrosive { get; set; }
    public bool IsReactive { get; set; }
    public bool IsExplosive { get; set; }
    public bool IsToxic { get; set; }
    public bool IsFlammable { get; set; }
    public bool IsBiological { get; set; }
    public bool IsMutagenic { get; set; }

    // Envase
    public string ContainerType { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;

    // Cantidad
    public decimal AmountKg { get; set; }

    // Etiqueta
    public bool? HasLabel { get; set; }
}