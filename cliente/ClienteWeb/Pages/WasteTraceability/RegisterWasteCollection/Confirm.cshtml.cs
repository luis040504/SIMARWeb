using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteCollection
{
    public class ConfirmModel : PageModel
    {
        public ServicioRecoleccion Servicio { get; set; }
        
        [TempData]
        public string MensajeError { get; set; }
        
        [TempData]
        public string TipoError { get; set; }

        public void OnGet()
        {
            // Simular un servicio de ejemplo para mostrar
            Servicio = new ServicioRecoleccion
            {
                Id = 1001,
                Cliente = "Industrias ABC",
                Direccion = "Av. Industrial 123, Parque Industrial",
                Contrato = "CON-2024-001",
                Conductor = "Juan Pérez",
                Vehiculo = "Camión ABC-123",
                Tecnico = "Carlos López",
                FechaServicio = DateTime.Today,
                Estado = "Asignado",
                Observaciones = "Residuos industriales no peligrosos",
                TipoResiduo = "Plásticos",
                CantidadEstimada = 500.5
            };
        }

        public IActionResult OnPostConfirmar()
        {
            TempData["MensajeExito"] = "Servicio recolectado exitosamente.";
            return RedirectToPage("IndexRegisterWasteCollection");
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("IndexRegisterWasteCollection");
        }
    }
}