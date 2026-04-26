using System.ComponentModel.DataAnnotations;

namespace API_Usuarios.src.DTOs
{
    public class RegistroRequestDto
    {
        // --- Datos para el Microservicio de Usuarios (Auth/DB Local) ---
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string RolSeleccionado { get; set; } = string.Empty; // chofer, administrador, etc.


        // --- Datos para el Microservicio de Empleados (RRHH) ---
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        // NUEVO: Teléfono
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string? Phone { get; set; }

        // NUEVO: Género
        public string? Genre { get; set; }

        // NUEVO: Fecha de Nacimiento
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "El CURP es obligatorio")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "El CURP debe tener 18 caracteres")]
        public string Curp { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RFC es obligatorio")]
        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
        public string Rfc { get; set; } = string.Empty;

        public decimal Salario { get; set; }


        // --- Campos Condicionales de Especialidad ---
        
        // Para Profesionales (Admin, Contador, Vendedor, etc.)
        public string? ProfessionalId { get; set; }

        // Para Choferes
        public string? LicenseNumber { get; set; } 
        public string? LicenseType { get; set; } 
    }
}