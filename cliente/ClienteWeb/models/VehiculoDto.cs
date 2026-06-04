using System;

namespace ClienteWeb.Models
{
public class VehiculoDto
{
    public int Id { get; set; }
    public string? NumeroEconomico { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int? Anio { get; set; }
    public string? Color { get; set; }
    public string Placas { get; set; } = string.Empty;
    public decimal? PesoToneladas { get; set; }
    public string LicenciaRequerida { get; set; } = string.Empty;
    public string TipoGasolina { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? FotoBase64 { get; set; }  // Para mostrar en frontend
    public List<TipoResiduoDto> TiposResiduo { get; set; } = new();
}
}