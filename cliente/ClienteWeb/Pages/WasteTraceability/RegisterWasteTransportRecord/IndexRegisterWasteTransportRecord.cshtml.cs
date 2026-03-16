using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteTransportRecord
{
    public class ServicioTransporte
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string Direccion { get; set; }
        public string Contrato { get; set; }
        public string Conductor { get; set; }
        public string Vehiculo { get; set; }
        public string Placa { get; set; }
        public string TipoVehiculo { get; set; }
        public string TipoResiduoTransporte { get; set; }
        public string Tecnico { get; set; }
        public DateTime FechaServicio { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public string TipoResiduo { get; set; }
        public double CantidadEstimada { get; set; }
    }

    public class IndexModel : PageModel
    {
        public List<ServicioTransporte> Servicios { get; set; }

        public void OnGet()
        {
            // Hardcodeamos servicios en estado "Recolectado" para CU-24
            Servicios = new List<ServicioTransporte>
            {
                new ServicioTransporte
                {
                    Id = 1001,
                    Cliente = "Industrias ABC",
                    Direccion = "Av. Industrial 123, Parque Industrial",
                    Contrato = "CON-2024-001",
                    Conductor = "Juan Pérez",
                    Vehiculo = "Camión ABC-123",
                    Placa = "ABC-123",
                    TipoVehiculo = "Camión de 3 toneladas",
                    TipoResiduoTransporte = "Residuos industriales no peligrosos",
                    Tecnico = "Carlos López",
                    FechaServicio = DateTime.Today,
                    Estado = "Recolectado",
                    Observaciones = "Residuos industriales no peligrosos",
                    TipoResiduo = "Plásticos",
                    CantidadEstimada = 500.5
                },
                new ServicioTransporte
                {
                    Id = 1002,
                    Cliente = "Comercial XYZ",
                    Direccion = "Calle Comercio 456, Centro",
                    Contrato = "CON-2024-045",
                    Conductor = "María García",
                    Vehiculo = "Camión DEF-456",
                    Placa = "DEF-456",
                    TipoVehiculo = "Camión refrigerado",
                    TipoResiduoTransporte = "Residuos orgánicos",
                    Tecnico = "Roberto Sánchez",
                    FechaServicio = DateTime.Today,
                    Estado = "Recolectado",
                    Observaciones = "Recolección semanal de residuos orgánicos",
                    TipoResiduo = "Orgánicos",
                    CantidadEstimada = 300.0
                },
                new ServicioTransporte
                {
                    Id = 1003,
                    Cliente = "Hospital del Sur",
                    Direccion = "Av. Salud 789, Colonia Médica",
                    Contrato = "CON-2024-089",
                    Conductor = "Ana Martínez",
                    Vehiculo = "Camión GHI-789",
                    Placa = "GHI-789",
                    TipoVehiculo = "Camión especial para residuos biológicos",
                    TipoResiduoTransporte = "Residuos biológicos",
                    Tecnico = "José Ramírez",
                    FechaServicio = DateTime.Today.AddDays(1),
                    Estado = "Recolectado",
                    Observaciones = "Residuos biológicos - Manejo especial",
                    TipoResiduo = "Biológicos",
                    CantidadEstimada = 150.75
                }
            };

            if (TempData["MensajeExito"] != null)
            {
                ViewData["MensajeExito"] = TempData["MensajeExito"];
            }

            if (TempData["MensajeError"] != null)
            {
                ViewData["MensajeError"] = TempData["MensajeError"];
                ViewData["TipoError"] = TempData["TipoError"];
            }
        }

        public IActionResult OnPostIniciarTransporte(int id)
        {
            return RedirectToPage("TransportConfirm", new { id });
        }
    }
}