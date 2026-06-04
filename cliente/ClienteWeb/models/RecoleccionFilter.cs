namespace ClienteWeb.Models;

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