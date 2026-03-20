using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace ClienteWeb.Pages.Vehiculos
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal? PesoToneladas { get; set; }
        public string LicenciaRequerida { get; set; } = string.Empty;
        public string TipoDesecho { get; set; } = string.Empty;
        public string TipoGasolina { get; set; } = string.Empty;
        public string Placas { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? FotoUrl { get; set; }
        public string? NumeroEconomico { get; set; }
        public int? Año { get; set; }
        public string? Color { get; set; }
    }

    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public List<Vehiculo> Vehiculos { get; set; } = new();

        public void OnGet()
        {
            // Datos de ejemplo
            Vehiculos = new List<Vehiculo>
            {
                new Vehiculo
                {
                    Id = 1,
                    Marca = "Kenworth",
                    Modelo = "T680",
                    PesoToneladas = 8.5m,
                    LicenciaRequerida = "C",
                    TipoDesecho = "Residuos peligrosos",
                    TipoGasolina = "Diesel",
                    Placas = "ABC-1234",
                    NumeroEconomico = "VH-001",
                    Año = 2020,
                    Color = "Blanco",
                    Descripcion = "Tractocamión para residuos peligrosos",
                    FotoUrl = "/images/vehiculos/kenworth-t680.jpg"
                },
                new Vehiculo
                {
                    Id = 2,
                    Marca = "International",
                    Modelo = "4300",
                    PesoToneladas = 6.2m,
                    LicenciaRequerida = "C",
                    TipoDesecho = "Residuos biológicos",
                    TipoGasolina = "Diesel",
                    Placas = "DEF-5678",
                    NumeroEconomico = "VH-002",
                    Año = 2021,
                    Color = "Azul",
                    Descripcion = "Camión para residuos biológicos",
                    FotoUrl = "/images/vehiculos/international-4300.jpg"
                },
                new Vehiculo
                {
                    Id = 3,
                    Marca = "Freightliner",
                    Modelo = "M2",
                    PesoToneladas = 5.0m,
                    LicenciaRequerida = "B",
                    TipoDesecho = "Papel y cartón",
                    TipoGasolina = "Diesel",
                    Placas = "GHI-9012",
                    NumeroEconomico = "VH-003",
                    Año = 2022,
                    Color = "Verde",
                    Descripcion = "Camión para recolección de reciclables",
                    FotoUrl = "/images/vehiculos/freightliner-m2.jpg"
                },
                new Vehiculo
                {
                    Id = 4,
                    Marca = "RAM",
                    Modelo = "5500",
                    PesoToneladas = 3.5m,
                    LicenciaRequerida = "B",
                    TipoDesecho = "Residuos varios",
                    TipoGasolina = "Diesel",
                    Placas = "JKL-3456",
                    NumeroEconomico = "VH-004",
                    Año = 2023,
                    Color = "Rojo",
                    Descripcion = "Camión mediano para servicios urbanos",
                    FotoUrl = "/images/vehiculos/ram-5500.jpg"
                }
            };

            // Filtrar por búsqueda si existe
            if (!string.IsNullOrEmpty(Search))
            {
                Vehiculos = Vehiculos.FindAll(v =>
                    v.Marca.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                    v.Modelo.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                    v.NumeroEconomico?.Contains(Search, StringComparison.OrdinalIgnoreCase) == true ||
                    v.Placas.Contains(Search, StringComparison.OrdinalIgnoreCase) ||
                    v.TipoDesecho.Contains(Search, StringComparison.OrdinalIgnoreCase)
                );
            }
        }

        public IActionResult OnGetObtenerTiposDesecho()
        {
            var tipos = new List<string>
            {
                "Residuos simples (papel, cartón)",
                "Residuos peligrosos",
                "Residuos biológicos",
                "Residuos electrónicos",
                "Residuos de construcción",
                "Residuos industriales",
                "Residuos varios"
            };
            return new JsonResult(tipos);
        }

        public IActionResult OnGetObtenerTiposGasolina()
        {
            var tipos = new List<string>
            {
                "Magna",
                "Premium",
                "Diesel",
                "Eléctrico",
                "Híbrido",
                "Gas LP"
            };
            return new JsonResult(tipos);
        }
    }
}