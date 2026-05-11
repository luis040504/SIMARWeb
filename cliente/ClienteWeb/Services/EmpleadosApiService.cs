using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class EmpleadosApiService
{
    private readonly HttpClient _http;

    public EmpleadosApiService(HttpClient http) => _http = http;

    public async Task<List<ChoferDto>> GetChoferesAsync()
    {
        try
        {
            var empleados = await _http.GetFromJsonAsync<List<EmpleadoApiDto>>("api/employees?role=chofer");
            if (empleados is null || empleados.Count == 0) return [];

            var tasks = empleados.Select(async e =>
            {
                try
                {
                    var detail = await _http.GetFromJsonAsync<EmpleadoDetailApiDto>($"api/employees/{e.UserId}");
                    return new ChoferDto
                    {
                        UserId        = e.UserId,
                        FullName      = e.FullName,
                        LicenseNumber = detail?.DriverInfo?.LicenseNumber ?? "",
                        LicenseType   = detail?.DriverInfo?.LicenseType  ?? ""
                    };
                }
                catch
                {
                    return new ChoferDto { UserId = e.UserId, FullName = e.FullName };
                }
            });

            return [.. await Task.WhenAll(tasks)];
        }
        catch
        {
            return [];
        }
    }

    // ─── DTOs internos ────────────────────────────────────────────────────────

    private class EmpleadoApiDto
    {
        [JsonPropertyName("userId")]        public Guid   UserId   { get; set; }
        [JsonPropertyName("fullName")]      public string FullName { get; set; } = "";
        [JsonPropertyName("professionalId")] public string ProfessionalId { get; set; } = "";
    }

    private class EmpleadoDetailApiDto
    {
        [JsonPropertyName("driverInfo")] public DriverInfoDto? DriverInfo { get; set; }
    }

    private class DriverInfoDto
    {
        [JsonPropertyName("licenseNumber")] public string LicenseNumber { get; set; } = "";
        [JsonPropertyName("licenseType")]   public string LicenseType   { get; set; } = "";
    }
}

public class ChoferDto
{
    public Guid   UserId        { get; set; }
    public string FullName      { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public string LicenseType   { get; set; } = "";
}
