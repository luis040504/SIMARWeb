using System.ComponentModel.DataAnnotations;

namespace API_WasteCatalog.DTOs;

public class WasteTypeUpsertDto
{
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>"peligroso" | "especial"</summary>
    [Required]
    [RegularExpression("peligroso|especial", ErrorMessage = "Type must be 'peligroso' or 'especial'.")]
    public string Type { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // ── CRETIB (only for peligroso) ──
    public bool? IsCorrosive { get; set; }
    public bool? IsReactive { get; set; }
    public bool? IsExplosive { get; set; }
    public bool? IsToxic { get; set; }
    public bool? IsFlammable { get; set; }
    public bool? IsBiological { get; set; }
    public bool? IsMutagenic { get; set; }

    /// <summary>"solido" | "liquido" | "gaseoso" | "pastoso"</summary>
    [MaxLength(20)]
    public string? PhysicalState { get; set; }

    /// <summary>How the waste is stored. E.g. "A granel bajo techo", "Contenedor hermético"</summary>
    [MaxLength(100)]
    public string? StorageForm { get; set; }

    /// <summary>LGPGIR category (only for especial)</summary>
    [MaxLength(100)]
    public string? LgpgirCategory { get; set; }

    /// <summary>Comma-separated valid units. E.g. "kg,ton,lt"</summary>
    [Required]
    [MaxLength(50)]
    public string ValidUnits { get; set; } = "kg";
}
