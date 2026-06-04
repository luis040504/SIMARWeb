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

    public async Task<List<TipoResiduoCatalogoDto>> GetTiposResiduoDisponiblesAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<TiposResiduoResponse>("api/vehiculos/tipos-residuo");
            return resp?.Data ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<List<string>> GetTiposGasolinaAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<TiposResponse>("api/vehiculos/tipos-gasolina");
            return resp?.Data ?? [];
        }
        catch
        {
            return new List<string> { "Diesel", "Gasolina Magna", "Gasolina Premium", "Electrico", "Hibrido", "Gas Natural" };
        }
    }

    public async Task<(bool Success, string? Error)> CreateAsync(VehiculoCreateDto vehiculo)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            
            // Agregar campos básicos
            formData.Add(new StringContent(vehiculo.Marca ?? ""), "Marca");
            formData.Add(new StringContent(vehiculo.Modelo ?? ""), "Modelo");
            formData.Add(new StringContent(vehiculo.Placas ?? ""), "Placas");
            formData.Add(new StringContent(vehiculo.LicenciaRequerida ?? ""), "LicenciaRequerida");
            formData.Add(new StringContent(vehiculo.TipoGasolina ?? ""), "TipoGasolina");
            
            // Campos opcionales
            if (!string.IsNullOrEmpty(vehiculo.NumeroEconomico))
                formData.Add(new StringContent(vehiculo.NumeroEconomico), "NumeroEconomico");
            
            if (vehiculo.Anio.HasValue)
                formData.Add(new StringContent(vehiculo.Anio.Value.ToString()), "Anio");
            
            if (!string.IsNullOrEmpty(vehiculo.Color))
                formData.Add(new StringContent(vehiculo.Color), "Color");
            
            if (vehiculo.PesoToneladas.HasValue)
                formData.Add(new StringContent(vehiculo.PesoToneladas.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "PesoToneladas");
            
            if (!string.IsNullOrEmpty(vehiculo.Descripcion))
                formData.Add(new StringContent(vehiculo.Descripcion), "Descripcion");
            
            // Agregar IDs de tipos de residuo (múltiple)
            if (vehiculo.TiposResiduoIds != null && vehiculo.TiposResiduoIds.Any())
            {
                foreach (var id in vehiculo.TiposResiduoIds)
                {
                    formData.Add(new StringContent(id.ToString()), "tipos_residuo_ids");
                }
            }
            
            // Agregar foto si existe
            if (vehiculo.Foto != null && vehiculo.Foto.Length > 0)
            {
                var fileContent = new ByteArrayContent(vehiculo.Foto);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                formData.Add(fileContent, "foto", "vehiculo.jpg");
            }
            
            var response = await _http.PostAsync("api/vehiculos", formData);
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, "Las placas o número económico ya están registrados");
            }
                
            var errorMsg = await response.Content.ReadAsStringAsync();
            return (false, $"Error al crear vehículo: {errorMsg}");
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
            using var formData = new MultipartFormDataContent();
            
            // Agregar campos básicos
            formData.Add(new StringContent(vehiculo.Marca ?? ""), "Marca");
            formData.Add(new StringContent(vehiculo.Modelo ?? ""), "Modelo");
            formData.Add(new StringContent(vehiculo.Placas ?? ""), "Placas");
            formData.Add(new StringContent(vehiculo.LicenciaRequerida ?? ""), "LicenciaRequerida");
            formData.Add(new StringContent(vehiculo.TipoGasolina ?? ""), "TipoGasolina");
            
            // Campos opcionales
            if (!string.IsNullOrEmpty(vehiculo.NumeroEconomico))
                formData.Add(new StringContent(vehiculo.NumeroEconomico), "NumeroEconomico");
            
            if (vehiculo.Anio.HasValue)
                formData.Add(new StringContent(vehiculo.Anio.Value.ToString()), "Anio");
            
            if (!string.IsNullOrEmpty(vehiculo.Color))
                formData.Add(new StringContent(vehiculo.Color), "Color");
            
            if (vehiculo.PesoToneladas.HasValue)
                formData.Add(new StringContent(vehiculo.PesoToneladas.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "PesoToneladas");
            
            if (!string.IsNullOrEmpty(vehiculo.Descripcion))
                formData.Add(new StringContent(vehiculo.Descripcion), "Descripcion");
            
            // Agregar IDs de tipos de residuo (múltiple)
            if (vehiculo.TiposResiduoIds != null && vehiculo.TiposResiduoIds.Any())
            {
                foreach (var idTipo in vehiculo.TiposResiduoIds)
                {
                    formData.Add(new StringContent(idTipo.ToString()), "tipos_residuo_ids");
                }
            }
            
            // Agregar nueva foto solo si se proporciona
            if (vehiculo.Foto != null && vehiculo.Foto.Length > 0)
            {
                var fileContent = new ByteArrayContent(vehiculo.Foto);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                formData.Add(fileContent, "foto", "vehiculo.jpg");
            }
            
            var response = await _http.PutAsync($"api/vehiculos/{id}", formData);
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (false, "Vehículo no encontrado");
                
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, "Las placas o número económico ya están registrados en otro vehículo");
                
            var error = await response.Content.ReadAsStringAsync();
            return (false, $"Error al actualizar vehículo: {error}");
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

    // Método legacy (opcional, para compatibilidad)
    public async Task<List<string>> GetTiposDesechoLegacyAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<TiposResponse>("api/vehiculos/tipos-desecho");
            return resp?.Data ?? [];
        }
        catch
        {
            return [];
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

    private class TiposResiduoResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public List<TipoResiduoCatalogoDto>? Data { get; set; }
    }
}

// ============================================
// DTOs ACTUALIZADOS
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
    
    // Campo legacy para compatibilidad (puede venir como string único)
    [JsonPropertyName("tipo_desecho")] public string? TipoDesechoLegacy { get; set; }
    
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    [JsonPropertyName("foto_url")] public string? FotoUrl { get; set; }
    
    // Nuevos campos
    [JsonPropertyName("foto")] public string? FotoBase64 { get; set; }
    [JsonPropertyName("tipos_residuo")] public List<TipoResiduoDto>? TiposResiduo { get; set; }

    public string DisplayLabel =>
        $"{Marca} {Modelo}{(Anio.HasValue ? $" ({Anio})" : "")} — {Placas}";
    
    public string MarcaModelo => $"{Marca} {Modelo}";
    public string PesoFormateado => PesoToneladas?.ToString("N1") + " ton" ?? "N/A";
    
    // Helper para obtener nombres de tipos de residuo como string (para vistas legacy)
    public string TiposResiduoString => TiposResiduo != null && TiposResiduo.Any() 
        ? string.Join(", ", TiposResiduo.Select(t => t.Nombre))
        : TipoDesechoLegacy ?? "No especificado";
}

public class VehiculoCreateDto
{
    // Campos básicos
    [JsonPropertyName("numero_economico")] public string? NumeroEconomico { get; set; }
    [JsonPropertyName("marca")] public string Marca { get; set; } = "";
    [JsonPropertyName("modelo")] public string Modelo { get; set; } = "";
    [JsonPropertyName("anio")] public int? Anio { get; set; }
    [JsonPropertyName("color")] public string? Color { get; set; }
    [JsonPropertyName("placas")] public string Placas { get; set; } = "";
    [JsonPropertyName("peso_toneladas")] public decimal? PesoToneladas { get; set; }
    [JsonPropertyName("licencia_requerida")] public string LicenciaRequerida { get; set; } = "";
    [JsonPropertyName("tipo_gasolina")] public string TipoGasolina { get; set; } = "";
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
    
    // Campo legacy (para compatibilidad, no se usa en el nuevo backend)
    [JsonPropertyName("tipo_desecho")] public string? TipoDesechoLegacy { get; set; }
    
    // Nuevos campos
    [JsonPropertyName("tipos_residuo_ids")] public List<int> TiposResiduoIds { get; set; } = new();
    [JsonPropertyName("foto")] public byte[]? Foto { get; set; }
}

public class TipoResiduoDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("codigo")] public string? Codigo { get; set; }
    [JsonPropertyName("codigo_catalogo")] public string? CodigoCatalogo { get; set; }
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    [JsonPropertyName("tipo")] public string? Tipo { get; set; }
    [JsonPropertyName("tipo_residuo")] public string? TipoResiduo { get; set; }
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
}

public class TipoResiduoCatalogoDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("codigo_catalogo")] public string CodigoCatalogo { get; set; } = "";
    [JsonPropertyName("nombre")] public string Nombre { get; set; } = "";
    [JsonPropertyName("tipo_residuo")] public string TipoResiduo { get; set; } = "";
    [JsonPropertyName("descripcion")] public string? Descripcion { get; set; }
}