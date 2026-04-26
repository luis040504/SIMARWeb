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
        // Deriva los datos desde DetailModel para mantener sincronía de estados
        var allManifests = DetailModel.SampleData.Select(d => new ManifestSummary
        {
            Id              = d.Id,
            ManifestNumber  = d.ManifestNumber,
            Type            = d.Type,
            Status          = d.Status switch
            {
                ManifestStatus.EnTransito => "en_transito",
                ManifestStatus.Completado => "completado",
                _                         => "borrador"
            },
            SocialReason    = d.SocialReason,
            Municipality    = d.Municipality,
            ManifestDate    = d.ManifestDate ?? d.GeneratorSignDate
                              ?? DateOnly.FromDateTime(DateTime.Today),
            ResidueSummary  = d.Type == "especial"
                ? string.Join(", ", d.SpecialResidues.Select(r => $"{r.ResidueName} ({r.Weight} {r.Unit})"))
                : string.Join(", ", d.HazardousResidues.Select(r => $"{r.ResidueName} ({r.AmountKg} kg)")),
            TransporterName = d.TransporterSocialReason
        });

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
