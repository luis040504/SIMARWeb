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
        public string Curp { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RFC es obligatorio")]
        [StringLength(13, MinimumLength = 12, ErrorMessage = "El RFC debe tener entre 12 y 13 caracteres")]
        public string Rfc { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser una cantidad positiva")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Es necesario especificar un nombre de rol")]
        public string RoleName { get; set; } = string.Empty;
    
        public string? LicenseNumber { get; set; }
        public string? LicenseType { get; set; }
        public string? ProfessionalId { get; set; }
    }
}