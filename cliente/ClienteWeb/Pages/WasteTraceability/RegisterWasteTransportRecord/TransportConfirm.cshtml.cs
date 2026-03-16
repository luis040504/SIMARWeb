using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteTransportRecord
{
    public class TransportConfirmModel : PageModel
    {
        public ServicioTransporte Servicio { get; set; }
        
        [TempData]
        public string MensajeError { get; set; }
        
        [TempData]
        public string TipoError { get; set; }

        public IActionResult OnGet(int id)
        {
            Servicio = ObtenerServicioPorId(id);
            
            if (Servicio == null)
            {
                return NotFound();
            }
            
            if (Servicio.Estado != "Recolectado")
            {
                TempData["MensajeError"] = "El servicio debe estar en estado 'Recolectado' para iniciar el transporte. Estado actual: " + Servicio.Estado;
                TempData["TipoError"] = "EX-02";
                return RedirectToPage("Index");
            }
            
            return Page();
        }

        public IActionResult OnPostConfirmarTransporte(int id)
        {
            try
            {
                var servicio = ObtenerServicioPorId(id);
                
                if (servicio == null)
                {
                    return NotFound();
                }
                
                if (servicio.Estado != "Recolectado")
                {
                    TempData["MensajeError"] = "El servicio ya no se puede transportar porque su estado actual es: " + servicio.Estado;
                    TempData["TipoError"] = "EX-02";
                    return RedirectToPage("Index");
                }
                
                Random rand = new Random();
                if (rand.Next(1, 100) <= 20)
                {
                    throw new Exception("Error de conexión a la base de datos");
                }
                
                servicio.Estado = "Residuos en transporte";
                
                DateTime fechaInicioTransporte = DateTime.Now;
                
                TempData["MensajeExito"] = $"Inicio de transporte exitoso. Servicio {id} - {fechaInicioTransporte:dd/MM/yyyy HH:mm:ss}";
                
                return RedirectToPage("Index");
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "No se pudo guardar el inicio del transporte. Por favor intente más tarde.";
                TempData["TipoError"] = "EX-01";
                return RedirectToPage("IndexRegisterWasteTransportRecord");
            }
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("IndexRegisterWasteTransportRecord");
        }

        private ServicioTransporte ObtenerServicioPorId(int id)
        {
            var servicios = new List<ServicioTransporte>
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
                }
            };
            
            return servicios.Find(s => s.Id == id);
        }
    }
}