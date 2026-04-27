namespace API_WasteCatalog.DTOs;

public class WasteTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }

    // CRETIB (hazardous only)
    public bool? IsCorrosive { get; set; }
    public bool? IsReactive { get; set; }
    public bool? IsExplosive { get; set; }
    public bool? IsToxic { get; set; }
    public bool? IsFlammable { get; set; }
    public bool? IsBiological { get; set; }
    public bool? IsMutagenic { get; set; }
    public string? PhysicalState { get; set; }

    // Category (RME only)
    public string? LgpgirCategory { get; set; }

    public string ValidUnits { get; set; } = string.Empty;

    /// <summary>Already-split units list for easy UI consumption.</summary>
    public IEnumerable<string> Units =>
        ValidUnits.Split(',', StringSplitOptions.RemoveEmptyEntries);
}
