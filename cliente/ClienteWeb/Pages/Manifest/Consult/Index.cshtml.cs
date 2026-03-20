using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? FilterManifestNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterSocialReason { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterType { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FilterDateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FilterDateTo { get; set; }

    public List<ManifestSummary> Results { get; private set; } = new();

    public void OnGet()
    {
        var allManifests = SampleData;

        Results = allManifests
            .Where(m =>
                (string.IsNullOrWhiteSpace(FilterManifestNumber) ||
                 m.ManifestNumber.Contains(FilterManifestNumber, StringComparison.OrdinalIgnoreCase))
             && (string.IsNullOrWhiteSpace(FilterSocialReason) ||
                 m.SocialReason.Contains(FilterSocialReason, StringComparison.OrdinalIgnoreCase))
             && (string.IsNullOrWhiteSpace(FilterType) ||
                 m.Type.Equals(FilterType, StringComparison.OrdinalIgnoreCase))
             && (string.IsNullOrWhiteSpace(FilterStatus) ||
                 m.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase))
             && (FilterDateFrom == null || m.ManifestDate >= FilterDateFrom)
             && (FilterDateTo == null || m.ManifestDate <= FilterDateTo)
            )
            .OrderByDescending(m => m.ManifestDate)
            .ThenByDescending(m => m.ManifestNumber)
            .ToList();
    }

    public static List<ManifestSummary> SampleData { get; } = BuildSampleData();

    private static List<ManifestSummary> BuildSampleData() =>
    [
        new()
        {
            Id = "1",
            ManifestNumber = "009/2026",
            Type = "especial",
            Status = "completado",
            SocialReason = "Cementos Moctezuma S.A. de C.V.",
            Municipality = "Apazapan",
            ManifestDate = new DateOnly(2026, 2, 26),
            ResidueSummary = "Otros residuos inorgánicos (680 kg)",
            TransporterName = "Sistemas en Manejo y Administración de Residuos S.A. de C.V."
        },
        new()
        {
            Id = "2",
            ManifestNumber = "002/2026",
            Type = "peligroso",
            Status = "en_transito",
            SocialReason = "Cementos Moctezuma S.A. de C.V.",
            Municipality = "Apazapan",
            ManifestDate = new DateOnly(2026, 2, 26),
            ResidueSummary = "Objetos punzocortantes (250 kg), Residuos no anatómicos (640 kg)",
            TransporterName = "Sistemas en Manejo y Administración de Residuos S.A. de C.V."
        },
        new()
        {
            Id = "3",
            ManifestNumber = "015/2026",
            Type = "especial",
            Status = "impreso",
            SocialReason = "Industrias Veracruz S.A. de C.V.",
            Municipality = "Xalapa",
            ManifestDate = new DateOnly(2026, 1, 15),
            ResidueSummary = "Residuos de construcción (1.2 ton)",
            TransporterName = "SIMAR"
        },
        new()
        {
            Id = "4",
            ManifestNumber = "005/2026",
            Type = "peligroso",
            Status = "borrador",
            SocialReason = "Hospital Regional IMSS",
            Municipality = "Veracruz",
            ManifestDate = new DateOnly(2026, 3, 1),
            ResidueSummary = "Sangre y fluidos corporales (120 kg)",
            TransporterName = "Exitum / SIMAR"
        }
    ];
}

public class ManifestSummary
{
    public string Id { get; set; } = string.Empty;
    public string ManifestNumber { get; set; } = string.Empty;
    public string Type { get; set; } = "especial";
    public string Status { get; set; } = "borrador";
    public string SocialReason { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public DateOnly ManifestDate { get; set; }
    public string ResidueSummary { get; set; } = string.Empty;
    public string TransporterName { get; set; } = string.Empty;
}