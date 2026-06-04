using System;

namespace ClienteWeb.Models
{
public class TipoResiduoCatalogoDto
{
    public int Id { get; set; }
    public string CodigoCatalogo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string TipoResiduo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
}