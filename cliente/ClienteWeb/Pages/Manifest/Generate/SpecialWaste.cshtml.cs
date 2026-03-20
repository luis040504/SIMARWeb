using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.Pages.Manifest.Generate;

public class SpecialWasteModel : PageModel
{
    [BindProperty]
    public string ManifestSerial { get; set; } = GenerateManifestSerial(); // OS-990226

    [BindProperty]
    public string ManifestNumber { get; set; } = GenerateManifestNumber(); // 009/2026

    // ===================== GENERADOR =====================
    [BindProperty]
    [Required]
    public string EnvironmentalRegistrationNumber { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string SocialReason { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    public string PostalCode { get; set; } = string.Empty;

    [BindProperty]
    public string Municipality { get; set; } = string.Empty;

    [BindProperty]
    public string PhoneNumber { get; set; } = string.Empty;

    [BindProperty]
    public DateOnly ManifestDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public TimeOnly ManifestTime { get; set; } = TimeOnly.FromDateTime(DateTime.Now);

    [BindProperty]
    public string GeneratorObservations { get; set; } = string.Empty;

    [BindProperty]
    public string GeneratorResponsibleName { get; set; } = string.Empty;

    [BindProperty]
    public List<ResidueItem> Residues { get; set; } = new()
    {
        new ResidueItem()
    };

    // ===================== TRANSPORTISTA =====================
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
    public DateOnly? TransporterDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public TimeOnly? TransporterTime { get; set; }

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

    // ===================== DESTINATARIO =====================
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
    public DateOnly? ReceiverDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public TimeOnly? ReceiverTime { get; set; }

    [BindProperty]
    public string DisposalType { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverObservations { get; set; } = string.Empty;

    [BindProperty]
    public string ReceiverResponsibleName { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        TempData["SuccessMessage"] = $"Manifiesto {ManifestNumber} generado correctamente.";
        return RedirectToPage("/Index");
    }

    private static string GenerateManifestSerial()
    {
        return $"OS-{Random.Shared.Next(1, 999999):D6}";
    }

    private static string GenerateManifestNumber()
    {
        int seq = Random.Shared.Next(1, 999);
        int year = DateTime.Today.Year;
        return $"{seq:D3}/{year}";
    }
}

public class ResidueItem
{
    public string ResidueKey { get; set; } = string.Empty;
    public string ResidueName { get; set; } = string.Empty;
    public string ContainerType { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "kg";
}