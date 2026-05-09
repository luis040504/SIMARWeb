using ClienteWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddTransient<BillingApiInterceptor>();

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
