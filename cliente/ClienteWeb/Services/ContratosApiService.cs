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

    public async Task<ContratoDetailDto?> GetDetailAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ContratoDetailDto>($"api/contracts/{id}/detail");
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
    [JsonPropertyName("clientName")]     public string ClientName { get; set; } = "";
    [JsonPropertyName("clientAddress")]  public string ClientAddress { get; set; } = "";
    [JsonPropertyName("clientRfc")]      public string ClientRfc { get; set; } = "";
    [JsonPropertyName("status")]         public string Status { get; set; } = "";
    [JsonPropertyName("createdAt")]      public DateTime CreatedAt { get; set; }
    [JsonPropertyName("expirationDate")] public DateTime? ExpirationDate { get; set; }
}

public class ContratoDetailDto : ContratoDto
{
    [JsonPropertyName("services")] public List<ContratoServiceItemDto> Services { get; set; } = [];
}

public class ContratoServiceItemDto
{
    [JsonPropertyName("id")]             public int Id { get; set; }
    [JsonPropertyName("contractId")]     public int ContractId { get; set; }
    [JsonPropertyName("wasteType")]      public string WasteType { get; set; } = "";
    [JsonPropertyName("wasteUnit")]      public string WasteUnit { get; set; } = "";
    [JsonPropertyName("frequency")]      public string Frequency { get; set; } = "";
    [JsonPropertyName("serviceAddress")] public string ServiceAddress { get; set; } = "";
}
