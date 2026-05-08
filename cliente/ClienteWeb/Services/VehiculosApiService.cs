using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class VehiculosApiService
{
    private readonly HttpClient _http;

    public VehiculosApiService(HttpClient http) => _http = http;

    public async Task<List<VehiculoDto>> GetAllAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<VehiculosResponse>("vehiculos");
            return resp?.Data ?? [];
        }
        catch
        {
            return [];
        }
    }

    private class VehiculosResponse
    {
        [JsonPropertyName("data")] public List<VehiculoDto>? Data { get; set; }
    }
}

public class VehiculoDto
{
    [JsonPropertyName("id")]                public int    Id                { get; set; }
    [JsonPropertyName("numero_economico")]  public string? NumeroEconomico  { get; set; }
    [JsonPropertyName("marca")]             public string  Marca            { get; set; } = "";
    [JsonPropertyName("modelo")]            public string  Modelo           { get; set; } = "";
    [JsonPropertyName("anio")]              public int?    Anio             { get; set; }
    [JsonPropertyName("placas")]            public string  Placas           { get; set; } = "";
    [JsonPropertyName("licencia_requerida")] public string? LicenciaRequerida { get; set; }
    [JsonPropertyName("tipo_desecho")]      public string? TipoDesecho      { get; set; }

    public string DisplayLabel =>
        $"{Marca} {Modelo}{(Anio.HasValue ? $" ({Anio})" : "")} — {Placas}";
}
