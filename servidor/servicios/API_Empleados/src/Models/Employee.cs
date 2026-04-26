using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Empleados.src.Models;

[Table("employees")]
public class Employee
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("full_name")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Column("address")]
    public string? Address { get; set; }

    [Column("birthday")]
    public DateTime? Birthday { get; set; }

    [Column("curp")]
    [MaxLength(18)]
    public string? Curp { get; set; }

    [Column("rfc")]
    [MaxLength(13)]
    public string? Rfc { get; set; }

    [Column("phone")]
    [MaxLength(15)]
    public string? Phone { get; set; }

    [Column("genre")]
    [MaxLength(20)]
    public string? Genre { get; set; }

    [Column("salary")]
    public decimal Salary { get; set; }

    [Column("state")]
    public int State { get; set; } = 1; // 1: Activo, 0: Inactivo, etc.

    [Column("register_date")]
    public DateTime RegisterDate { get; set; } = DateTime.UtcNow;

    [Column("id_role")]
    public Guid? IdRole { get; set; }

    [Column("manager_id")]
    public Guid? ManagerId { get; set; }

    // Propiedades de Navegación (Opcionales, para facilitar consultas)
    [ForeignKey("IdRole")]
    public virtual Role? Role { get; set; }

    [ForeignKey("ManagerId")]
    public virtual Employee? Manager { get; set; }
}