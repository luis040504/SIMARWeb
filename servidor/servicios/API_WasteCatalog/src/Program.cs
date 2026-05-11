using API_WasteCatalog.Data;
using API_WasteCatalog.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CatalogDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWasteCatalogService, WasteCatalogService>();

builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter("catalog", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 10;
    });
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.EnsureCreated();
    WasteSeeder.Seed(db);
}

app.MapOpenApi();
app.MapScalarApiReference(opts => opts.WithTitle("SIMAR — Catálogo de Residuos"));

app.UseRateLimiter();
app.MapControllers().RequireRateLimiting("catalog");

app.Run();

public partial class Program { }
