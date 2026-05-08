using Microsoft.EntityFrameworkCore;
using API_Empleados.src.Models;

namespace API_Empleados.src.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<DriverDetail> DriverDetails { get; set; }
    public DbSet<ProfessionalStaff> ProfessionalStaff { get; set; }
    
    // Nueva tabla registrada
    public DbSet<Role> Roles { get; set; } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuraciones adicionales si fueran necesarias
        modelBuilder.Entity<Role>().HasKey(r => r.IdRole);
    }
}