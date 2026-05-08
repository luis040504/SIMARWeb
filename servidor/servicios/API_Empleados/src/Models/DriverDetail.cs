using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Empleados.src.Models;

[Table("driver_details")]
public class DriverDetail
{
    [Key]
    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("license_number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Column("license_type")]
    public string LicenseType { get; set; } = string.Empty;
}