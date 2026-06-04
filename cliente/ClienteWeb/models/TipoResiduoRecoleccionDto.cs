using System.Text.Json.Serialization;

namespace ClienteWeb.Models;

public class TipoResiduoRecoleccionDto
{
    [JsonPropertyName("wasteTypeId")] 
    public int WasteTypeId { get; set; }
    
    [JsonPropertyName("wasteTypeCode")] 
    public string WasteTypeCode { get; set; } = "";
    
    [JsonPropertyName("wasteTypeName")] 
    public string WasteTypeName { get; set; } = "";
    
    [JsonPropertyName("wasteType")] 
    public string WasteType { get; set; } = "";
    
    [JsonPropertyName("cantidadEstimada")] 
    public double CantidadEstimada { get; set; }
    
    [JsonPropertyName("unidad")] 
    public string Unidad { get; set; } = "kg";
}