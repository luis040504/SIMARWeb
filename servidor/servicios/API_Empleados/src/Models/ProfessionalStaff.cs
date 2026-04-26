using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Empleados.src.Models;

[Table("professional_staff")]
public class ProfessionalStaff
{
    [Key]
    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("professional_id")]
    public string ProfessionalId { get; set; } = string.Empty;
}