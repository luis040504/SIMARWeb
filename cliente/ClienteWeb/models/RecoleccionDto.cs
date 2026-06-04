using System.Text.Json.Serialization;

namespace ClienteWeb.Models;

public class RecoleccionDto
{
    [JsonPropertyName("_id")] 
    public string Id { get; set; } = "";
    
    [JsonPropertyName("idContrato")] 
    public int IdContrato { get; set; }
    
    [JsonPropertyName("cliente")] 
    public string Cliente { get; set; } = "";
    
    [JsonPropertyName("fecha")] 
    public DateTime Fecha { get; set; }
    
    [JsonPropertyName("direccion")] 
    public string Direccion { get; set; } = "";
    
    [JsonPropertyName("vehiculos")] 
    public List<VehiculoAsignadoDto> Vehiculos { get; set; } = new();
    
    [JsonPropertyName("estado")] 
    public string Estado { get; set; } = "";
    
    [JsonPropertyName("tiposResiduo")] 
    public List<TipoResiduoRecoleccionDto> TiposResiduo { get; set; } = new();
    
    [JsonPropertyName("observaciones")] 
    public string? Observaciones { get; set; }
    
    // Propiedad legacy para compatibilidad (si aún existe en la API)
    [JsonPropertyName("tipoResiduo")]
    public string? TipoResiduoLegacy { get; set; }
    
    // Propiedades de ayuda para la vista
    public string VehiculoPrincipal => Vehiculos.FirstOrDefault()?.Vehiculo ?? "Sin asignar";
    public string ChoferPrincipal => Vehiculos.FirstOrDefault()?.Chofer ?? "Sin asignar";
}