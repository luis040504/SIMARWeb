namespace SimarWeb.models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal? PesoToneladas { get; set; }
        public string LicenciaRequerida { get; set; } = string.Empty; // A, B, C, D, E
        public string TipoDesecho { get; set; } = string.Empty; // Residuo simple, peligroso, biológico, etc.
        public string TipoGasolina { get; set; } = string.Empty; // Magna, Premium, Diesel, Eléctrico
        public string Placas { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? FotoUrl { get; set; }
        public string? NumeroEconomico { get; set; }
        public int? Año { get; set; }
        public string? Color { get; set; }
    }
}