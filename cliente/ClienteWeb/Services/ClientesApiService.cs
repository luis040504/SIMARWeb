using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class ClientesApiService
{
    private readonly HttpClient _http;

    public ClientesApiService(HttpClient http) => _http = http;

    public async Task<List<ClienteDto>> GetAllAsync()
    {
        try
        {
            var list = await _http.GetFromJsonAsync<List<ClienteDto>>("client/all");
            return list ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<ClienteDto?> GetByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ClienteDto>($"client/id/{id}");
        }
        catch
        {
            return null;
        }
    }
}

public class ClienteDto
{
    [JsonPropertyName("id")]               public int Id { get; set; }
    [JsonPropertyName("name")]             public string Name { get; set; } = "";
    [JsonPropertyName("phone")]            public string? Phone { get; set; }
    [JsonPropertyName("address")]          public string? Address { get; set; }
    [JsonPropertyName("rfc")]              public string? Rfc { get; set; }
    [JsonPropertyName("semarnatNum")]      public string? SemarnatNum { get; set; }
    [JsonPropertyName("status")]           public string Status { get; set; } = "";
}
