using ContractsService.Data;
using ContractsService.Models;
using ContractsService.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContractsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

builder.Services.AddScoped<IContractService, ContractService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContractsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();

app.MapPost("/api/contracts", async (Contract contractRequest, IContractService contractService) =>
{
    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(contractRequest);
    bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(contractRequest, validationContext, validationResults, true);
    
    if (!isValid)
    {
        var errors = validationResults.Select(v => v.ErrorMessage);
        return Results.BadRequest(new { error = "Errores de validación", detalles = errors });
    }

    try
    {
        var result = await contractService.CreateContractAsync(contractRequest);
        return Results.Created($"/api/contracts/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception)
    {
        return Results.Problem("Ocurrió un error interno en el servidor.");
    }
}).WithName("CreateContract");

app.MapGet("/api/contracts", async (string? search, string? status, DateTime? dateFilter, IContractService contractService) =>
{
    var contracts = await contractService.GetContractsAsync(search, status, dateFilter);
    return Results.Ok(contracts);
}).WithName("GetContracts");
 
app.MapGet("/api/contracts/{id:int}", async (int id, IContractService contractService) =>
{
    try
    {
        var contract = await contractService.GetContractByIdAsync(id);
        return Results.Ok(contract);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
}).WithName("GetContractById");

app.MapGet("/api/contracts/{id:int}/detail", async (int id, IContractService contractService) =>
{
    try
    {
        var detail = await contractService.GetContractFullDetailAsync(id);
        return Results.Ok(detail);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
}).WithName("GetContractFullDetail");

app.MapPut("/api/contracts/{id:int}", async (int id, Contract contractRequest, IContractService contractService) =>
{
    try
    {
        var result = await contractService.UpdateContractAsync(id, contractRequest);
        return Results.Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (Exception)
    {
        return Results.Problem("Error al actualizar el contrato.");
    }
}).WithName("UpdateContract");

app.MapGet("/api/contracts/{id:int}/download", async (int id, IContractService contractService) =>
{
    try
    {
        var (content, contentType, fileName) = await contractService.GetContractPdfAsync(id);
        return Results.File(content, contentType, fileName);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).WithName("DownloadContractPdf");

app.MapPost("/api/quotations/sync", async (JsonDocument rawJson, ContractsDbContext db) =>
{
    try
    {
        var root = rawJson.RootElement;
        
        var createdAtUnix = root.GetProperty("createdAt").GetInt64();
        var createdAtDate = DateTimeOffset.FromUnixTimeMilliseconds(createdAtUnix).UtcDateTime;

        var mirroredQuote = new Quotation
        {
            Id = root.GetProperty("id").GetInt32(),
            Folio = root.GetProperty("folio").GetString() ?? "",
            Status = root.GetProperty("status").GetString() ?? "",
            ClientName = root.GetProperty("clientName").GetString() ?? "",
            ClientRfc = root.GetProperty("clientRfc").GetString() ?? "",
            ContactName = root.GetProperty("contactName").GetString() ?? "",
            ContactPhone = root.GetProperty("contactPhone").GetString() ?? "",
            ContactEmail = root.GetProperty("contactEmail").GetString() ?? "",
            ValidityDays = root.GetProperty("validityDays").GetInt32(),
            Subtotal = root.GetProperty("subtotal").GetDecimal(),
            Total = root.GetProperty("total").GetDecimal(),
            CreatedAt = createdAtDate,
            ServicesRawJson = root.GetProperty("services").GetRawText()
        };

        db.Quotations.Add(mirroredQuote);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = "Cotización sincronizada correctamente en la BD Espejo." });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = "Formato de cotización inválido", details = ex.Message });
    }
}).WithName("SyncQuotation");

app.MapGet("/api/quotations", async (ContractsDbContext db) =>
{
    var list = await db.Quotations
        .Where(q => q.Status != "contracted")
        .Select(q => new { 
            Id = q.Id, 
            ClientName = q.ClientName, 
            ServiceType = "Múltiples Servicios",
            DateApproved = q.CreatedAt.ToString("yyyy-MM-dd") 
        }).ToListAsync();
        
    return Results.Ok(list);
}).WithName("GetAllQuotations");

app.MapGet("/api/quotations/{id:int}", async (int id, ContractsDbContext db) =>
{
    var quote = await db.Quotations.FindAsync(id);
    if (quote == null) return Results.NotFound();
    
    return Results.Ok(quote);
}).WithName("GetQuotationById");

app.Run();