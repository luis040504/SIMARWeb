namespace API_WasteCatalog.DTOs;

public class WasteFilterDto
{
    /// <summary>Filter by type: "peligroso" or "especial"</summary>
    public string? Type { get; set; }

    /// <summary>Search by name or code (contains, case-insensitive)</summary>
    public string? Search { get; set; }

    /// <summary>Filter biological-infectious waste only (RPBI)</summary>
    public bool? IsBiological { get; set; }

    /// <summary>Filter by LGPGIR category (RME only)</summary>
    public string? LgpgirCategory { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
