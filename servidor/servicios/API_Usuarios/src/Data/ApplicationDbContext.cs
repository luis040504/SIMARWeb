using Microsoft.EntityFrameworkCore;
using API_Usuarios.src.Models;
using Npgsql;

namespace API_Usuarios.src.Data // <--- AGREGA ESTA LÍNEA
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<RoleEnum>();
            base.OnModelCreating(modelBuilder);
        }
    }
}