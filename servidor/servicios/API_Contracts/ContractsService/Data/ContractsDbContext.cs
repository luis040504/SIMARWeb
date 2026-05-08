using Microsoft.EntityFrameworkCore;
using ContractsService.Models;

namespace ContractsService.Data;

public class ContractsDbContext : DbContext
{
    public ContractsDbContext(DbContextOptions<ContractsDbContext> options)
        : base(options) { }

    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ContractServiceItem> ContractServices => Set<ContractServiceItem>();
    public DbSet<ContractPaymentItem> ContractPayments => Set<ContractPaymentItem>();
    public DbSet<ContractExtra> ContractExtras => Set<ContractExtra>();
}