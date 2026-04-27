namespace API_WasteCatalog.Models;

public class WasteType
{
    public int Id { get; set; }

    /// <summary>Unique regulatory code. E.g. "RP-RPBI-001", "RME-CAR-001"</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>"peligroso" (hazardous / RP) | "especial" (special handling / RME)</summary>
    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    // ── CRETIB classification — only applies to hazardous waste (null for RME) ──
    public bool? IsCorrosive { get; set; }
    public bool? IsReactive { get; set; }
    public bool? IsExplosive { get; set; }
    public bool? IsToxic { get; set; }
    public bool? IsFlammable { get; set; }
    public bool? IsBiological { get; set; }
    public bool? IsMutagenic { get; set; }

    /// <summary>"solido" | "liquido" | "gaseoso" | "pastoso" — only for hazardous waste</summary>
    public string? PhysicalState { get; set; }

    // ── LGPGIR category — only applies to RME (null for RP) ──
    public string? LgpgirCategory { get; set; }

    /// <summary>Comma-separated list of valid units. E.g. "kg,ton,lt"</summary>
    public string ValidUnits { get; set; } = "kg";

    public bool IsActive { get; set; } = true;
}
