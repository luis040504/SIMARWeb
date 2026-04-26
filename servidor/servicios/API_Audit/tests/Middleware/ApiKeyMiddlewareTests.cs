using API_Audit.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace API_Audit.Tests.Middleware;

public class ApiKeyMiddlewareTests
{
    private static ApiKeyMiddleware CreateMiddleware(RequestDelegate next, string apiKeyValue = "test-api-key")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "ApiKey:Value", apiKeyValue } })
            .Build();
        return new ApiKeyMiddleware(next, config);
    }

    private static DefaultHttpContext CreateContext(string? apiKey = null, string path = "/api/audit/log")
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        if (apiKey is not null)
            context.Request.Headers["X-Api-Key"] = apiKey;
        return context;
    }

    // ─── API Key válida ────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidApiKey_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(CreateContext(apiKey: "test-api-key"));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ValidApiKey_DoesNotReturn401()
    {
        var context = CreateContext(apiKey: "test-api-key");
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.NotEqual(401, context.Response.StatusCode);
    }

    // ─── API Key faltante ──────────────────────────────────────────────────────

    [Fact]
    public async Task MissingApiKey_Returns401()
    {
        var context = CreateContext(apiKey: null);
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task MissingApiKey_DoesNotCallNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(CreateContext(apiKey: null));

        Assert.False(nextCalled);
    }

    // ─── API Key incorrecta ────────────────────────────────────────────────────

    [Fact]
    public async Task InvalidApiKey_Returns401()
    {
        var context = CreateContext(apiKey: "wrong-key");
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task EmptyApiKey_Returns401()
    {
        var context = CreateContext(apiKey: "");
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task WhitespaceApiKey_Returns401()
    {
        var context = CreateContext(apiKey: "   ");
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }

    // ─── Endpoint /health (sin API Key) ───────────────────────────────────────

    [Fact]
    public async Task HealthEndpoint_WithoutApiKey_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(CreateContext(apiKey: null, path: "/health"));

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task HealthEndpoint_WithoutApiKey_DoesNotReturn401()
    {
        var context = CreateContext(apiKey: null, path: "/health");
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.NotEqual(401, context.Response.StatusCode);
    }

    // ─── Seguridad: comparación en tiempo constante ───────────────────────────

    [Fact]
    public async Task KeyWithSameLengthButDifferentChars_Returns401()
    {
        // "test-api-key" y "test-api-keX" tienen el mismo largo pero difieren
        var context = CreateContext(apiKey: "test-api-keX");
        var middleware = CreateMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
    }
}
