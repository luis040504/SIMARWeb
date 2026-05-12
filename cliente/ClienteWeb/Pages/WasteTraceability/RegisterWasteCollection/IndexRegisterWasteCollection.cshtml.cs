using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteWeb.Services;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteCollection
{
    public class ContratoSeguimiento
    {
        public int Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string EstadoContrato { get; set; } = string.Empty;
        public DateTime? FechaExpiracion { get; set; }
        
        // Último servicio en curso
        public ManifestSummary? UltimoServicio { get; set; }
        
        // Historial completo
        public List<ManifestSummary> HistorialServicios { get; set; } = new();
    }

    public class ServicioRecoleccion
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string Direccion { get; set; }
        public string Contrato { get; set; }
        public string Conductor { get; set; }
        public string Vehiculo { get; set; }
        public string Tecnico { get; set; }
        public DateTime FechaServicio { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public string TipoResiduo { get; set; }
        public double CantidadEstimada { get; set; }
        public string Manifiesto { get; set; }
        public string OperadorAsignado { get; set; }
    }

    public class IndexModel : PageModel
    {
        private readonly ContratosApiService _contratosService;
        private readonly ManifestApiService _manifestService;

        public IndexModel(ContratosApiService contratosService, ManifestApiService manifestService)
        {
            _contratosService = contratosService;
            _manifestService = manifestService;
        }

        public List<ContratoSeguimiento> Contratos { get; set; } = new();
        public string Rol { get; set; } = "empresa";

        public async Task OnGetAsync(string rol = "empresa")
        {
            Rol = rol;
            ViewData["Rol"] = rol;
            
            var contratosApi = await _contratosService.GetAllAsync();
            
            foreach (var c in contratosApi)
            {
                var manifests = await _manifestService.GetAllAsync(contratoId: c.Id);
                
                var cs = new ContratoSeguimiento
                {
                    Id = c.Id,
                    Folio = c.Folio,
                    Cliente = c.ClientName,
                    EstadoContrato = c.Status,
                    FechaExpiracion = c.ExpirationDate,
                    HistorialServicios = manifests.OrderByDescending(m => m.ManifestDate).ToList()
                };
                
                // El "último servicio en curso" (o el más reciente que no esté completado/cancelado)
                cs.UltimoServicio = cs.HistorialServicios
                    .FirstOrDefault(m => m.Status == "borrador" || m.Status == "en_transito") 
                    ?? cs.HistorialServicios.FirstOrDefault();
                
                Contratos.Add(cs);
            }

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

        // Mapeo de estados para mostrar al usuario
        public static string MapStatus(string status)
        {
            return status switch
            {
                "borrador" => "Programado",
                "en_transito" => "En ruta",
                "completado" => "Concluido",
                "cancelado" => "Cancelado",
                _ => status
            };
        }

        public static string GetStatusClass(string status)
        {
            return status switch
            {
                "borrador" => "bg-info",
                "en_transito" => "bg-warning",
                "completado" => "bg-success",
                "cancelado" => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}