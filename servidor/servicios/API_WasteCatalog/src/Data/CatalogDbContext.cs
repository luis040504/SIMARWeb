using API_WasteCatalog.Models;
using Microsoft.EntityFrameworkCore;

namespace API_WasteCatalog.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<WasteType> WasteTypes => Set<WasteType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WasteType>(entity =>
        {
            entity.ToTable("WasteTypes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.PhysicalState).HasMaxLength(20);
            entity.Property(e => e.LgpgirCategory).HasMaxLength(100);
            entity.Property(e => e.ValidUnits).IsRequired().HasMaxLength(50);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Type);
        });
    }
}
