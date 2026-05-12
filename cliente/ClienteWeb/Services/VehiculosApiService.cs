using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class VehiculosApiService
{
    private readonly HttpClient _http;

    public VehiculosApiService(HttpClient http) => _http = http;

    public async Task<List<VehiculoDto>> GetAllAsync(string? search = null)
    {
        try
        {
            var url = "api/vehiculos";
            if (!string.IsNullOrEmpty(search))
                url += $"?search={Uri.EscapeDataString(search)}";
                
            var resp = await _http.GetFromJsonAsync<VehiculosResponse>(url);
            return resp?.Data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<VehiculoDto?> GetByIdAsync(int id)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<SingleVehiculoResponse>($"api/vehiculos/{id}");
            return resp?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Success, string? Error)> CreateAsync(VehiculoCreateDto vehiculo)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/vehiculos", vehiculo);
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "Las placas o número económico ya están registrados");
                
            var error = await response.Content.ReadAsStringAsync();
            return (false, $"Error al crear vehículo: {error}");
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, VehiculoCreateDto vehiculo)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/vehiculos/{id}", vehiculo);
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (false, "Vehículo no encontrado");
                
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "Las placas o número económico ya están registrados en otro vehículo");
                
            return (false, "Error al actualizar vehículo");
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/vehiculos/{id}");
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (false, "Vehículo no encontrado");
                
            return (false, "Error al eliminar vehículo");
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<List<string>> GetTiposDesechoAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<TiposResponse>("api/vehiculos/tipos-desecho");
            return resp?.Data ?? new List<string>();
        }
        catch
        {
            return new List<string> { "Residuos Peligrosos", "Residuos Reciclables", "Residuos de Construcción" };
        }
    }

    public async Task<List<string>> GetTiposGasolinaAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<TiposResponse>("api/vehiculos/tipos-gasolina");
            return resp?.Data ?? new List<string>();
        }
        catch
        {
            return new List<string> { "Diesel", "Gasolina Magna", "Gasolina Premium", "Eléctrico", "Híbrido" };
        }
    }

    // Clases de respuesta internas
    private class VehiculosResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public List<VehiculoDto>? Data { get; set; }
        [JsonPropertyName("count")] public int Count { get; set; }
    }

    private class SingleVehiculoResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public VehiculoDto? Data { get; set; }
    }

    private class TiposResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public List<string>? Data { get; set; }
    }
}

// ============================================
// DTOs
// ============================================

public class VehiculoDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("numero_economico")] public string? NumeroEconomico { get; set; }
    [JsonPropertyName("marca")] public string Marca { get; set; } = "";
    [JsonPropertyName("modelo")] public string Modelo { get; set; } = "";
    [JsonPropertyName("anio")] public int? Anio { get; set; }
    [JsonPropertyName("color")] public string? Color { get; set; }
    [JsonPropertyName("placas")] public string Placas { get; set; } = "";
    [JsonPropertyName("peso_toneladas")] public decimal? PesoToneladas { get; set; }
    [JsonPropertyName("licencia_requerida")] public string? LicenciaRequerida { get; set; }
    [JsonPropertyName("tipo_gasolina")] public string? TipoGasolina { get; set; }
    [JsonPropertyName("tipo_desecho")] public string? TipoDesecho { get; set; }
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("foto_url")] public string? FotoUrl { get; set; }

    public string DisplayLabel =>
        $"{Marca} {Modelo}{(Anio.HasValue ? $" ({Anio})" : "")} — {Placas}";
    
    // Para mostrar en cards y tablas
    public string MarcaModelo => $"{Marca} {Modelo}";
    public string PesoFormateado => PesoToneladas?.ToString("N1") + " ton" ?? "N/A";
}

public class VehiculoCreateDto
{
    [JsonPropertyName("numero_economico")] public string? NumeroEconomico { get; set; }
    [JsonPropertyName("marca")] public string Marca { get; set; } = "";
    [JsonPropertyName("modelo")] public string Modelo { get; set; } = "";
    [JsonPropertyName("anio")] public int? Anio { get; set; }
    [JsonPropertyName("color")] public string? Color { get; set; }
    [JsonPropertyName("placas")] public string Placas { get; set; } = "";
    [JsonPropertyName("peso_toneladas")] public decimal PesoToneladas { get; set; }
    [JsonPropertyName("licencia_requerida")] public string LicenciaRequerida { get; set; } = "";
    [JsonPropertyName("tipo_gasolina")] public string TipoGasolina { get; set; } = "";
    [JsonPropertyName("tipo_desecho")] public string TipoDesecho { get; set; } = "";
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("foto_url")] public string? FotoUrl { get; set; }
}