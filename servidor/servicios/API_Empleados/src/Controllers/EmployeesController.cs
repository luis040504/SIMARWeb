using Microsoft.AspNetCore.Mvc;
using API_Empleados.src.Data;
using API_Empleados.src.Models;
using API_Empleados.src.DTOs;
using Microsoft.EntityFrameworkCore;

namespace API_Empleados.src.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
    {
        // Iniciamos transacción para asegurar integridad entre tablas
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. BUSCAR EL ID DEL ROL (Para que no sea null en la BD)
            // Convertimos a minúsculas para que coincida con tu script de inserción
            string nombreRolLimpio = dto.RoleName.ToLower().Trim();
            var roleDb = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName.ToLower() == nombreRolLimpio);

            // 2. Mapeo del empleado con los NUEVOS campos (Evita los NULL en pgAdmin)
            var emp = new Employee {
                UserId = dto.UserId,
                FullName = dto.FullName,
                Address = dto.Address,
                Birthday = dto.Birthday, // <-- Agregado
                Phone = dto.Phone,       // <-- Agregado
                Genre = dto.Genre,       // <-- Agregado
                Curp = dto.Curp,
                Rfc = dto.Rfc,
                Salary = dto.Salary,
                IdRole = roleDb?.IdRole, // <-- ID real de la tabla 'roles'
                State = 1, 
                RegisterDate = DateTime.UtcNow
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // 3. Lógica de especialidad según el rol
            if (nombreRolLimpio == "chofer")
            {
                _context.DriverDetails.Add(new DriverDetail {
                    EmployeeId = dto.UserId,
                    LicenseNumber = dto.LicenseNumber ?? "N/A",
                    LicenseType = dto.LicenseType ?? "N/A"
                });
            }
            else if (new[] { "administrador", "vendedor", "tecnico", "dueño", "contador" }.Contains(nombreRolLimpio))
            {
                _context.ProfessionalStaff.Add(new ProfessionalStaff {
                    EmployeeId = dto.UserId,
                    ProfessionalId = dto.ProfessionalId ?? "N/A"
                });
            }
            else
            {
                throw new Exception($"El rol '{dto.RoleName}' no es reconocido o no tiene una tabla de destino.");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { mensaje = $"Registro exitoso: {dto.FullName} guardado como {dto.RoleName}" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new { 
                mensaje = "Error al procesar el registro en el módulo de Empleados",
                error = ex.Message 
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.Role) // Incluye el nombre del rol en la consulta
            .FirstOrDefaultAsync(e => e.UserId == id);

        if (employee == null) return NotFound(new { mensaje = "Empleado no encontrado" });

        var driver = await _context.DriverDetails.FindAsync(id);
        var professional = await _context.ProfessionalStaff.FindAsync(id);

        return Ok(new {
            BaseInfo = employee,
            DriverInfo = driver,
            ProfessionalInfo = professional
        });
    }

    // === ACTUALIZACION DE INFORMACION ===
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeUpdateDto dto)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return NotFound(new { mensaje = "Empleado no encontrado" });

        // Actualizamos solo lo permitido
        emp.FullName = dto.FullName;
        emp.Address = dto.Address;
        emp.Phone = dto.Phone;
        emp.Genre = dto.Genre;
        emp.Salary = dto.Salary;

        // Actualizamos detalles según el rol
        var driver = await _context.DriverDetails.FindAsync(id);
        if (driver != null) driver.LicenseNumber = dto.LicenseNumber ?? driver.LicenseNumber;

        var prof = await _context.ProfessionalStaff.FindAsync(id);
        if (prof != null) prof.ProfessionalId = dto.ProfessionalId ?? prof.ProfessionalId;

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Datos actualizados correctamente" });
    }

    // === BUSCAR POR USUARIO O ROL ===
    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] string? name, [FromQuery] string? role)
    {
        // 1. Iniciamos la consulta incluyendo el Rol para poder filtrar por su nombre
        var query = _context.Employees
            .Include(e => e.Role)
            .AsQueryable();

        // 2. Filtro por Nombre (si el usuario lo envió)
        // Usamos ToLower() para que no importe si escriben en mayúsculas o minúsculas
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => e.FullName.ToLower().Contains(name.ToLower()));
        }

        // 3. Filtro por Rol (si el usuario lo envió)
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(e => e.Role.RoleName.ToLower() == role.ToLower());
        }

        // 4. Ejecutamos la consulta y traemos los resultados
        ar employees = await query.ToListAsync();

        return Ok(employees);
    }


    // === DAR DE BAJA ===
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] int newState)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return NotFound();

        emp.State = newState; // 0 para inactivo, 1 para activo
        await _context.SaveChangesAsync();
    
        return Ok(new { mensaje = $"Estado cambiado a {newState}" });
    }
}