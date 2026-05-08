using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class ContratosApiService
{
    private readonly HttpClient _http;

    public ContratosApiService(HttpClient http) => _http = http;

    public async Task<List<ContratoDto>> GetAllAsync()
    {
        try
        {
            var list = await _http.GetFromJsonAsync<List<ContratoDto>>("api/contracts");
            return list ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ContratoDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ContratoDto>($"api/contracts/{id}");
        }
        catch
        {
            return null;
        }
    }
}

public class ContratoDto
{
    [JsonPropertyName("id")]             public int Id { get; set; }
    [JsonPropertyName("folio")]          public string Folio { get; set; } = "";
    [JsonPropertyName("clientId")]       public int ClientId { get; set; }
    [JsonPropertyName("status")]         public string Status { get; set; } = "";
    [JsonPropertyName("createdAt")]      public DateTime CreatedAt { get; set; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; set; }
}
