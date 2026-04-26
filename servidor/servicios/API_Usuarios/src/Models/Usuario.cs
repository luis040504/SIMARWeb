using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Usuarios.src.Models
{
    [Table("users")] // Mapea al nombre exacto de la tabla en Postgres
    public class Usuario
    {
        [Key]
        [Column("id_user")]
        public Guid IdUser { get; set; } = Guid.NewGuid();

        [Required]
        [Column("username")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        public RoleEnum Role { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // Definimos el Enum para que coincida con tu role_enum de SQL
    public enum RoleEnum
    {
        empleado,
        cliente
    }
}