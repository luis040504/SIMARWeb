using ContractsService.Data;
using ContractsService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ContractsService.Services;

public class ClienteApiDto
{
    [JsonPropertyName("id")]           public int    Id            { get; set; }
    [JsonPropertyName("name")]         public string Name          { get; set; } = "";
    [JsonPropertyName("businessName")] public string BusinessName  { get; set; } = "";
    [JsonPropertyName("rfc")]          public string Rfc           { get; set; } = "";
    [JsonPropertyName("contactEmail")] public string ContactEmail  { get; set; } = "";
    [JsonPropertyName("phone")]        public string Phone         { get; set; } = "";
    [JsonPropertyName("address")]      public string Address       { get; set; } = "";
}

public interface IClientesApiService
{
    Task<ClienteApiDto?> BuscarPorRfcAsync(string rfc);
    Task<ClienteApiDto?> ObtenerPorIdAsync(int id);
    Task<ClienteApiDto?> CrearClienteAsync(ClienteApiDto cliente, string token);
}

public class ClientesApiService : IClientesApiService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _json =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public ClientesApiService(HttpClient http) => _http = http;

    public async Task<ClienteApiDto?> BuscarPorRfcAsync(string rfc)
    {
        try
        {
            var response = await _http.GetAsync($"/client/rfc/{Uri.EscapeDataString(rfc)}");
            if (!response.IsSuccessStatusCode) return null;
            var lista = await response.Content.ReadFromJsonAsync<List<ClienteApiDto>>(_json);
            return lista?.FirstOrDefault();
        }
        catch { return null; }
    }

    public async Task<ClienteApiDto?> ObtenerPorIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"/client/id/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ClienteApiDto>(_json);
        }
        catch { return null; }
    }

    public async Task<ClienteApiDto?> CrearClienteAsync(ClienteApiDto cliente, string token)
    {
        try
        {
            var payload = new
            {
                name         = cliente.Name,
                businessName = cliente.BusinessName,
                rfc          = cliente.Rfc,
                contactEmail = cliente.ContactEmail,
                phone        = cliente.Phone,
                address      = cliente.Address,
                semarnatNum  = "PENDIENTE"
            };

            // 🛠️ Construimos la petición manualmente para adjuntar el JWT
            using var request = new HttpRequestMessage(HttpMethod.Post, "/client");
            request.Content = JsonContent.Create(payload);
            
            // Inyectar el token en las cabeceras
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ClienteApiDto>(_json);
        }
        catch { return null; }
    }
}

public class WasteCatalogDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("code")] public string Code { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
}

public class CatalogPagedResultDto
{
    [JsonPropertyName("items")] public List<WasteCatalogDto> Items { get; set; } = new();
    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
}

public class ContractWasteDto
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("code")] public string Code { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
}

public interface ICatalogApiService
{
    Task<List<WasteCatalogDto>> GetActiveWastesAsync();
}

public class CatalogApiService : ICatalogApiService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _json =
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public CatalogApiService(HttpClient http) => _http = http;

    public async Task<List<WasteCatalogDto>> GetActiveWastesAsync()
    {
        try
        {
            var response = await _http.GetAsync("/api/catalog?pageSize=200");
            if (!response.IsSuccessStatusCode) return new();
            var result = await response.Content.ReadFromJsonAsync<CatalogPagedResultDto>(_json);
            return result?.Items ?? new();
        }
        catch { return new(); }
    }
}


public interface IContractService
{
    Task<ContractResponseDto> CreateContractAsync(Contract request, string token);
    Task<List<ContractListDto>> GetContractsAsync(string? search, string? status, DateTime? dateFilter);
    Task<ContractListDto> GetContractByIdAsync(int id);
    Task<ContractFullDetailDto> GetContractFullDetailAsync(int id);
    Task<ContractResponseDto> UpdateContractAsync(int id, Contract request);
    Task<(byte[] Content, string ContentType, string FileName)> GetContractPdfAsync(int id);
}

public class ContractService : IContractService
{
    private readonly ContractsDbContext _context;
    private readonly IClientesApiService _clientesApi;
    private readonly ICatalogApiService _catalogApi;

    public ContractService(ContractsDbContext context, IClientesApiService clientesApi, ICatalogApiService catalogApi)
    {
        _context     = context;
        _clientesApi = clientesApi;
        _catalogApi  = catalogApi;
    }

    public async Task<ContractResponseDto> CreateContractAsync(Contract request, string token)
    {
        request.Folio     = $"CON-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        request.CreatedAt = DateTime.UtcNow;
        request.Status    = "Pendiente de firma";

        var quotation = await _context.Quotations.FindAsync(request.QuotationId);

        if (quotation != null && !string.IsNullOrWhiteSpace(quotation.ClientRfc))
        {
            var clienteExistente = await _clientesApi.BuscarPorRfcAsync(quotation.ClientRfc);
            if (clienteExistente != null)
                request.ClientId = clienteExistente.Id;
        }

        _context.Contracts.Add(request);

        if (quotation != null)
            quotation.Status = "contracted";

        await _context.SaveChangesAsync();

        return new ContractResponseDto { Id = request.Id, Folio = request.Folio, Message = "Contrato creado exitosamente." };
    }

    public async Task<List<ContractListDto>> GetContractsAsync(string? search, string? status, DateTime? dateFilter)
    {
        var query = _context.Contracts.Include(c => c.Payments).AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);

        if (dateFilter.HasValue)
        {
            var filterDate = dateFilter.Value.Date;
            query = query.Where(c =>
                c.CreatedAt.Date <= filterDate &&
                c.Payments.Any(p => p.PaymentDate.Date >= filterDate));
        }

        var contracts = await query.ToListAsync();

        var result = new List<ContractListDto>();
        foreach (var c in contracts)
        {
            var cliente = await _clientesApi.ObtenerPorIdAsync(c.ClientId);
            var clientName = cliente?.BusinessName ?? cliente?.Name ?? "";

            // Filtro por nombre de cliente (post-fetch)
            if (!string.IsNullOrEmpty(search) &&
                !c.Folio.Contains(search, StringComparison.OrdinalIgnoreCase) &&
                !clientName.Contains(search, StringComparison.OrdinalIgnoreCase))
                continue;

            result.Add(new ContractListDto
            {
                Id              = c.Id,
                Folio           = c.Folio,
                ClientId        = c.ClientId,
                ClientName      = clientName,
                Status          = c.Status,
                CreatedAt       = c.CreatedAt,
                SignedContractPath = c.SignedContractPath,
                ExpirationDate  = c.EndDate ?? (c.Payments.Any() ? c.Payments.Max(p => p.PaymentDate) : (c.FirstServiceDate ?? DateTime.MinValue))
            });
        }
        return result;
    }

    public async Task<ContractListDto> GetContractByIdAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Payments)
            .Include(c => c.Services)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contract != null)
        {
            var cliente = await _clientesApi.ObtenerPorIdAsync(contract.ClientId);
            var clientName = cliente?.BusinessName ?? cliente?.Name ?? "";

            var allWastes = await _catalogApi.GetActiveWastesAsync();
            var mappedWastes = new List<ContractWasteDto>();
            foreach (var service in contract.Services)
            {
                var match = allWastes.FirstOrDefault(w => 
                    w.Name.Equals(service.WasteType, StringComparison.OrdinalIgnoreCase) || 
                    w.Code.Equals(service.WasteType, StringComparison.OrdinalIgnoreCase));
                
                if (match != null)
                {
                    mappedWastes.Add(new ContractWasteDto
                    {
                        Id = match.Id,
                        Code = match.Code,
                        Name = match.Name,
                        Type = match.Type
                    });
                }
                else
                {
                    mappedWastes.Add(new ContractWasteDto
                    {
                        Id = 0,
                        Code = "",
                        Name = service.WasteType,
                        Type = "especial"
                    });
                }
            }

            var firstWaste = mappedWastes.FirstOrDefault();

            return new ContractListDto
            {
                Id = contract.Id, 
                Folio = contract.Folio, 
                ClientId = contract.ClientId, 
                ClientName = clientName,
                Status = contract.Status, 
                CreatedAt = contract.CreatedAt,
                ExpirationDate = contract.Payments.Any() ? contract.Payments.Max(p => p.PaymentDate) : (contract.FirstServiceDate ?? DateTime.MinValue),
                ResidueId = firstWaste?.Id,
                ResidueCode = firstWaste?.Code,
                ResidueName = firstWaste?.Name,
                ResidueType = firstWaste?.Type,
                Wastes = mappedWastes
            };
        }

        var quote = await _context.Quotations.FindAsync(id);
        if (quote == null) throw new KeyNotFoundException("Contrato o Cotización no encontrada.");

        var clienteCot = await _clientesApi.BuscarPorRfcAsync(quote.ClientRfc);

        var quoteWastes = new List<ContractWasteDto>();
        try
        {
            var rawServices = JsonSerializer.Deserialize<List<JsonElement>>(quote.ServicesRawJson);
            if (rawServices != null)
            {
                var allWastes = await _catalogApi.GetActiveWastesAsync();
                foreach (var s in rawServices)
                {
                    if (s.TryGetProperty("wastes", out var wastes) && wastes.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var w in wastes.EnumerateArray())
                        {
                            var wName = w.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                            var match = allWastes.FirstOrDefault(x => 
                                x.Name.Equals(wName, StringComparison.OrdinalIgnoreCase) || 
                                x.Code.Equals(wName, StringComparison.OrdinalIgnoreCase));

                            if (match != null)
                            {
                                quoteWastes.Add(new ContractWasteDto
                                {
                                    Id = match.Id,
                                    Code = match.Code,
                                    Name = match.Name,
                                    Type = match.Type
                                });
                            }
                            else
                            {
                                quoteWastes.Add(new ContractWasteDto
                                {
                                    Id = 0,
                                    Code = "",
                                    Name = wName,
                                    Type = w.TryGetProperty("type", out var t) ? t.GetString() ?? "especial" : "especial"
                                });
                            }
                        }
                    }
                }
            }
        }
        catch { }

        var firstQuoteWaste = quoteWastes.FirstOrDefault();

        return new ContractListDto
        {
            Id = quote.Id, 
            Folio = quote.Folio, 
            ClientId = clienteCot?.Id ?? 0, 
            ClientName = clienteCot?.BusinessName ?? clienteCot?.Name ?? quote.ClientName,
            Status = quote.Status, 
            CreatedAt = quote.CreatedAt,
            ExpirationDate = DateTime.MinValue,
            ResidueId = firstQuoteWaste?.Id,
            ResidueCode = firstQuoteWaste?.Code,
            ResidueName = firstQuoteWaste?.Name,
            ResidueType = firstQuoteWaste?.Type,
            Wastes = quoteWastes
        };
    }

    public async Task<ContractFullDetailDto> GetContractFullDetailAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Services)
            .Include(c => c.Payments)
            .Include(c => c.Extras)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract != null)
        {
            var cliente = await _clientesApi.ObtenerPorIdAsync(contract.ClientId);
            
            var allWastes = await _catalogApi.GetActiveWastesAsync();
            var mappedWastes = new List<ContractWasteDto>();
            foreach (var service in contract.Services)
            {
                var match = allWastes.FirstOrDefault(w => 
                    w.Name.Equals(service.WasteType, StringComparison.OrdinalIgnoreCase) || 
                    w.Code.Equals(service.WasteType, StringComparison.OrdinalIgnoreCase));
                
                if (match != null)
                {
                    mappedWastes.Add(new ContractWasteDto
                    {
                        Id = match.Id,
                        Code = match.Code,
                        Name = match.Name,
                        Type = match.Type
                    });
                }
                else
                {
                    mappedWastes.Add(new ContractWasteDto
                    {
                        Id = 0,
                        Code = "",
                        Name = service.WasteType,
                        Type = "especial"
                    });
                }
            }

            return new ContractFullDetailDto
            {
                Id                  = contract.Id,
                Folio               = contract.Folio,
                ClientId            = contract.ClientId,
                ClientName          = cliente?.BusinessName ?? cliente?.Name ?? "",
                ClientRfc           = cliente?.Rfc ?? "",
                ClientAddress       = cliente?.Address ?? "",
                Representative      = contract.Representative,
                Status              = contract.Status,
                CreatedAt           = contract.CreatedAt,
                TotalBasePrice      = contract.TotalBasePrice,
                ClientObjetoSocial  = contract.ClientObjetoSocial,
                ClientDeclaraciones = contract.ClientDeclaraciones,
                ContractDuration    = contract.ContractDuration,
                FirstServiceDate    = contract.FirstServiceDate,
                Services            = contract.Services,
                Payments            = contract.Payments,
                Extras              = contract.Extras,
                Wastes              = mappedWastes
            };
        }

        var quote = await _context.Quotations.FindAsync(id);
        if (quote == null) throw new KeyNotFoundException("Contrato o Cotización no encontrada.");

        // Para cotizaciones sin contrato, buscar el cliente por RFC
        var clienteCot = await _clientesApi.BuscarPorRfcAsync(quote.ClientRfc);

        var services = new List<ContractServiceItem>();
        var quoteWastes = new List<ContractWasteDto>();
        try {
            var rawServices = JsonSerializer.Deserialize<List<JsonElement>>(quote.ServicesRawJson);
            if (rawServices != null) {
                var allWastes = await _catalogApi.GetActiveWastesAsync();
                foreach (var s in rawServices) {
                    string address = "";
                    if (s.TryGetProperty("location", out var location) && location.ValueKind == JsonValueKind.Object) {
                        var street = location.TryGetProperty("street", out var st) ? st.GetString() : "";
                        var muni   = location.TryGetProperty("municipality", out var mu) ? mu.GetString() : "";
                        address = $"{street}, {muni}".Trim(',', ' ');
                    }
                    if (s.TryGetProperty("wastes", out var wastes) && wastes.ValueKind == JsonValueKind.Array) {
                        foreach (var w in wastes.EnumerateArray()) {
                            var wName = w.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                            services.Add(new ContractServiceItem {
                                ContractId    = quote.Id,
                                WasteType     = wName,
                                WasteUnit     = w.TryGetProperty("unit", out var u) ? u.GetString() ?? "" : "",
                                ServiceAddress = address
                            });

                            var match = allWastes.FirstOrDefault(x => 
                                x.Name.Equals(wName, StringComparison.OrdinalIgnoreCase) || 
                                x.Code.Equals(wName, StringComparison.OrdinalIgnoreCase));

                            if (match != null)
                            {
                                quoteWastes.Add(new ContractWasteDto
                                {
                                    Id = match.Id,
                                    Code = match.Code,
                                    Name = match.Name,
                                    Type = match.Type
                                });
                            }
                            else
                            {
                                quoteWastes.Add(new ContractWasteDto
                                {
                                    Id = 0,
                                    Code = "",
                                    Name = wName,
                                    Type = w.TryGetProperty("type", out var t) ? t.GetString() ?? "especial" : "especial"
                                });
                            }
                        }
                    }
                }
            }
        } catch { }

        return new ContractFullDetailDto
        {
            Id            = quote.Id,
            Folio         = quote.Folio,
            ClientId      = clienteCot?.Id ?? 0,
            ClientName    = clienteCot?.BusinessName ?? clienteCot?.Name ?? quote.ClientName,
            ClientRfc     = clienteCot?.Rfc ?? quote.ClientRfc,
            ClientAddress = clienteCot?.Address ?? "",
            Status        = quote.Status,
            CreatedAt     = quote.CreatedAt,
            TotalBasePrice = quote.Total,
            Services      = services,
            Wastes        = quoteWastes
        };
    }

    public async Task<ContractResponseDto> UpdateContractAsync(int id, Contract request)
    {
        var existing = await _context.Contracts
            .Include(c => c.Services).Include(c => c.Payments).Include(c => c.Extras)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existing == null) throw new KeyNotFoundException("Contrato no encontrado.");

        existing.Status = request.Status;
        existing.TotalBasePrice = request.TotalBasePrice;
        existing.Representative = request.Representative;
        existing.ClientObjetoSocial = request.ClientObjetoSocial;
        existing.ClientDeclaraciones = request.ClientDeclaraciones;
        existing.ContractDuration = request.ContractDuration;
        existing.FirstServiceDate = request.FirstServiceDate;
        existing.EndDate = request.EndDate;
        existing.SignedContractPath = request.SignedContractPath;

        if (!string.IsNullOrEmpty(existing.SignedContractPath)) {
            existing.Status = "Activo";
        }

        // Actualizar Servicios
        existing.Services.RemoveAll(e => !request.Services.Any(r => r.Id == e.Id && e.Id != 0));
        foreach (var item in request.Services) {
            var exItem = existing.Services.FirstOrDefault(e => e.Id == item.Id && e.Id != 0);
            if (exItem != null) _context.Entry(exItem).CurrentValues.SetValues(item); else existing.Services.Add(item);
        }

        // Actualizar Pagos
        existing.Payments.RemoveAll(e => !request.Payments.Any(r => r.Id == e.Id && e.Id != 0));
        foreach (var item in request.Payments) {
            var exItem = existing.Payments.FirstOrDefault(e => e.Id == item.Id && e.Id != 0);
            if (exItem != null) _context.Entry(exItem).CurrentValues.SetValues(item); else existing.Payments.Add(item);
        }

        // Actualizar Extras
        existing.Extras.RemoveAll(e => !request.Extras.Any(r => r.Id == e.Id && e.Id != 0));
        foreach (var item in request.Extras) {
            var exItem = existing.Extras.FirstOrDefault(e => e.Id == item.Id && e.Id != 0);
            if (exItem != null) _context.Entry(exItem).CurrentValues.SetValues(item); else existing.Extras.Add(item);
        }

        await _context.SaveChangesAsync();
        return new ContractResponseDto { Id = existing.Id, Folio = existing.Folio, Message = "Contrato actualizado exitosamente." };
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> GetContractPdfAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID inválido");
        
        var contract = await _context.Contracts
            .Include(c => c.Services)
            .Include(c => c.Payments)
            .Include(c => c.Extras)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (contract == null) throw new KeyNotFoundException("Contract not found");

        if (!string.IsNullOrEmpty(contract.SignedContractPath) && File.Exists(contract.SignedContractPath))
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(contract.SignedContractPath);
            return (fileBytes, "application/pdf", $"Contrato_Firmado_{contract.Folio}.pdf");
        }

        var cliente = await _clientesApi.ObtenerPorIdAsync(contract.ClientId);
        string clientName    = cliente?.BusinessName ?? cliente?.Name ?? "";
        string clientRfc     = cliente?.Rfc ?? "";
        string clientAddress = cliente?.Address ?? "";

        byte[] pdfBuffer = ContractPdfGenerator.Generate(contract, clientName, clientRfc, clientAddress);
        return (pdfBuffer, "application/pdf", $"{contract.Folio}.pdf");
    }
}

public class ContractListDto { public int Id { get; set; } public string Folio { get; set; } = ""; public int ClientId { get; set; } public string ClientName { get; set; } = ""; public string Status { get; set; } = ""; public DateTime CreatedAt { get; set; } public DateTime ExpirationDate { get; set; } public string? SignedContractPath { get; set; } public int? ResidueId { get; set; } public string? ResidueCode { get; set; } public string? ResidueName { get; set; } public string? ResidueType { get; set; } public List<ContractWasteDto> Wastes { get; set; } = new(); }
public class ContractResponseDto { public int Id { get; set; } public string Folio { get; set; } = ""; public string Message { get; set; } = ""; }

public class ContractFullDetailDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public int ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public string ClientRfc { get; set; } = "";
    public string Representative { get; set; } = "";
    public string ClientAddress { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public decimal TotalBasePrice { get; set; }
    public string ClientObjetoSocial { get; set; } = "";
    public string ClientDeclaraciones { get; set; } = "";
    public string ContractDuration { get; set; } = "";
    public DateTime? FirstServiceDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SignedContractPath { get; set; }
    public List<ContractServiceItem> Services { get; set; } = new();
    public List<ContractPaymentItem> Payments { get; set; } = new();
    public List<ContractExtra> Extras { get; set; } = new();
    public List<ContractWasteDto> Wastes { get; set; } = new();
}
