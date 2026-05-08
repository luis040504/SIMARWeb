using Microsoft.EntityFrameworkCore;
using API_Usuarios.src.Models;

namespace API_Usuarios.src.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 
        }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasColumnType("character varying"); 
            });
        }
    }
}