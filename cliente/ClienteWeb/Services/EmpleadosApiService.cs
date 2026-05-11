using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class EmpleadosApiService
{
    private readonly HttpClient _http;

    public EmpleadosApiService(HttpClient http) => _http = http;

    // Obtener empleados por rol (chofer, tecnico, etc.)
    public async Task<List<EmpleadoItemDto>> GetByRoleAsync(string role)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<EmpleadoDto>>($"api/employees?role={role}");
            return response?.Select(e => new EmpleadoItemDto
            {
                UserId = e.UserId,
                FullName = e.FullName,
                RoleName = e.Role?.RoleName ?? role,
                ProfessionalId = e.ProfessionalId
            }).ToList() ?? new List<EmpleadoItemDto>();
        }
        catch
        {
            // Fallback para desarrollo
            return role switch
            {
                "chofer" => new List<EmpleadoItemDto>
                {
                    new() { UserId = Guid.NewGuid(), FullName = "Carlos Hernández", RoleName = "chofer" },
                    new() { UserId = Guid.NewGuid(), FullName = "Miguel Rodríguez", RoleName = "chofer" },
                    new() { UserId = Guid.NewGuid(), FullName = "Jorge Martínez", RoleName = "chofer" }
                },
                "tecnico" => new List<EmpleadoItemDto>
                {
                    new() { UserId = Guid.NewGuid(), FullName = "Ana García", RoleName = "tecnico" },
                    new() { UserId = Guid.NewGuid(), FullName = "Luis Pérez", RoleName = "tecnico" },
                    new() { UserId = Guid.NewGuid(), FullName = "Roberto Gómez", RoleName = "tecnico" }
                },
                _ => new List<EmpleadoItemDto>()
            };
        }
    }

    // Obtener todos los choferes
    public async Task<List<EmpleadoItemDto>> GetChoferesAsync() => await GetByRoleAsync("chofer");

    // Obtener todos los técnicos
    public async Task<List<EmpleadoItemDto>> GetTecnicosAsync() => await GetByRoleAsync("tecnico");

    // Clases internas para deserializar
    // Clases internas para deserializar
private class EmpleadoDto
{
    [JsonPropertyName("userId")]        
    public Guid UserId { get; set; }
    
    [JsonPropertyName("fullName")]      
    public string FullName { get; set; } = "";
    
    [JsonPropertyName("professionalId")] 
    public string? ProfessionalId { get; set; }
    
    [JsonPropertyName("idRole")]        
    public Guid? IdRole { get; set; }
    
    public RoleDto? Role { get; set; }
}

private class RoleDto
{
    [JsonPropertyName("idRole")]       
    public Guid IdRole { get; set; }
    
    [JsonPropertyName("roleName")]      
    public string RoleName { get; set; } = "";
}
}

public class EmpleadoItemDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = "";
    public string RoleName { get; set; } = "";
    public string? ProfessionalId { get; set; }
    
    public string DisplayName => $"{FullName} ({RoleName})";
}