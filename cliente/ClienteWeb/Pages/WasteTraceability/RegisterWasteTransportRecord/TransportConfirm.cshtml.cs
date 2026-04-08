using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

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

    public class TransportConfirmModel : PageModel
    {
        public ServicioTransporte Servicio { get; set; }
        
        [TempData]
        public string MensajeError { get; set; }
        
        [TempData]
        public string TipoError { get; set; }

        public void OnGet()
        {
            // Simular un servicio de ejemplo para mostrar
            Servicio = new ServicioTransporte
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
            };
        }

        public IActionResult OnPostConfirmarTransporte()
        {
            TempData["MensajeExito"] = "Inicio de transporte exitoso. Estado actualizado a 'Residuos en transporte'.";
            return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
        }
    }
}