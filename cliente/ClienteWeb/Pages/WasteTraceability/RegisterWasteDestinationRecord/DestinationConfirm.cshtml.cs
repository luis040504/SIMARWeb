using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteDestinationRecord
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

    public class DestinationConfirmModel : PageModel
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
                Id = 1008,
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
                Estado = "Residuos en transporte",
                Observaciones = "Residuos industriales no peligrosos en tránsito",
                TipoResiduo = "Plásticos",
                CantidadEstimada = 450.0
            };
        }

        public IActionResult OnPostConfirmar()
        {
            TempData["MensajeExito"] = "Llegada al destino registrada exitosamente. Estado actualizado a 'Residuos depositados'.";
            return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
        }
    }
}