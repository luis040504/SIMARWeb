using System.ComponentModel.DataAnnotations;

namespace API_Usuarios.src.DTOs
{
    public class RegistroRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string RolSeleccionado { get; set; } = string.Empty; 

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        public string? Direccion { get; set; }
        
        public DateTime? Birthday { get; set; }

        [Required]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "La CURP debe tener exactamente 18 caracteres")]
        public string Curp { get; set; } = string.Empty;

        [Required]
        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
        public string Rfc { get; set; } = string.Empty;

        public string? Phone { get; set; }
        
        public string? Genre { get; set; }

        public decimal Salario { get; set; }

        // Campos opcionales según especialidad
        public string? ProfessionalId { get; set; }
        public string? LicenseNumber { get; set; }
        public string? LicenseType { get; set; }
    }
}