using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClienteWeb.Pages.WasteTraceability.RegisterWasteDestinationRecord
{
    public class ServicioTransporte
    {
        public string Id { get; set; }
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
        private const string ServiciosApiUrl = "http://localhost:8005/api/servicios";
        private readonly IHttpClientFactory _httpClientFactory;

        public DestinationConfirmModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public ServicioTransporte? Servicio { get; set; }
        
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
                TempData["MensajeError"] = "No se identificó el servicio para registrar la llegada.";
                return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var requestUrl = $"{ServiciosApiUrl}/{Uri.EscapeDataString(Servicio.Id)}/registrar-llegada";
                var response = await httpClient.PostAsync(requestUrl, null);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["MensajeError"] = "No se pudo registrar la llegada al backend.";
                    TempData["TipoError"] = response.StatusCode.ToString();
                    return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
                }

                TempData["MensajeExito"] = "Llegada al destino registrada exitosamente. Estado actualizado a 'Residuos depositados'.";
                return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error al registrar la llegada.";
                return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
            }
        }

        public IActionResult OnPostCancelar()
        {
            return RedirectToPage("../RegisterWasteCollection/IndexRegisterWasteCollection");
        }

        private async Task LoadServicioAsync(string id)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetFromJsonAsync<ApiResponse<ServicioTransporteResponse>>(
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

        private static ServicioTransporte MapToViewModel(ServicioTransporteResponse dto)
        {
            return new ServicioTransporte
            {
                Id = dto.Id ?? string.Empty,
                Cliente = dto.Cliente ?? string.Empty,
                Direccion = dto.Direccion ?? string.Empty,
                Contrato = dto.Contrato ?? string.Empty,
                Conductor = dto.Conductor ?? string.Empty,
                Vehiculo = dto.Vehiculo ?? string.Empty,
                Placa = dto.Placa ?? string.Empty,
                TipoVehiculo = dto.TipoVehiculo ?? string.Empty,
                TipoResiduoTransporte = dto.TipoResiduoTransporte ?? dto.TipoResiduo ?? string.Empty,
                Tecnico = dto.Tecnico ?? string.Empty,
                FechaServicio = dto.FechaServicio ?? DateTime.Today,
                Estado = dto.Estado ?? string.Empty,
                Observaciones = dto.Observaciones ?? string.Empty,
                TipoResiduo = dto.TipoResiduo ?? string.Empty,
                CantidadEstimada = dto.CantidadEstimada ?? 0.0
            };
        }
    }

    public class ServicioTransporteResponse
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

        [JsonPropertyName("placa")]
        public string Placa { get; set; }

        [JsonPropertyName("tipoVehiculo")]
        public string TipoVehiculo { get; set; }

        [JsonPropertyName("tipoResiduoTransporte")]
        public string TipoResiduoTransporte { get; set; }

        [JsonPropertyName("tipoResiduo")]
        public string TipoResiduo { get; set; }

        [JsonPropertyName("tecnico")]
        public string Tecnico { get; set; }

        [JsonPropertyName("fechaServicio")]
        public DateTime? FechaServicio { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("observaciones")]
        public string Observaciones { get; set; }

        [JsonPropertyName("cantidadEstimada")]
        public double? CantidadEstimada { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
    }
}