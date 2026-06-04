using System.Text.Json.Serialization;

namespace ClienteWeb.Models;

public class VehiculoAsignadoDto
{
    [JsonPropertyName("vehiculo")] 
    public string Vehiculo { get; set; } = "";
    
    [JsonPropertyName("chofer")] 
    public string Chofer { get; set; } = "";
    
    [JsonPropertyName("tecnicos")] 
    public List<string> Tecnicos { get; set; } = new();
}