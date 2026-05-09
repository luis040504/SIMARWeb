using System.ComponentModel.DataAnnotations;

namespace ClienteWeb.models
{
    public sealed class RegistroEmpleadoDTO
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string RolSeleccionado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string NombreCompleto { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime? Birthday { get; set; } 

        [Required(ErrorMessage = "La CURP es obligatoria")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "La CURP debe tener 18 caracteres")]
        public string Curp { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RFC es obligatorio")]
        [StringLength(13, MinimumLength = 12, ErrorMessage = "RFC inválido")]
        public string Rfc { get; set; } = string.Empty;

        public string? Phone { get; set; } 

        public string? Genre { get; set; } 

        [Required(ErrorMessage = "El salario es obligatorio")]
        [Range(0.01, 999999, ErrorMessage = "El salario debe ser mayor a 0")]
        public decimal Salario { get; set; }

        public string? LicenseNumber { get; set; } 
        public string? LicenseType { get; set; }
    }
}
