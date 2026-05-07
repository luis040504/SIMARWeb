using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClienteWeb.Pages.WasteTraceability.ConsultWasteHistory
{
    public class ServicioHistorial
    {
        public string? Id { get; set; }
        public string? Cliente { get; set; }
        public string? Observaciones { get; set; }
        public string? Manifiesto { get; set; }
        public string? TecnicoAsignado { get; set; }
        public string? OperadorAsignado { get; set; }
        public string? Vehiculo { get; set; }
        public string? TipoResiduo { get; set; }
        public double CantidadEstimada { get; set; }
        public string? Estado { get; set; }
        public DateTime FechaServicio { get; set; }
        public string? Direccion { get; set; }
        public string? Contrato { get; set; }
        public string? Conductor { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public int Count { get; set; }
    }

    public class ServicioHistorialResponse
    {
        [JsonPropertyName("_id")]
        public string? Id { get; set; }

        [JsonPropertyName("cliente")]
        public string? Cliente { get; set; }

        [JsonPropertyName("observaciones")]
        public string? Observaciones { get; set; }

        [JsonPropertyName("manifiesto")]
        public string? Manifiesto { get; set; }

        [JsonPropertyName("tecnico")]
        public string? TecnicoAsignado { get; set; }

        [JsonPropertyName("operadorAsignado")]
        public string? OperadorAsignado { get; set; }

        [JsonPropertyName("vehiculo")]
        public string? Vehiculo { get; set; }

        [JsonPropertyName("tipoResiduo")]
        public string? TipoResiduo { get; set; }

        [JsonPropertyName("cantidadEstimada")]
        public double? CantidadEstimada { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }

        [JsonPropertyName("fechaServicio")]
        public DateTime? FechaServicio { get; set; }

        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }

        [JsonPropertyName("contrato")]
        public string? Contrato { get; set; }

        [JsonPropertyName("conductor")]
        public string? Conductor { get; set; }
    }

    public class ServiceHistoryModel : PageModel
    {
        private const string ServiciosApiUrlTemplate = "http://localhost:8005/api/servicios/{0}";
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceHistoryModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<ServicioHistorial> ServiciosHistorial { get; set; } = new();
        public string? ClienteSeleccionado { get; set; }
        public string? Rol { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync(int? id, string cliente, string rol = "empresa")
        {
            Rol = rol;
            ViewData["Rol"] = rol;
            ClienteSeleccionado = cliente ?? "Cliente no especificado";

            if (string.IsNullOrWhiteSpace(cliente))
            {
                ServiciosHistorial = new List<ServicioHistorial>();
                return;
            }

            await LoadServiceHistoryAsync(id, cliente);
        }

        private async Task LoadServiceHistoryAsync(int? id, string cliente)
        {
            try
            {
                var requestUrl = string.Format(ServiciosApiUrlTemplate, Uri.EscapeDataString(cliente));
                using var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.GetFromJsonAsync<ApiResponse<List<ServicioHistorialResponse>>>(
                    requestUrl,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (response is null)
                {
                    ErrorMessage = "El cliente no existe.";
                    ServiciosHistorial = new List<ServicioHistorial>();
                    return;
                }

                if (!response.Success || response.Data is null)
                {
                    ErrorMessage = "No se pudo obtener el historial de servicios para este cliente.";
                    ServiciosHistorial = new List<ServicioHistorial>();
                    return;
                }

                ServiciosHistorial = response.Data.Select(MapToViewModel).ToList();
            }
            catch (Exception)
            {
                ErrorMessage = "Ocurrió un error al consultar el backend de servicios.";
                ServiciosHistorial = new List<ServicioHistorial>();
            }
        }

        private static ServicioHistorial MapToViewModel(ServicioHistorialResponse dto)
        {
            return new ServicioHistorial
            {
                Id = dto.Id ?? string.Empty,
                Cliente = dto.Cliente ?? string.Empty,
                Observaciones = dto.Observaciones ?? string.Empty,
                Manifiesto = dto.Manifiesto ?? string.Empty,
                TecnicoAsignado = dto.TecnicoAsignado ?? string.Empty,
                OperadorAsignado = dto.OperadorAsignado ?? string.Empty,
                Vehiculo = dto.Vehiculo ?? string.Empty,
                TipoResiduo = dto.TipoResiduo ?? string.Empty,
                CantidadEstimada = dto.CantidadEstimada ?? 0.0,
                Estado = dto.Estado ?? string.Empty,
                FechaServicio = dto.FechaServicio ?? DateTime.Today,
                Direccion = dto.Direccion ?? string.Empty,
                Contrato = dto.Contrato ?? string.Empty,
                Conductor = dto.Conductor ?? string.Empty
            };
        }
    }
}
