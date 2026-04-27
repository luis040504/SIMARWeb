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
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            string nombreRolLimpio = dto.RoleName.ToLower().Trim();
            var roleDb = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName.ToLower() == nombreRolLimpio);

            var emp = new Employee {
                UserId = dto.UserId,
                FullName = dto.FullName,
                Address = dto.Address,
                Birthday = dto.Birthday,
                Phone = dto.Phone,
                Genre = dto.Genre,
                Curp = dto.Curp,
                Rfc = dto.Rfc,
                Salary = dto.Salary,
                IdRole = roleDb?.IdRole, 
                State = 1, 
                RegisterDate = DateTime.UtcNow
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

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
            .Include(e => e.Role)
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeUpdateDto dto)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return NotFound(new { mensaje = "Empleado no encontrado" });

        emp.FullName = dto.FullName;
        emp.Address = dto.Address;
        emp.Phone = dto.Phone;
        emp.Genre = dto.Genre;
        emp.Salary = dto.Salary;

        var driver = await _context.DriverDetails.FindAsync(id);
        if (driver != null) driver.LicenseNumber = dto.LicenseNumber ?? driver.LicenseNumber;

        var prof = await _context.ProfessionalStaff.FindAsync(id);
        if (prof != null) prof.ProfessionalId = dto.ProfessionalId ?? prof.ProfessionalId;

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Datos actualizados correctamente" });
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] string? name, [FromQuery] string? role)
    {
        var query = _context.Employees
            .Include(e => e.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => e.FullName.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(e => e.Role != null && e.Role.RoleName.ToLower() == role.ToLower());
        }

        var employees = await query.ToListAsync();
        return Ok(employees);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] int newState)
    {
        var emp = await _context.Employees.FindAsync(id);
        if (emp == null) return NotFound();

        emp.State = newState; 
        await _context.SaveChangesAsync();
    
        return Ok(new { mensaje = $"Estado cambiado a {newState}" });
    }
}