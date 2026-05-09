namespace API_Audit.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // El endpoint /health no requiere API Key para que el gateway pueda sondearlo
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey)
            || string.IsNullOrWhiteSpace(providedKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "MISSING_API_KEY",
                message = "Se requiere el header X-Api-Key para consumir este servicio."
            });
            return;
        }

        var configuredKey = _configuration["ApiKey:Value"];

        if (string.IsNullOrEmpty(configuredKey) || !ConstantTimeEquals(providedKey!, configuredKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "INVALID_API_KEY",
                message = "API Key inválida."
            });
            return;
        }

        await _next(context);
    }

    // Comparación en tiempo constante para evitar ataques de temporización
    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (var i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0;
    }
}
