using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;

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

        public IActionResult OnGet(int id)
        {
            Servicio = ObtenerServicioPorId(id);
            
            if (Servicio == null)
            {
                return NotFound();
            }
            
            if (Servicio.Estado != "Residuos en transporte")
            {
                TempData["MensajeError"] = "El servicio ya no se puede registrar llegada porque su estado actual es: " + Servicio.Estado;
                TempData["TipoError"] = "EX-03";
                return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
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
                
                if (servicio.Estado != "Residuos en transporte")
                {
                    TempData["MensajeError"] = "El servicio ya no se puede registrar llegada porque su estado actual es: " + servicio.Estado + ". Por favor, contacte a su administrador.";
                    TempData["TipoError"] = "EX-03";
                    return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
                }
                
                Random rand = new Random();
                if (rand.Next(1, 100) <= 20) 
                {
                    throw new Exception("Error de conexión a la base de datos");
                }
                
                servicio.Estado = "Residuos depositados";
                
                DateTime fechaLlegada = DateTime.Now;
                
                string auditoria = $"Usuario: {User.Identity?.Name ?? "OperadorDemo"} - Fecha: {fechaLlegada} - Servicio: {id} - Acción: Llegada al destino registrada";
                
                
                TempData["MensajeExito"] = $"Servicio {id} - Llegada al destino registrada exitosamente. Fecha: {fechaLlegada:dd/MM/yyyy HH:mm:ss}";
                
                return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error de conexión. No se pudo registrar la llegada al destino. Intente de nuevo.";
                TempData["TipoError"] = "EX-01";
                return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
            }
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
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
                    Estado = "Residuos en transporte",
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
                    Estado = "Residuos en transporte",
                    Observaciones = "Recolección semanal de residuos orgánicos",
                    TipoResiduo = "Orgánicos",
                    CantidadEstimada = 300.0
                }
            };
            
            return servicios.Find(s => s.Id == id);
        }
    }
}
