using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteCollection
{
    public class ServicioRecoleccion
    {
        public string Id { get; set; }
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
        private const string ServiciosApiUrl = "http://localhost:8005/api/servicios";
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ServicioRecoleccion> Servicios { get; set; } = new();
        public string Rol { get; set; } = "empresa";

        public async Task OnGetAsync(string rol = "empresa")
        {
            Rol = rol;
            ViewData["Rol"] = rol;
            await LoadServiciosAsync();

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

        private async Task LoadServiciosAsync()
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetFromJsonAsync<ApiResponse<List<ServicioRecoleccionResponse>>>(
                    ServiciosApiUrl,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (response?.Success == true && response.Data != null)
                {
                    Servicios = response.Data.ConvertAll(MapToViewModel);
                }
                else
                {
                    Servicios = new List<ServicioRecoleccion>();
                    ViewData["MensajeError"] = "No se pudieron cargar los servicios desde el backend.";
                }
            }
            catch (Exception)
            {
                Servicios = new List<ServicioRecoleccion>();
                ViewData["MensajeError"] = "Ocurrió un error al consultar el backend de servicios.";
            }
        }

        private static ServicioRecoleccion MapToViewModel(ServicioRecoleccionResponse dto)
        {
            return new ServicioRecoleccion
            {
                Id = dto.Id ?? string.Empty,
                Cliente = dto.Cliente ?? string.Empty,
                Direccion = dto.Direccion ?? string.Empty,
                Contrato = dto.Contrato ?? string.Empty,
                Conductor = dto.Conductor ?? string.Empty,
                Vehiculo = dto.Vehiculo ?? string.Empty,
                Tecnico = dto.Tecnico ?? string.Empty,
                FechaServicio = dto.FechaServicio ?? DateTime.Today,
                Estado = dto.Estado ?? string.Empty,
                Observaciones = dto.Observaciones ?? string.Empty,
                TipoResiduo = dto.TipoResiduo ?? string.Empty,
                CantidadEstimada = dto.CantidadEstimada ?? 0.0,
                Manifiesto = dto.Manifiesto ?? string.Empty,
                OperadorAsignado = dto.OperadorAsignado ?? string.Empty
            };
        }
    }
}
