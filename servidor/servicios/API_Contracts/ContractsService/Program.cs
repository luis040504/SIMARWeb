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

// HttpClient hacia clientes_api (nombre de servicio en Docker)
builder.Services.AddHttpClient<IClientesApiService, ClientesApiService>(client =>
{
    var url = builder.Configuration["ServiceUrls:ClientesApi"] ?? "http://clientes_api:8005";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// HttpClient hacia catalog_api (nombre de servicio en Docker)
builder.Services.AddHttpClient<ICatalogApiService, CatalogApiService>(client =>
{
    var url = builder.Configuration["ServiceUrls:CatalogApi"] ?? "http://catalog_api:8010";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// HttpClient hacia manifiestos_api (nombre de servicio en Docker)
builder.Services.AddHttpClient("ManifestosApi", client =>
{
    var url = builder.Configuration["ServiceUrls:ManifestosApi"] ?? "http://manifiestos_api:8007";
    client.BaseAddress = new Uri(url);
    client.Timeout = TimeSpan.FromSeconds(15);
});

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
    int retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Migración de base de datos completada exitosamente.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            if (retries == 0)
            {
                Console.WriteLine("Error crítico: no se pudo migrar la base de datos después de varios intentos.");
                throw;
            }
            Console.WriteLine($"Error al conectar o migrar la base de datos (intentos restantes: {retries}): {ex.Message}. Reintentando en 5 segundos...");
            Thread.Sleep(5000);
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Esto es lo que hace que cargue la interfaz visual
}

app.UseCors();

// Solo forzar HTTPS si no estamos en desarrollo (Docker usa HTTP en el 8006)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapPost("/api/contracts", async (HttpContext httpContext, Contract contractRequest, IContractService contractService) =>
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
        string token = "";
        var authHeader = httpContext.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authHeader["Bearer ".Length..].Trim();
        }

        var result = await contractService.CreateContractAsync(contractRequest, token);
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

// Verifica si un cliente ya existe en clientes_api por RFC.
// Devuelve { exists: bool, clientId: int, clientName: string }
// No crea ni modifica nada — solo consulta.
app.MapGet("/api/contracts/check-client", async (string rfc, IClientesApiService clientesApi) =>
{
    if (string.IsNullOrWhiteSpace(rfc))
        return Results.BadRequest(new { error = "RFC requerido." });

    var cliente = await clientesApi.BuscarPorRfcAsync(rfc);

    if (cliente != null)
        return Results.Ok(new { exists = true,  clientId = cliente.Id, clientName = cliente.Name });

    return Results.Ok(new { exists = false, clientId = 0, clientName = "" });
}).WithName("CheckClientExists");

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

app.MapGet("/api/contracts/{id:int}/download", async (int id, bool? inline, IContractService contractService) =>
{
    try
    {
        var (content, contentType, fileName) = await contractService.GetContractPdfAsync(id);
        if (inline == true)
        {
            return Results.File(content, contentType);
        }
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
            ServicesRawJson = root.GetProperty("services").GetRawText(),
            Frequency = root.GetProperty("frequency").GetProperty("description").GetString() ?? ""
        };

        db.Quotations.Add(mirroredQuote);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = "Cotización sincronizada correctamente en la BD." });
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

// ──────────────────────────────────────────────────────────────────────────────
// Endpoint espejo para Manifiestos
// Devuelve los servicios de una cotización con el campo "wastetype" que
// espera API_MANIFEST, mapeando:
//   JSON cotización: { name, type, unit }  →  { wastetype, unit }
// GET /api/quotations/{id}/manifest-data
// ──────────────────────────────────────────────────────────────────────────────
app.MapGet("/api/quotations/{id:int}/manifest-data", async (int id, ContractsDbContext db, IClientesApiService clientesApi) =>
{
    var quote = await db.Quotations.FindAsync(id);
    if (quote == null) return Results.NotFound(new { error = "Cotización no encontrada." });

    int clientId = 0;
    if (!string.IsNullOrWhiteSpace(quote.ClientRfc))
    {
        var cliente = await clientesApi.BuscarPorRfcAsync(quote.ClientRfc);
        if (cliente != null)
        {
            clientId = cliente.Id;
        }
    }

    // Parsear los servicios raw almacenados en ServicesRawJson
    var wasteItems = new List<object>();
    try
    {
        var rawServices = JsonSerializer.Deserialize<List<JsonElement>>(quote.ServicesRawJson);
        if (rawServices != null)
        {
            foreach (var svc in rawServices)
            {
                // Extraer ubicación del servicio
                string address = "";
                if (svc.TryGetProperty("location", out var loc) && loc.ValueKind == JsonValueKind.Object)
                {
                    var street = loc.TryGetProperty("street",       out var st) ? st.GetString() ?? "" : "";
                    var muni   = loc.TryGetProperty("municipality", out var mu) ? mu.GetString() ?? "" : "";
                    address = $"{street}, {muni}".Trim(',', ' ');
                }

                // Cada servicio puede tener múltiples residuos en "wastes"
                if (svc.TryGetProperty("wastes", out var wastes) && wastes.ValueKind == JsonValueKind.Array)
                {
                    foreach (var w in wastes.EnumerateArray())
                    {
                        // "name" = nombre del residuo, "type" = categoría/tipo, "clave" = code
                        // Se devuelven como campos separados tal como los espera Manifiestos
                        var name = w.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                            ? nameProp.GetString() ?? ""
                            : "";
                        var type = w.TryGetProperty("type", out var typeProp) && typeProp.ValueKind == JsonValueKind.String
                            ? typeProp.GetString() ?? ""
                            : "";
                        var code = w.TryGetProperty("clave", out var codeProp) && codeProp.ValueKind == JsonValueKind.String
                            ? codeProp.GetString() ?? ""
                            : "";
                        var unit = w.TryGetProperty("unit", out var u) ? u.GetString() ?? "" : "";

                        wasteItems.Add(new
                        {
                            code           = code,
                            name           = name,
                            type           = type,
                            unit           = unit,
                            serviceAddress = address
                        });
                    }
                }
            }
        }
    }
    catch { /* JSON malformado: devolver lista vacía */ }

    return Results.Ok(new
    {
        quotationId  = quote.Id,
        folio        = quote.Folio,
        clientId     = clientId,
        clientName   = quote.ClientName,
        clientRfc    = quote.ClientRfc,
        status       = quote.Status,
        frequency    = quote.Frequency,
        total        = quote.Total,
        wastes       = wasteItems
    });
});

app.MapGet("/api/contracts/{id:int}/manifest-data",
async (
    int id,
    ContractsDbContext db,
    IClientesApiService clientesApi,
    ICatalogApiService catalogApi
) =>
{
    var contract = await db.Contracts
        .Include(c => c.Services)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (contract == null)
        return Results.NotFound();

    var cliente =
        await clientesApi.ObtenerPorIdAsync(contract.ClientId);

    var quote = await db.Quotations.FindAsync(contract.QuotationId);

    // Normaliza el string largo de tipo a "peligroso" o "especial"
    static string NormalizeWasteType(string raw)
    {
        var lower = raw.ToLowerInvariant();
        if (lower.Contains("peligroso") || lower.StartsWith("rp"))
            return "peligroso";
        return "especial";
    }

    var serviceItems = new List<object>();
    var flatWastes   = new List<object>();
    int serviceIndex = 1;
    bool parsedQuotation = false;

    if (quote != null && !string.IsNullOrEmpty(quote.ServicesRawJson))
    {
        try
        {
            var rawServices = JsonSerializer.Deserialize<List<JsonElement>>(quote.ServicesRawJson);
            if (rawServices != null)
            {
                foreach (var svc in rawServices)
                {
                    // 1. Extraer ID y Actividad
                    int sId = svc.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number
                        ? idProp.GetInt32()
                        : serviceIndex++;

                    string activity = svc.TryGetProperty("activity", out var actProp)
                        ? actProp.GetString() ?? ""
                        : (svc.TryGetProperty("name", out var nameProp)
                            ? nameProp.GetString() ?? ""
                            : (svc.TryGetProperty("description", out var descProp)
                                ? descProp.GetString() ?? ""
                                : ""));

                    // 2. Frequency mapping
                    object freqObj = null;
                    if (svc.TryGetProperty("frequency", out var freqProp))
                    {
                        if (freqProp.ValueKind == JsonValueKind.Object)
                        {
                            var fType = freqProp.TryGetProperty("type", out var ft) ? ft.GetString() ?? "" : "";
                            var fDur = freqProp.TryGetProperty("duration", out var fd)
                                ? (fd.ValueKind == JsonValueKind.Number ? fd.GetInt32().ToString() : fd.GetString() ?? "")
                                : "";
                            freqObj = new { type = fType, duration = fDur };
                        }
                        else if (freqProp.ValueKind == JsonValueKind.String)
                        {
                            freqObj = new { type = freqProp.GetString() ?? "", duration = "" };
                        }
                    }
                    if (freqObj == null)
                    {
                        freqObj = new { type = quote.Frequency, duration = "" };
                    }

                    // 3. Location mapping
                    string locStreet = "";
                    string locMuni   = "";
                    object locObj = new { cp = "", street = "", municipality = "", neighborhood = "", state = "" };

                    if (svc.TryGetProperty("location", out var loc) && loc.ValueKind == JsonValueKind.Object)
                    {
                        var cp    = loc.TryGetProperty("cp", out var cpProp)
                            ? (cpProp.ValueKind == JsonValueKind.Number ? cpProp.GetInt32().ToString() : cpProp.GetString() ?? "")
                            : "";
                        locStreet = loc.TryGetProperty("street",       out var st) ? st.GetString() ?? "" : "";
                        locMuni   = loc.TryGetProperty("municipality", out var mu) ? mu.GetString() ?? "" : "";
                        var neigh = loc.TryGetProperty("neighborhood", out var ne) ? ne.GetString() ?? "" : "";
                        var state = loc.TryGetProperty("state",        out var sa) ? sa.GetString() ?? "" : "";

                        locObj = new { cp, street = locStreet, municipality = locMuni, neighborhood = neigh, state };
                    }

                    // 4. Wastes mapping
                    var serviceWastes = new List<object>();
                    if (svc.TryGetProperty("wastes", out var wastes) && wastes.ValueKind == JsonValueKind.Array)
                    {
                        string serviceAddress = $"{locStreet}, {locMuni}".Trim(',', ' ');

                        foreach (var w in wastes.EnumerateArray())
                        {
                            var wName = w.TryGetProperty("name", out var wNameProp) && wNameProp.ValueKind == JsonValueKind.String
                                ? wNameProp.GetString() ?? "" : "";
                            var rawType = w.TryGetProperty("type", out var wTypeProp) && wTypeProp.ValueKind == JsonValueKind.String
                                ? wTypeProp.GetString() ?? "" : "";
                            // Intentar leer "code" primero; caer a "clave" para compatibilidad
                            var wCode = w.TryGetProperty("code", out var wCodeProp) && wCodeProp.ValueKind == JsonValueKind.String
                                ? wCodeProp.GetString() ?? ""
                                : (w.TryGetProperty("clave", out var wClaveProp) && wClaveProp.ValueKind == JsonValueKind.String
                                    ? wClaveProp.GetString() ?? "" : "");
                            var wUnit = w.TryGetProperty("unit", out var u) ? u.GetString() ?? "" : "";
                            var normalizedType = NormalizeWasteType(rawType);

                            var wasteEntry = new
                            {
                                code           = wCode,
                                name           = wName,
                                type           = normalizedType,
                                unit           = wUnit,
                                serviceAddress = serviceAddress
                            };
                            serviceWastes.Add(wasteEntry);
                            flatWastes.Add(wasteEntry);
                        }
                    }

                    serviceItems.Add(new
                    {
                        id       = sId,
                        activity = activity,
                        frequency = freqObj,
                        location  = locObj,
                        wastes    = serviceWastes
                    });
                }
                parsedQuotation = true;
            }
        }
        catch { /* Fallback to standard flow */ }
    }

    // FALLBACK: Si no se pudo parsear la cotización o está vacía, usamos los servicios del contrato y el catálogo
    if (!parsedQuotation || serviceItems.Count == 0)
    {
        var allWastes = await catalogApi.GetActiveWastesAsync();

        foreach (var s in contract.Services)
        {
            var match = allWastes.FirstOrDefault(w =>
                w.Name.Equals(s.WasteType, StringComparison.OrdinalIgnoreCase) ||
                w.Code.Equals(s.WasteType, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                match = allWastes.FirstOrDefault(w =>
                    s.WasteType.Contains(w.Name, StringComparison.OrdinalIgnoreCase) ||
                    w.Name.Contains(s.WasteType, StringComparison.OrdinalIgnoreCase));
            }

            if (match == null)
            {
                string lowerType = s.WasteType.ToLower();
                if (lowerType.Contains("cartón") || lowerType.Contains("carton") || lowerType.Contains("papel"))
                {
                    match = allWastes.FirstOrDefault(w => w.Code == "RO-002");
                }
                else if (lowerType.Contains("pet") || lowerType.Contains("plástico") || lowerType.Contains("plastico"))
                {
                    match = allWastes.FirstOrDefault(w => w.Code == "RI-010");
                }
                else if (lowerType.Contains("rpbi") || lowerType.Contains("biológico") || lowerType.Contains("biologico") || lowerType.Contains("infeccioso"))
                {
                    match = allWastes.FirstOrDefault(w => w.Code == "RPBI-004");
                }
                else if (lowerType.Contains("aceite"))
                {
                    match = allWastes.FirstOrDefault(w => w.Code == "RP-002");
                }
                else if (lowerType.Contains("batería") || lowerType.Contains("bateria"))
                {
                    match = allWastes.FirstOrDefault(w => w.Code == "RP-010");
                }
                else if (lowerType.Contains("lámpara") || lowerType.Contains("lampara") || lowerType.Contains("foco"))
                {
                    match = allWastes.FirstOrDefault(w => w.Code == "RP-022");
                }
            }

            string determinedType = "especial";
            if (match != null)
            {
                determinedType = match.Type;
            }
            else
            {
                string lowerType = s.WasteType.ToLower();
                if (lowerType.Contains("rp") || lowerType.Contains("peligroso") || lowerType.Contains("biologico") || lowerType.Contains("biológico") || lowerType.Contains("infeccioso"))
                {
                    determinedType = "peligroso";
                }
            }

            var fallbackWaste = new
            {
                code           = match?.Code ?? "",
                name           = s.WasteType,
                type           = determinedType,
                unit           = s.WasteUnit,
                serviceAddress = s.ServiceAddress
            };

            serviceItems.Add(new
            {
                id        = s.Id,
                activity  = s.WasteType,
                frequency = new { type = s.Frequency, duration = "" },
                location  = new { cp = "", street = s.ServiceAddress, municipality = "", neighborhood = "", state = "" },
                wastes    = new List<object> { fallbackWaste }
            });
            flatWastes.Add(fallbackWaste);
        }
    }

    return Results.Ok(new
    {
        contractId  = contract.Id,
        quotationId = contract.QuotationId,
        folio       = contract.Folio,
        clientId    = contract.ClientId,
        clientName  = cliente?.BusinessName ?? cliente?.Name,
        clientRfc   = cliente?.Rfc,
        status      = contract.Status,
        frequency   = contract.Services.FirstOrDefault()?.Frequency ?? "",
        total       = contract.TotalBasePrice,
        wastes      = flatWastes,
        services    = serviceItems
    });
});


app.MapPost("/api/contracts/{id:int}/upload-pdf", async (int id, IFormFile file, ContractsDbContext db) =>
{
    try
    {
        if (file == null || file.Length == 0) return Results.BadRequest("Archivo no válido.");
        var uploads = Path.Combine(app.Environment.ContentRootPath, "uploads", "signed");
        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
        var fileName = $"Signed_{id}_{Guid.NewGuid().ToString()[..8]}.pdf";
        var filePath = Path.Combine(uploads, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
        var contract = await db.Contracts.FindAsync(id);
        if (contract != null) { 
            contract.SignedContractPath = filePath; 
            contract.Status = "Activo";
            await db.SaveChangesAsync(); 
        }
        return Results.Ok(new { path = filePath });
    }
    catch (Exception ex) { return Results.Problem(ex.Message); }
}).DisableAntiforgery();

app.MapPatch("/api/contracts/{id:int}/cancel", async (int id, CancelContractRequest request, ContractsDbContext db, IHttpClientFactory httpClientFactory) =>
{
    var contract = await db.Contracts.FindAsync(id);
    if (contract is null)
        return Results.NotFound(new { error = "Contrato no encontrado." });

    if (contract.Status == "Cancelado")
        return Results.Ok(new { message = "El contrato ya estaba cancelado.", cancelledManifests = 0 });

    contract.Status = "Cancelado";
    contract.CancellationReason = request.CancellationReason;
    await db.SaveChangesAsync();

    var http = httpClientFactory.CreateClient("ManifestosApi");
    int cancelledCount = 0;
    var errors = new List<string>();

    try
    {
        var listResponse = await http.GetAsync($"/api/manifiestos?contrato_id={id}");
        if (listResponse.IsSuccessStatusCode)
        {
            var json = await listResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            JsonElement manifests;
            // El API puede devolver { data: [...] } o directamente [...]
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("data", out manifests))
            {
                // ok
            }
            else
            {
                manifests = doc.RootElement;
            }

            if (manifests.ValueKind == JsonValueKind.Array)
            {
                foreach (var m in manifests.EnumerateArray())
                {
                    var estado = m.TryGetProperty("estado", out var e) ? e.GetString() : null;
                    if (estado == "cancelado") continue;

                    if (!m.TryGetProperty("id", out var idProp)) continue;
                    var manifestId = idProp.GetInt32();

                    var body = new StringContent(
                        JsonSerializer.Serialize(new { estado = "cancelado" }),
                        System.Text.Encoding.UTF8,
                        "application/json");

                    var patchResp = await http.PatchAsync($"/api/manifiestos/{manifestId}/estado", body);
                    if (patchResp.IsSuccessStatusCode)
                        cancelledCount++;
                    else
                        errors.Add($"Manifiesto {manifestId}: {patchResp.StatusCode}");
                }
            }
        }
        else
        {
            errors.Add($"No se pudo consultar manifiestos: {listResponse.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        errors.Add($"Error al comunicarse con API de manifiestos: {ex.Message}");
    }

    return Results.Ok(new
    {
        message = "Contrato cancelado.",
        cancelledManifests = cancelledCount,
        errors
    });
}).WithName("CancelContract");

app.Run();

public record CancelContractRequest(string? CancellationReason);