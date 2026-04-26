using System;
using System.ComponentModel.DataAnnotations;

namespace API_Empleados.src.DTOs
{
    public class EmployeeCreateDto
    {
        [Required(ErrorMessage = "El UserId es obligatorio para vincular con la cuenta")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre es demasiado largo")]
        public string FullName { get; set; } = string.Empty;

        public string? Address { get; set; }
    
        public DateTime? Birthday { get; set; }
        
        [Phone(ErrorMessage = "El formato de teléfono no es válido")]
        public string? Phone { get; set; }
        
        public string? Genre { get; set; }
    
        [Required(ErrorMessage = "La CURP es obligatoria para el registro legal")]
        [StringLength(18, MinimumLength = 18, ErrorMessage = "La CURP debe tener exactamente 18 caracteres")]
        public string? Curp { get; set; }

        [Required(ErrorMessage = "El RFC es obligatorio")]
        public string? Rfc { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser una cantidad positiva")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Es necesario especificar un rol (ej. chofer, administrador)")]
        public string RoleName { get; set; } = string.Empty;
    
        // --- Detalles de especialidad (según el rol) ---
        public string? LicenseNumber { get; set; }
        public string? LicenseType { get; set; }
        public string? ProfessionalId { get; set; }
    }
}