using ContractsService.Data;
using ContractsService.Models;
using ContractsService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContractsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IContractService, ContractService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/contracts", async (Contract contractRequest, IContractService contractService) =>
{
    try
    {
        var result = await contractService.CreateContractAsync(contractRequest);
        
        return Results.Created($"/api/contracts/{result.Id}", result);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem("Ocurrió un error interno en el servidor.");
    }
})
.WithName("CreateContract");

app.MapGet("/api/contracts", async (
    string? search, 
    string? status, 
    DateTime? dateFilter, 
    IContractService contractService) =>
{
    var contracts = await contractService.GetContractsAsync(search, status, dateFilter);
    return Results.Ok(contracts);
})
.WithName("GetContracts");

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
})
.WithName("DownloadContractPdf");

app.Run();