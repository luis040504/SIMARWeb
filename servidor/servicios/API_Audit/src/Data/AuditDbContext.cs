using API_Audit.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Audit.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Payload).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);

            // Índices para consultas frecuentes
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.PerformedBy);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
