using System.ComponentModel.DataAnnotations; // Necesario para las validaciones

namespace API_Empleados.src.DTOs
{
    public class EmployeeUpdateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string FullName { get; set; } = string.Empty;

        public string? Address { get; set; }
        
        [Phone(ErrorMessage = "El formato del teléfono es inválido")]
        public string? Phone { get; set; }

        public string? Genre { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El salario no puede ser negativo")]
        public decimal Salary { get; set; }

        // --- Campos de especialidad (Opcionales en la edición) ---
        
        // Para Choferes
        public string? LicenseNumber { get; set; }
        public string? LicenseType { get; set; }

        // Para Personal Administrativo/Técnico
        public string? ProfessionalId { get; set; } 
    }
}