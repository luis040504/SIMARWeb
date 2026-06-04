using System.Text.Json.Serialization;

namespace ClienteWeb.Models;

public class RecoleccionCreateDto
{
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
    public string Estado { get; set; } = "Programada";
    
    [JsonPropertyName("tiposResiduo")] 
    public List<TipoResiduoRecoleccionDto> TiposResiduo { get; set; } = new();
    
    [JsonPropertyName("observaciones")] 
    public string? Observaciones { get; set; }
}