using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Generate;

public class SpecialWasteModel : PageModel
{

    [BindProperty]
    public string ManifestNumber { get; set; } = GenerateManifestNumber();


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
    public DateOnly ManifestDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public TimeOnly ManifestTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [BindProperty]
    public string GeneratorObservations { get; set; } = string.Empty;

    [BindProperty]
    public string RegulatoryFramework { get; set; } = string.Empty;

    [BindProperty]
    public string GeneratorResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    [EmailAddress(ErrorMessage = "Correo electrónico inválido.")]
    public string Email { get; set; } = string.Empty;


    [BindProperty]
    public List<ResidueItem> Residues { get; set; } = new()
    {
        new ResidueItem() // primera fila vacía por defecto
    };


    [BindProperty]
    public string TransporterAuthorizationNumber { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterSocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterAddress { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterPostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterMunicipality { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterPhone { get; set; } = string.Empty;

    [BindProperty]
    public string VehicleType { get; set; } = string.Empty;

    [BindProperty]
    public string VehiclePlate { get; set; } = string.Empty;

    [BindProperty]
    public string DriverName { get; set; } = string.Empty;

    [BindProperty]
    public string DriverLicense { get; set; } = string.Empty;

    [BindProperty]
    public string TransportRoute { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterObservations { get; set; } = string.Empty;

    [BindProperty]
    public string TransporterResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? TransporterReceptionDate { get; set; }

    [BindProperty]
    public string ReceiverAuthorizationNumber { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverSocialReason { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverAddress { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverMunicipality { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverPhone { get; set; } = string.Empty;

    [BindProperty]
    public string DisposalType { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly? ReceiverDate { get; set; }

    [BindProperty]
    public string ReceiverObservations { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverResponsibleName { get; set; } = string.Empty;


    public IFormFile? SignedManifestFile { get; set; }


    public void OnGet()
    {
    }

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
        return $"OS-{seq:D6}";
    }
}

/// <summary>Representa un renglón de residuo dentro del manifiesto de manejo especial.</summary>
public class ResidueItem
{
    public string ResidueKey         { get; set; } = string.Empty;
    public string ResidueName        { get; set; } = string.Empty;
    public string Container          { get; set; } = string.Empty;
    public string ContainerCapacity  { get; set; } = string.Empty;
    public float  Amount             { get; set; }
    public string Unit               { get; set; } = "kg";
}
