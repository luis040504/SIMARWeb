using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Empleados.src.Models;

[Table("roles")]
public class Role
{
    [Key]
    [Column("id_role")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] 
    public Guid IdRole { get; set; }

    [Required]
    [Column("name_role")] 
    public string RoleName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("permissions")]
    public string Permissions { get; set; } = "[]";
}