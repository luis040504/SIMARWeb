using ClienteWeb.Services;

using QuestPDF.Infrastructure;
using ClienteWeb.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Billing API Configuration
builder.Services.AddTransient<ClienteWeb.Services.BillingApiInterceptor>();
builder.Services.AddHttpClient<ClienteWeb.Services.IBillingService, ClienteWeb.Services.BillingService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BillingApiUrl"] ?? "http://localhost:8009");
})
.AddHttpMessageHandler<ClienteWeb.Services.BillingApiInterceptor>();

// PDF Generation
QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddScoped<IInvoiceGeneratorService, InvoiceGeneratorService>();



builder.Services.AddHttpClient<ManifestApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost/api/");
}).AddHttpMessageHandler<BillingApiInterceptor>();

builder.Services.AddHttpClient("ContractsApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8006"); 
});

builder.Services.AddHttpClient<ClientesApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ClientesApiBaseUrl"] ?? "http://localhost/api/clientes/");
});

builder.Services.AddHttpClient<VehiculosApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["VehiculosApiBaseUrl"] ?? "http://localhost/api/vehiculos/");
});

builder.Services.AddHttpClient<WasteCatalogApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["WasteCatalogApiBaseUrl"] ?? "http://localhost/api/catalog/");
});

builder.Services.AddHttpClient("ClientesApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8005");
});

builder.Services.AddHttpClient("AuthApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8001");
});

builder.Services.AddHttpClient("UsuariosApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8002");
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient("UserApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8002");
});

builder.Services.AddHttpClient("EmpleadoApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:8008");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseRouting();

app.UseSession();

app.UseAuthorization();



app.MapGet("/", context =>
{
    context.Response.Redirect("/inicio");
    return Task.CompletedTask;
});

app.MapPost("/logout", context =>
{
    context.Session.Clear();

    context.Response.Redirect("/inicio");

    return Task.CompletedTask;
});


app.MapRazorPages()
   .WithStaticAssets();

app.Run();
