using ContractsService.Data;
using ContractsService.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractsService.Services;

public interface IContractService
{
    Task<ContractResponseDto> CreateContractAsync(Contract request);
    Task<List<ContractListDto>> GetContractsAsync(string? search, string? status, DateTime? dateFilter);
    Task<ContractListDto> GetContractByIdAsync(int id);
    Task<ContractFullDetailDto> GetContractFullDetailAsync(int id);
    Task<ContractResponseDto> UpdateContractAsync(int id, Contract request);
    Task<(byte[] Content, string ContentType, string FileName)> GetContractPdfAsync(int id);
}

public class ContractService : IContractService
{
    private readonly ContractsDbContext _context;

    public ContractService(ContractsDbContext context)
    {
        _context = context;
    }

public async Task<ContractResponseDto> CreateContractAsync(Contract request)
    {
        request.Folio = $"CON-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        request.CreatedAt = DateTime.UtcNow;
        request.Status = "Pendiente de firma";

        _context.Contracts.Add(request);

        var quote = await _context.Quotations.FindAsync(request.ClientId);
        if (quote != null) 
        {
            quote.Status = "contracted";
        }

        await _context.SaveChangesAsync();
        
        return new ContractResponseDto { Id = request.Id, Folio = request.Folio, Message = "Contrato creado exitosamente." };
    }

    public async Task<List<ContractListDto>> GetContractsAsync(string? search, string? status, DateTime? dateFilter)
    {
        var query = _context.Contracts.Include(c => c.Payments).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Folio.Contains(search) || c.ClientId.ToString() == search);
        }
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }
        if (dateFilter.HasValue)
        {
            // Busca contratos cuyo primer servicio sea en la fecha seleccionada
            query = query.Where(c => c.FirstServiceDate.HasValue && c.FirstServiceDate.Value.Date == dateFilter.Value.Date);
        }

        return await query.Select(c => new ContractListDto
        {
            Id = c.Id,
            Folio = c.Folio,
            ClientId = c.ClientId,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            // Tomamos la fecha del último pago como posible expiración, o la de inicio
            ExpirationDate = c.Payments.Any() ? c.Payments.Max(p => p.PaymentDate) : (c.FirstServiceDate ?? DateTime.MinValue)
        }).ToListAsync();
    }

    public async Task<ContractListDto> GetContractByIdAsync(int id)
    {
        var contract = await _context.Contracts.Include(c => c.Payments).FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) throw new KeyNotFoundException("Contrato no encontrado.");

        return new ContractListDto
        {
            Id = contract.Id, Folio = contract.Folio, ClientId = contract.ClientId, Status = contract.Status, CreatedAt = contract.CreatedAt,
            ExpirationDate = contract.Payments.Any() ? contract.Payments.Max(p => p.PaymentDate) : (contract.FirstServiceDate ?? DateTime.MinValue)
        };
    }

    public async Task<ContractFullDetailDto> GetContractFullDetailAsync(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Services)
            .Include(c => c.Payments)
            .Include(c => c.Extras)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (contract == null) throw new KeyNotFoundException("Contrato no encontrado.");

        var quotation = await _context.Quotations.FindAsync(contract.ClientId);

        return new ContractFullDetailDto
        {
            Id = contract.Id,
            Folio = contract.Folio,
            ClientId = contract.ClientId,
            ClientName = quotation?.ClientName ?? $"Cliente #{contract.ClientId}",
            ClientRfc = quotation?.ClientRfc ?? "",
            Representative = quotation?.ContactName ?? "",
            Status = contract.Status,
            CreatedAt = contract.CreatedAt,
            TotalBasePrice = contract.TotalBasePrice,
            ClientObjetoSocial = contract.ClientObjetoSocial,
            ClientDeclaraciones = contract.ClientDeclaraciones,
            ContractDuration = contract.ContractDuration,
            FirstServiceDate = contract.FirstServiceDate,
            Services = contract.Services,
            Payments = contract.Payments,
            Extras = contract.Extras
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
        existing.ClientObjetoSocial = request.ClientObjetoSocial;
        existing.ClientDeclaraciones = request.ClientDeclaraciones;
        existing.ContractDuration = request.ContractDuration;
        existing.FirstServiceDate = request.FirstServiceDate;

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

        // Obtener datos del cliente desde la cotización vinculada
        var quotation = await _context.Quotations.FindAsync(contract.ClientId);

        byte[] pdfBuffer = ContractPdfGenerator.Generate(contract, quotation);
        return (pdfBuffer, "application/pdf", $"{contract.Folio}.pdf");
    }
}

public class ContractListDto { public int Id { get; set; } public string Folio { get; set; } = ""; public int ClientId { get; set; } public string Status { get; set; } = ""; public DateTime CreatedAt { get; set; } public DateTime ExpirationDate { get; set; } }
public class ContractResponseDto { public int Id { get; set; } public string Folio { get; set; } = ""; public string Message { get; set; } = ""; }

public class ContractFullDetailDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public int ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public string ClientRfc { get; set; } = "";
    public string Representative { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public decimal TotalBasePrice { get; set; }
    public string ClientObjetoSocial { get; set; } = "";
    public string ClientDeclaraciones { get; set; } = "";
    public string ContractDuration { get; set; } = "";
    public DateTime? FirstServiceDate { get; set; }
    public List<ContractServiceItem> Services { get; set; } = new();
    public List<ContractPaymentItem> Payments { get; set; } = new();
    public List<ContractExtra> Extras { get; set; } = new();
}