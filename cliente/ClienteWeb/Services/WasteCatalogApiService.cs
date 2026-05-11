using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class WasteCatalogApiService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public WasteCatalogApiService(HttpClient http) => _http = http;

    // ── READ ──────────────────────────────────────────────────────────────────

    public async Task<WasteCatalogPagedResult> GetAllAsync(
        string? search = null,
        string? type   = null,
        int     page   = 1,
        int     size   = 50)
    {
        var url = $"?pageNumber={page}&pageSize={size}";
        if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(type))   url += $"&type={type}";

        try
        {
            var result = await _http.GetFromJsonAsync<WasteCatalogPagedResult>(url, _opts);
            return result ?? new WasteCatalogPagedResult();
        }
        catch
        {
            return new WasteCatalogPagedResult();
        }
    }

    public async Task<WasteCatalogItemDto?> GetByIdAsync(int id)
    {
        try { return await _http.GetFromJsonAsync<WasteCatalogItemDto>($"{id}", _opts); }
        catch { return null; }
    }

    // ── WRITE ─────────────────────────────────────────────────────────────────

    public async Task<(bool ok, string? error, WasteCatalogItemDto? item)> CreateAsync(WasteCatalogUpsertDto dto)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("", dto);
            if (resp.IsSuccessStatusCode)
            {
                var item = await resp.Content.ReadFromJsonAsync<WasteCatalogItemDto>(_opts);
                return (true, null, item);
            }
            var body = await resp.Content.ReadAsStringAsync();
            return (false, ParseError(body) ?? resp.ReasonPhrase, null);
        }
        catch (Exception ex) { return (false, ex.Message, null); }
    }

    public async Task<(bool ok, string? error)> UpdateAsync(int id, WasteCatalogUpsertDto dto)
    {
        try
        {
            var resp = await _http.PutAsJsonAsync($"{id}", dto);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, ParseError(body) ?? resp.ReasonPhrase);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    public async Task<(bool ok, string? error)> DeleteAsync(int id)
    {
        try
        {
            var resp = await _http.DeleteAsync($"{id}");
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, ParseError(body) ?? resp.ReasonPhrase);
        }
        catch (Exception ex) { return (false, ex.Message); }
    }

    private static string? ParseError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("message", out var m)) return m.GetString();
        }
        catch { }
        return null;
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public class WasteCatalogPagedResult
{
    [JsonPropertyName("items")]      public List<WasteCatalogItemDto> Items      { get; set; } = [];
    [JsonPropertyName("totalCount")] public int                       TotalCount { get; set; }
    [JsonPropertyName("pageNumber")] public int                       PageNumber { get; set; } = 1;
    [JsonPropertyName("pageSize")]   public int                       PageSize   { get; set; } = 50;
}

public class WasteCatalogItemDto
{
    [JsonPropertyName("id")]              public int     Id              { get; set; }
    [JsonPropertyName("code")]            public string  Code            { get; set; } = "";
    [JsonPropertyName("name")]            public string  Name            { get; set; } = "";
    [JsonPropertyName("type")]            public string  Type            { get; set; } = "";
    [JsonPropertyName("description")]     public string? Description     { get; set; }
    [JsonPropertyName("isCorrosive")]     public bool?   IsCorrosive     { get; set; }
    [JsonPropertyName("isReactive")]      public bool?   IsReactive      { get; set; }
    [JsonPropertyName("isExplosive")]     public bool?   IsExplosive     { get; set; }
    [JsonPropertyName("isToxic")]         public bool?   IsToxic         { get; set; }
    [JsonPropertyName("isFlammable")]     public bool?   IsFlammable     { get; set; }
    [JsonPropertyName("isBiological")]    public bool?   IsBiological    { get; set; }
    [JsonPropertyName("isMutagenic")]     public bool?   IsMutagenic     { get; set; }
    [JsonPropertyName("physicalState")]   public string? PhysicalState   { get; set; }
    [JsonPropertyName("storageForm")]     public string? StorageForm     { get; set; }
    [JsonPropertyName("lgpgirCategory")]  public string? LgpgirCategory  { get; set; }
    [JsonPropertyName("validUnits")]      public string  ValidUnits      { get; set; } = "kg";
    [JsonPropertyName("units")]           public List<string> Units      { get; set; } = [];

    public string TypeLabel    => Type == "peligroso" ? "Peligroso (RP)" : "Manejo Especial (RME)";
    public string TypeBadgeCss => Type == "peligroso" ? "danger" : "warning";

    public string CretiString
    {
        get
        {
            var flags = new List<string>();
            if (IsCorrosive  == true) flags.Add("C");
            if (IsReactive   == true) flags.Add("R");
            if (IsExplosive  == true) flags.Add("E");
            if (IsToxic      == true) flags.Add("T");
            if (IsFlammable  == true) flags.Add("I");
            if (IsBiological == true) flags.Add("B");
            if (IsMutagenic  == true) flags.Add("M");
            return flags.Count > 0 ? string.Join(", ", flags) : "—";
        }
    }
}

public class WasteCatalogUpsertDto
{
    public string  Code           { get; set; } = "";
    public string  Name           { get; set; } = "";
    public string  Type           { get; set; } = "peligroso";
    public string? Description    { get; set; }
    public bool?   IsCorrosive    { get; set; }
    public bool?   IsReactive     { get; set; }
    public bool?   IsExplosive    { get; set; }
    public bool?   IsToxic        { get; set; }
    public bool?   IsFlammable    { get; set; }
    public bool?   IsBiological   { get; set; }
    public bool?   IsMutagenic    { get; set; }
    public string? PhysicalState  { get; set; }
    public string? StorageForm    { get; set; }
    public string? LgpgirCategory { get; set; }
    public string  ValidUnits     { get; set; } = "kg";
}
