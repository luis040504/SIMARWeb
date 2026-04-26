using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_Empleados.src.Models;

[Table("roles")]
public class Role
{
    [Key]
    [Column("id_role")]
    public Guid IdRole { get; set; }

    [Column("name_role")] // <--- ¡CÁMBIALO AQUÍ! (Antes decía role_name)
    public string RoleName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("permissions")]
    public string Permissions { get; set; } = "[]";
}