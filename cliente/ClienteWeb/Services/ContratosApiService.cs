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

    public async Task<ContratoManifestDataDto?> GetManifestDataAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ContratoManifestDataDto>($"api/contracts/{id}/manifest-data");
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
    // Estructura nueva (igual que cotización): services[].wastes[].type
    [JsonPropertyName("wastes")]         public List<ContratoWasteItemDto> Wastes { get; set; } = [];
}

public class ContratoWasteItemDto
{
    [JsonPropertyName("name")]           public string Name { get; set; } = "";
    [JsonPropertyName("type")]           public string Type { get; set; } = "";
    [JsonPropertyName("classification")] public string Classification { get; set; } = "";
    [JsonPropertyName("clave")]          public string Clave { get; set; } = "";
    [JsonPropertyName("quantity")]       public decimal Quantity { get; set; }
    [JsonPropertyName("unit")]           public string Unit { get; set; } = "";
}

// DTOs para GET /api/contracts/{id}/manifest-data
public class ContratoManifestDataDto
{
    [JsonPropertyName("contractId")]   public int ContractId { get; set; }
    [JsonPropertyName("quotationId")]  public int QuotationId { get; set; }
    [JsonPropertyName("folio")]        public string Folio { get; set; } = "";
    [JsonPropertyName("clientId")]     public int ClientId { get; set; }
    [JsonPropertyName("clientName")]   public string ClientName { get; set; } = "";
    [JsonPropertyName("clientRfc")]    public string ClientRfc { get; set; } = "";
    [JsonPropertyName("status")]       public string Status { get; set; } = "";
    [JsonPropertyName("frequency")]    public string Frequency { get; set; } = "";
    [JsonPropertyName("total")]        public decimal Total { get; set; }
    [JsonPropertyName("wastes")]       public List<ContratoManifestWasteDto> Wastes { get; set; } = [];
    // Servicios con ubicación por actividad — pendiente exposición en backend
    [JsonPropertyName("services")]     public List<ContratoManifestServiceDto> Services { get; set; } = [];
}

public class ContratoManifestWasteDto
{
    [JsonPropertyName("code")]           public string Code { get; set; } = "";
    [JsonPropertyName("name")]           public string Name { get; set; } = "";
    [JsonPropertyName("type")]           public string Type { get; set; } = "";   // "peligroso" | "especial"
    [JsonPropertyName("unit")]           public string Unit { get; set; } = "";
    [JsonPropertyName("serviceAddress")] public string ServiceAddress { get; set; } = "";
}

// activity: "collection" → ubicación del generador
// activity: "final_disposal" → ubicación del destinatario
public class ContratoManifestServiceDto
{
    [JsonPropertyName("id")]       public string Id { get; set; } = "";
    [JsonPropertyName("activity")] public string Activity { get; set; } = "";
    [JsonPropertyName("location")] public ContratoServiceLocationDto? Location { get; set; }
}

public class ContratoServiceLocationDto
{
    [JsonPropertyName("cp")]           public string? Cp { get; set; }
    [JsonPropertyName("street")]       public string? Street { get; set; }
    [JsonPropertyName("municipality")] public string? Municipality { get; set; }
    [JsonPropertyName("neighborhood")] public string? Neighborhood { get; set; }
    [JsonPropertyName("state")]        public string? State { get; set; }
}
