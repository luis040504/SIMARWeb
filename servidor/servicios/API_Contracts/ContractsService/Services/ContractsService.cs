using ContractsService.Data;
using ContractsService.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractsService.Services;

public interface IContractService
{
    Task<ContractResponseDto> CreateContractAsync(Contract request);
    Task<List<ContractListDto>> GetContractsAsync(string? search, string? status, DateTime? dateFilter);
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
        if (request.Anexo3Steps.Any(step => step.EndDate < step.StartDate))
        {
            throw new ArgumentException("Rango de fechas inválido: La fecha de fin no puede ser anterior a la de inicio.");
        }

        request.Folio = $"CON-{DateTime.Now:yyyyMM}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
        request.CreatedAt = DateTime.UtcNow;
        request.Status = "Pendiente de firma";

        _context.Contracts.Add(request);
        await _context.SaveChangesAsync();

        var audit = new AuditLog
        {
            Action = "Create Contract",
            Details = $"Contrato {request.Folio} (ID: {request.Id}) creado para el Cliente ID: {request.ClientId}"
        };
        _context.AuditLogs.Add(audit);
        await _context.SaveChangesAsync();

        return new ContractResponseDto 
        { 
            Id = request.Id, 
            Folio = request.Folio,
            Message = "Contrato creado exitosamente."
        };
    }

    public async Task<List<ContractListDto>> GetContractsAsync(string? search, string? status, DateTime? dateFilter)
    {
        var query = _context.Contracts.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Folio.Contains(search) || c.ClientId.ToString() == search);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (dateFilter.HasValue)
            {
                query = query.Where(c => c.Anexo3Steps.Any(s => dateFilter >= s.StartDate && dateFilter <= s.EndDate));
            }
        }
        else if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(c => c.Status == status);
        }

        return await query.Select(c => new ContractListDto
        {
            Id = c.Id,
            Folio = c.Folio,
            ClientId = c.ClientId,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            ExpirationDate = c.Anexo3Steps.Max(s => (DateTime?)s.EndDate) ?? DateTime.MinValue
        }).ToListAsync();
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> GetContractPdfAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("ID inválido");

        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) throw new KeyNotFoundException("Contract not found");

        byte[] pdfBuffer = System.Text.Encoding.UTF8.GetBytes($"Contrato {contract.Folio}");
        
        return (pdfBuffer, "application/pdf", $"{contract.Folio}.pdf");
    }
}

public class ContractListDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public int ClientId { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpirationDate { get; set; }
}

public class ContractResponseDto
{
    public int Id { get; set; }
    public string Folio { get; set; } = "";
    public string Message { get; set; } = "";
}