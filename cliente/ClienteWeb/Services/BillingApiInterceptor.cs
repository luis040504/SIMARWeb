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
            var response = await base.SendAsync(request, cancellationToken);

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
                        if (document.RootElement.TryGetProperty("detail", out var detailElement))
                        {
                            friendlyMessage = detailElement.GetString() ?? friendlyMessage;
                        }
                        else if (document.RootElement.TryGetProperty("message", out var messageElement))
                        {
                            friendlyMessage = messageElement.GetString() ?? friendlyMessage;
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
