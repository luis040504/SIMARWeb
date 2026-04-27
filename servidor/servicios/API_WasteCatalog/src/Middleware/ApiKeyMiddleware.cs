namespace API_WasteCatalog.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly string _validKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _validKey = config["ApiKey"] ?? throw new InvalidOperationException("ApiKey is not configured.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var provided) ||
            !ConstantTimeEquals(_validKey, provided!))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or missing API key." });
            return;
        }

        await _next(context);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (var i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0;
    }
}
