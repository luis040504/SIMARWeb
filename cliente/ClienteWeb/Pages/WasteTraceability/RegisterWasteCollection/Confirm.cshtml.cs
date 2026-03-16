using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteCollection
{
    public class ConfirmModel : PageModel
    {
        public ServicioRecoleccion Servicio { get; set; }
        
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
            
            if (Servicio.Estado != "Asignado")
            {
                TempData["MensajeError"] = "El servicio ya no se puede iniciar porque su estado actual es: " + Servicio.Estado;
                TempData["TipoError"] = "EX-02";
                return RedirectToPage("Index");
            }
            
            return Page();
        }

        public IActionResult OnPostConfirmar(int id)
        {
            try
            {
                var servicio = ObtenerServicioPorId(id);
                
                if (servicio == null)
                {
                    return NotFound();
                }
                
                if (servicio.Estado != "Asignado")
                {
                    TempData["MensajeError"] = "El servicio ya no se puede iniciar porque su estado actual es: " + servicio.Estado + ". Por favor, contacte a su administrador.";
                    TempData["TipoError"] = "EX-02";
                    return RedirectToPage("Index");
                }
                
                Random rand = new Random();
                if (rand.Next(1, 100) <= 20) 
                {
                    throw new Exception("Error de conexión a la base de datos");
                }
                
                servicio.Estado = "Recolectado";
                
                DateTime fechaInicio = DateTime.Now;
                
                string auditoria = $"Usuario: {User.Identity?.Name ?? "OperadorDemo"} - Fecha: {fechaInicio} - Servicio: {id} - Acción: Inicio de recolección";
                
                
                TempData["MensajeExito"] = $"Servicio {id} recolectado exitosamente. Fecha de inicio: {fechaInicio:dd/MM/yyyy HH:mm:ss}";
                
                return RedirectToPage("IndexRegisterWasteCollection");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error de conexión. No se pudo iniciar el servicio. Intente de nuevo.";
                TempData["TipoError"] = "EX-01";
                return RedirectToPage("Index");
            }
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("IndexRegisterWasteCollection");
        }

        private ServicioRecoleccion ObtenerServicioPorId(int id)
        {
            var servicios = new List<ServicioRecoleccion>
            {
                new ServicioRecoleccion
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
                },
                new ServicioRecoleccion
                {
                    Id = 1002,
                    Cliente = "Comercial XYZ",
                    Direccion = "Calle Comercio 456, Centro",
                    Contrato = "CON-2024-045",
                    Conductor = "María García",
                    Vehiculo = "Camión DEF-456",
                    Tecnico = "Roberto Sánchez",
                    FechaServicio = DateTime.Today,
                    Estado = "Asignado",
                    Observaciones = "Recolección semanal de residuos orgánicos",
                    TipoResiduo = "Orgánicos",
                    CantidadEstimada = 300.0
                }
            };
            
            return servicios.Find(s => s.Id == id);
        }
    }
}