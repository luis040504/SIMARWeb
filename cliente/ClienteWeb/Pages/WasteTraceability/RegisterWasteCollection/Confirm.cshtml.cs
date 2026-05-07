using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteCollection
{
    public class ConfirmModel : PageModel
    {
        private const string ServiciosApiUrl = "http://localhost:8005/api/servicios";
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfirmModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ServicioRecoleccion? Servicio { get; set; }
        
        [TempData]
        public string? MensajeError { get; set; }
        
        [TempData]
        public string? TipoError { get; set; }

        public async Task OnGetAsync(string? id = null)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                await LoadServicioAsync(id);
            }
        }

        public async Task<IActionResult> OnPostConfirmarAsync()
        {
            if (Servicio == null || string.IsNullOrWhiteSpace(Servicio.Id))
            {
                TempData["MensajeError"] = "No se identificó el servicio a confirmar.";
                return RedirectToPage("IndexRegisterWasteCollection");
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var requestUrl = $"{ServiciosApiUrl}/{Uri.EscapeDataString(Servicio.Id)}/confirmar-recoleccion";
                var response = await httpClient.PostAsync(requestUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] = "No se pudo confirmar la recolección en el backend.";
                    TempData["TipoError"] = response.StatusCode.ToString();
                    return RedirectToPage("IndexRegisterWasteCollection");
                }

                TempData["MensajeExito"] = "Servicio recolectado exitosamente.";
                return RedirectToPage("IndexRegisterWasteCollection");
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error al confirmar la recolección.";
                return RedirectToPage("IndexRegisterWasteCollection");
            }
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("IndexRegisterWasteCollection");
        }

        private async Task LoadServicioAsync(string id)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetFromJsonAsync<ApiResponse<ServicioRecoleccionResponse>>(
                    $"{ServiciosApiUrl}/{Uri.EscapeDataString(id)}",
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (response?.Success == true && response.Data != null)
                {
                    Servicio = MapToViewModel(response.Data);
                }
            }
            catch (Exception)
            {
                Servicio = null;
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

    public class ServicioRecoleccionResponse
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("cliente")]
        public string Cliente { get; set; }

        [JsonPropertyName("direccion")]
        public string Direccion { get; set; }

        [JsonPropertyName("contrato")]
        public string Contrato { get; set; }

        [JsonPropertyName("conductor")]
        public string Conductor { get; set; }

        [JsonPropertyName("vehiculo")]
        public string Vehiculo { get; set; }

        [JsonPropertyName("tecnico")]
        public string Tecnico { get; set; }

        [JsonPropertyName("fechaServicio")]
        public DateTime? FechaServicio { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("observaciones")]
        public string Observaciones { get; set; }

        [JsonPropertyName("tipoResiduo")]
        public string TipoResiduo { get; set; }

        [JsonPropertyName("cantidadEstimada")]
        public double? CantidadEstimada { get; set; }

        [JsonPropertyName("manifiesto")]
        public string Manifiesto { get; set; }

        [JsonPropertyName("operadorAsignado")]
        public string OperadorAsignado { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
    }
}