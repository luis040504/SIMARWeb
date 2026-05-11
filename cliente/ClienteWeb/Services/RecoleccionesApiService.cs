using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClienteWeb.Services;

public class RecoleccionesApiService
{
    private readonly HttpClient _http;

    public RecoleccionesApiService(HttpClient http) => _http = http;

    // Obtener todas las recolecciones con filtros
    public async Task<List<RecoleccionDto>> GetAllAsync(RecoleccionFilter? filtro = null)
    {
        try
        {
            var queryParams = new List<string>();
            
            if (filtro != null)
            {
                if (filtro.IdContrato.HasValue)
                    queryParams.Add($"idContrato={filtro.IdContrato}");
                if (!string.IsNullOrEmpty(filtro.Cliente))
                    queryParams.Add($"cliente={Uri.EscapeDataString(filtro.Cliente)}");
                if (filtro.FechaInicio.HasValue)
                    queryParams.Add($"fechaInicio={filtro.FechaInicio.Value:yyyy-MM-dd}");
                if (filtro.FechaFin.HasValue)
                    queryParams.Add($"fechaFin={filtro.FechaFin.Value:yyyy-MM-dd}");
                if (!string.IsNullOrEmpty(filtro.Vehiculo))
                    queryParams.Add($"vehiculo={Uri.EscapeDataString(filtro.Vehiculo)}");
                if (!string.IsNullOrEmpty(filtro.Chofer))
                    queryParams.Add($"chofer={Uri.EscapeDataString(filtro.Chofer)}");
                if (!string.IsNullOrEmpty(filtro.Tecnico))
                    queryParams.Add($"tecnico={Uri.EscapeDataString(filtro.Tecnico)}");
                if (!string.IsNullOrEmpty(filtro.Estado))
                    queryParams.Add($"estado={Uri.EscapeDataString(filtro.Estado)}");
            }

            var url = "api/recolecciones";
            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            var resp = await _http.GetFromJsonAsync<RecoleccionesResponse>(url);
            return resp?.Data ?? [];
        }
        catch
        {
            return [];
        }
    }

    // Obtener recolecciones por contrato
    public async Task<List<RecoleccionDto>> GetByContratoAsync(int idContrato)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<RecoleccionesResponse>($"api/recolecciones/contrato/{idContrato}");
            return resp?.Data ?? [];
        }
        catch
        {
            return [];
        }
    }

    // Obtener por ID
    public async Task<RecoleccionDto?> GetByIdAsync(string id)
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<SingleRecoleccionResponse>($"api/recolecciones/{id}");
            return resp?.Data;
        }
        catch
        {
            return null;
        }
    }

    // Crear recolección
    public async Task<(bool Success, string? Error)> CreateAsync(RecoleccionCreateDto recoleccion)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/recolecciones", recoleccion);
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            var error = await response.Content.ReadAsStringAsync();
            return (false, $"Error al crear recolección: {error}");
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // Actualizar recolección
    public async Task<(bool Success, string? Error)> UpdateAsync(string id, RecoleccionUpdateDto recoleccion)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/recolecciones/{id}", recoleccion);
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (false, "Recolección no encontrada");
                
            return (false, "Error al actualizar recolección");
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // Eliminar recolección
    public async Task<(bool Success, string? Error)> DeleteAsync(string id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/recolecciones/{id}");
            
            if (response.IsSuccessStatusCode)
                return (true, null);
                
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return (false, "Recolección no encontrada");
                
            return (false, "Error al eliminar recolección");
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    // Obtener estados
    public async Task<List<string>> GetEstadosAsync()
    {
        try
        {
            var resp = await _http.GetFromJsonAsync<EstadosResponse>("api/recolecciones/estados");
            return resp?.Data ?? new List<string> { "Programada", "En ruta", "Completada", "Cancelada" };
        }
        catch
        {
            return new List<string> { "Programada", "En ruta", "Completada", "Cancelada" };
        }
    }

    // Clases de respuesta
    private class RecoleccionesResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public List<RecoleccionDto>? Data { get; set; }
        [JsonPropertyName("count")] public int Count { get; set; }
    }

    private class SingleRecoleccionResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public RecoleccionDto? Data { get; set; }
    }

    private class EstadosResponse
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")] public List<string>? Data { get; set; }
    }
    public async Task<List<ContratoItemDto>> GetContratosActivosAsync()
{
    try
    {
        // Usamos el cliente de contratos
        var response = await _http.GetFromJsonAsync<List<ContratoItemDto>>("http://localhost:8006/api/contracts");
        return response ?? [];
    }
    catch
    {
        // Fallback con datos de prueba
        return new List<ContratoItemDto>
        {
            new ContratoItemDto { Id = 1, Folio = "CON-202605-0001", Cliente = "Hospital Ángeles" },
            new ContratoItemDto { Id = 2, Folio = "CON-202605-0002", Cliente = "Plaza Comercial Galerías" },
            new ContratoItemDto { Id = 3, Folio = "CON-202605-0003", Cliente = "Constructora ABC" }
        };
    }

}
}

// ============================================
// DTOs
// ============================================

public class RecoleccionFilter
{
    public int? IdContrato { get; set; }
    public string? Cliente { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Vehiculo { get; set; }
    public string? Chofer { get; set; }
    public string? Tecnico { get; set; }
    public string? Estado { get; set; }
}

public class VehiculoAsignadoDto
{
    [JsonPropertyName("vehiculo")] public string Vehiculo { get; set; } = "";
    [JsonPropertyName("chofer")] public string Chofer { get; set; } = "";
    [JsonPropertyName("tecnicos")] public List<string> Tecnicos { get; set; } = new();
}

public class RecoleccionDto
{
    [JsonPropertyName("_id")] public string Id { get; set; } = "";
    [JsonPropertyName("idContrato")] public int? IdContrato { get; set; }
    [JsonPropertyName("cliente")] public string Cliente { get; set; } = "";
    [JsonPropertyName("fecha")] public DateTime Fecha { get; set; }
    [JsonPropertyName("direccion")] public string Direccion { get; set; } = "";
    [JsonPropertyName("vehiculos")] public List<VehiculoAsignadoDto> Vehiculos { get; set; } = new();
    [JsonPropertyName("estado")] public string Estado { get; set; } = "Programada";
    [JsonPropertyName("tipoResiduo")] public string? TipoResiduo { get; set; }
    [JsonPropertyName("cantidadEstimada")] public double? CantidadEstimada { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }

    // Propiedades de ayuda para la vista
    public string VehiculoPrincipal => Vehiculos.FirstOrDefault()?.Vehiculo ?? "Sin asignar";
    public string ChoferPrincipal => Vehiculos.FirstOrDefault()?.Chofer ?? "Sin asignar";
    public string TecnicosList => string.Join(", ", Vehiculos.FirstOrDefault()?.Tecnicos ?? new List<string>());
}

public class RecoleccionCreateDto
{
    [JsonPropertyName("idContrato")] public int IdContrato { get; set; }
    [JsonPropertyName("cliente")] public string Cliente { get; set; } = "";
    [JsonPropertyName("fecha")] public DateTime Fecha { get; set; }
    [JsonPropertyName("direccion")] public string Direccion { get; set; } = "";
    [JsonPropertyName("vehiculos")] public List<VehiculoAsignadoDto> Vehiculos { get; set; } = new();
    [JsonPropertyName("estado")] public string Estado { get; set; } = "Programada";
    [JsonPropertyName("tipoResiduo")] public string? TipoResiduo { get; set; }
    [JsonPropertyName("cantidadEstimada")] public double? CantidadEstimada { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
}

public class RecoleccionUpdateDto
{
    [JsonPropertyName("idContrato")] public int? IdContrato { get; set; }
    [JsonPropertyName("cliente")] public string? Cliente { get; set; }
    [JsonPropertyName("fecha")] public DateTime? Fecha { get; set; }
    [JsonPropertyName("direccion")] public string? Direccion { get; set; }
    [JsonPropertyName("vehiculos")] public List<VehiculoAsignadoDto>? Vehiculos { get; set; }
    [JsonPropertyName("estado")] public string? Estado { get; set; }
    [JsonPropertyName("tipoResiduo")] public string? TipoResiduo { get; set; }
    [JsonPropertyName("cantidadEstimada")] public double? CantidadEstimada { get; set; }
    [JsonPropertyName("observaciones")] public string? Observaciones { get; set; }
}
public class ContratoItemDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("folio")] public string Folio { get; set; } = "";
    [JsonPropertyName("clientName")] public string Cliente { get; set; } = "";
}