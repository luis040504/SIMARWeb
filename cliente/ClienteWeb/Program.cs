using QuestPDF.Infrastructure;
using ClienteWeb.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

/* Redirigir la raíz a tu página específica
app.MapGet("/", context => {
    context.Response.Redirect("/WasteTraceability/RegisterWasteCollection/IndexRegisterWasteCollection");
    return Task.CompletedTask;
});*/

app.Run();
