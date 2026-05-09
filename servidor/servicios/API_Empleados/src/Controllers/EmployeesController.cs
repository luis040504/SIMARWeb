using Microsoft.AspNetCore.Mvc;
using API_Empleados.src.Data;
using API_Empleados.src.Models;
using API_Empleados.src.DTOs;
using API_Empleados.src.Services;
using Microsoft.EntityFrameworkCore;

namespace API_Empleados.src.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EmployeeService _employeeService; 

    public EmployeesController(ApplicationDbContext context, EmployeeService employeeService)
    {
        _context = context;
        _employeeService = employeeService;
    }

    // === CREAR EMPLEADO ===

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto dto)
    {
        try
        {
            var empleadoCreado = await _employeeService.CreateEmployeeAsync(dto);

            return Ok(new { 
                mensaje = $"Registro exitoso: {empleadoCreado.FullName} guardado como {dto.RoleName}",
                professionalId = empleadoCreado.ProfessionalId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                mensaje = "Error al procesar el registro en el módulo de Empleados",
                error = ex.Message 
            });
        }
    }

    // === CONSULTAR EMPLEADO ===

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.UserId == id);

        if (employee == null) return NotFound(new { mensaje = "Empleado no encontrado" });

        var driver = await _context.DriverDetails.FindAsync(id);

        return Ok(new {
            BaseInfo = employee,
            DriverInfo = driver
        });
    }

    // === ACTUALIZAR EMPLEADO ===

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

        await _context.SaveChangesAsync();
        return Ok(new { mensaje = "Datos actualizados correctamente" });
    }

    // === CONSULTAR EMPLEADOS ===

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

    // === DAR BAJA EMPLEADO ===

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