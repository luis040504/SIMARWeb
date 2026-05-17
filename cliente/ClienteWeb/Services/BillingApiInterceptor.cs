using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClienteWeb.Services
{
    public class BillingApiInterceptor : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw new BillingApiException("No se pudo establecer comunicación con el servicio de facturación. Verifique su conexión o intente más tarde.", 503, ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                int statusCode = (int)response.StatusCode;
                string content = await response.Content.ReadAsStringAsync();
                string friendlyMessage = "Ocurrió un error inesperado. Por favor, inténtelo más tarde.";

                try
                {
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        using var document = JsonDocument.Parse(content);
                        JsonElement errorElement;
                        
                        if (document.RootElement.TryGetProperty("detail", out errorElement) || 
                            document.RootElement.TryGetProperty("details", out errorElement))
                        {
                            if (errorElement.ValueKind == JsonValueKind.Array)
                            {
                                var errors = new List<string>();
                                foreach (var err in errorElement.EnumerateArray())
                                {
                                    if (err.TryGetProperty("msg", out var msgElement))
                                    {
                                        var loc = err.TryGetProperty("loc", out var locElement) ? string.Join(".", locElement.EnumerateArray()) : "unknown";
                                        errors.Add($"{loc}: {msgElement.GetString()}");
                                    }
                                    else
                                    {
                                        errors.Add(err.ToString());
                                    }
                                }
                                friendlyMessage = string.Join(" | ", errors);
                            }
                            else
                            {
                                friendlyMessage = errorElement.GetString() ?? friendlyMessage;
                            }
                        }
                        
                        if (document.RootElement.TryGetProperty("message", out var messageElement))
                        {
                            var msg = messageElement.GetString();
                            if (!string.IsNullOrEmpty(msg) && msg != "Error de validación en los datos de entrada")
                            {
                                friendlyMessage = msg;
                            }
                        }
                    }
                }
                catch
                {
                    // If parsing fails, fall back to default logic
                }

                if (statusCode >= 400 && statusCode < 500)
                {
                    if (statusCode == 422)
                    {
                        friendlyMessage = $"Error de validación de datos: {friendlyMessage}";
                    }
                    else if (statusCode == 404)
                    {
                        friendlyMessage = "El recurso solicitado no fue encontrado.";
                    }
                    throw new BillingApiException(friendlyMessage, statusCode);
                }
                else if (statusCode >= 500)
                {
                    throw new BillingApiException($"Error interno del servidor de facturación: {friendlyMessage}", statusCode);
                }
            }

            return response;
        }
    }
}
