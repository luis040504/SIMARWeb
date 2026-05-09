using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace API_Empleados.src.Models;

[Table("employees")]
[Index(nameof(ProfessionalId), IsUnique = true)]
public class Employee
{
    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; }

    [Required]
    [Column("professional_id")]
    [StringLength(10)]
    public string ProfessionalId { get; set; } = string.Empty;

    [Column("full_name")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Column("address")]
    public string? Address { get; set; }

    [Column("birthday")]
    public DateTime? Birthday { get; set; }

    [Column("curp")]
    [StringLength(18)]
    public string? Curp { get; set; }

    [Column("rfc")]
    [StringLength(13)]
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
    public int State { get; set; } = 1;

    [Column("register_date")]
    public DateTime RegisterDate { get; set; } = DateTime.UtcNow;

    [Column("id_role")]
    public Guid? IdRole { get; set; }

    [Column("manager_id")]
    public Guid? ManagerId { get; set; }

    [ForeignKey("IdRole")]
    public virtual Role? Role { get; set; }

    [ForeignKey("ManagerId")]
    public virtual Employee? Manager { get; set; }
}