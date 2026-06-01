using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClienteWeb.Services;

using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using ClienteWeb.Models;

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
        private readonly HttpClient _clientesApi;

        public IndexModel(ContratosApiService contratosService, ManifestApiService manifestService, IHttpClientFactory factory)
        {
            _contratosService = contratosService;
            _manifestService = manifestService;
            _clientesApi = factory.CreateClient("ClientesApi");
        }

        public List<ContratoSeguimiento> Contratos { get; set; } = new();
        public string Rol { get; set; } = "empresa";

        public async Task OnGetAsync(string rol = "empresa")
        {
            var sessionRole = HttpContext.Session.GetString("Rol");
            Rol = !string.IsNullOrEmpty(sessionRole) ? sessionRole : rol;
            ViewData["Rol"] = Rol;
            
            try
            {
                int? filterClientId = null;
                if (Rol.ToLower() == "cliente")
                {
                    var userId = HttpContext.Session.GetString("UserId");
                    if (!string.IsNullOrEmpty(userId))
                    {
                        try
                        {
                            var clientInfo = await _clientesApi.GetFromJsonAsync<ClienteOutput>($"client/user/{userId}");
                            if (clientInfo != null)
                            {
                                filterClientId = clientInfo.Id;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching client ID for traceability: {ex.Message}");
                        }
                    }
                }

                var contratosApi = await _contratosService.GetAllAsync();
                
                foreach (var c in contratosApi)
                {
                    if (filterClientId.HasValue && c.ClientId != filterClientId.Value)
                    {
                        continue;
                    }

                    List<ManifestSummary> manifests = new();
                    try
                    {
                        manifests = await _manifestService.GetAllAsync(contratoId: c.Id);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching manifests for contract {c.Id}: {ex.Message}");
                    }
                    
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
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = $"Error al cargar la trazabilidad de residuos: {ex.Message}";
                ViewData["TipoError"] = "danger";
                Console.WriteLine($"Error general en OnGetAsync de RegisterWasteCollection: {ex}");
            }

            if (TempData["MensajeExito"] != null)
            {
                ViewData["MensajeExito"] = TempData["MensajeExito"];
            }

            if (TempData["MensajeError"] != null && ViewData["MensajeError"] == null)
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
                "borrador" => "badge-simar badge-simar-programada",
                "en_transito" => "badge-simar badge-simar-enruta",
                "completado" => "badge-simar badge-simar-completada",
                "cancelado" => "badge-simar badge-simar-cancelada",
                _ => "badge-simar badge-simar-sin"
            };
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(string manifestId, string nuevoEstado)
        {
            var sessionRole = HttpContext.Session.GetString("Rol");
            if (string.Equals(sessionRole, "cliente", StringComparison.OrdinalIgnoreCase))
            {
                TempData["MensajeError"] = "No tienes permisos para realizar esta acción.";
                TempData["TipoError"] = "danger";
                return RedirectToPage();
            }

            try
            {
                await _manifestService.UpdateStatusAsync(manifestId, nuevoEstado);
                TempData["MensajeExito"] = "Estado del servicio actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al actualizar el estado: {ex.Message}";
                TempData["TipoError"] = "danger";
            }

            return RedirectToPage();
        }
    }
}