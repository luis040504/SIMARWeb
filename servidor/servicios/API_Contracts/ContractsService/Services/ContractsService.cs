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

        // 1. Búsqueda por Folio o Cliente (usando el ID del cliente o nombre si tuvieras la relación)
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Folio.Contains(search) || c.ClientId.ToString() == search);

            // 2. Filtros secundarios (Solo aplican si hay una búsqueda activa según Criterio de Aceptación)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            if (dateFilter.HasValue)
            {
                // Filtra contratos que estén vigentes en esa fecha
                query = query.Where(c => c.Anexo3Steps.Any(s => dateFilter >= s.StartDate && dateFilter <= s.EndDate));
            }
        }
        else if (!string.IsNullOrEmpty(status))
        {
            // Búsqueda general por estatus si no hay búsqueda por cliente
            query = query.Where(c => c.Status == status);
        }

        return await query.Select(c => new ContractListDto
        {
            Id = c.Id,
            Folio = c.Folio,
            ClientId = c.ClientId,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            // Aquí podrías calcular la fecha de fin basada en los anexos
            ExpirationDate = c.Anexo3Steps.Max(s => (DateTime?)s.EndDate) ?? DateTime.MinValue
        }).ToListAsync();
    }

    public async Task<(byte[] Content, string ContentType, string FileName)> GetContractPdfAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) throw new Exception("Contract not found");

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