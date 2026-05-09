using API_Empleados.src.Data;
using API_Empleados.src.Models;
using API_Empleados.src.DTOs;
using Microsoft.EntityFrameworkCore;

namespace API_Empleados.src.Services;

public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee> CreateEmployeeAsync(EmployeeCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            string nombreRolLimpio = dto.RoleName.ToLower().Trim();
            
            var roleDb = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName.ToLower() == nombreRolLimpio);

            if (roleDb == null)
            {
                throw new Exception($"El rol '{dto.RoleName}' no es reconocido en la base de datos.");
            }

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
                IdRole = roleDb.IdRole,
                State = 1,
                RegisterDate = DateTime.UtcNow,
                
                ProfessionalId = await GenerarSiguienteProfessionalId(roleDb.RoleName)
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            // Si es chofer, guardamos sus detalles extra
            if (nombreRolLimpio == "chofer")
            {
                _context.DriverDetails.Add(new DriverDetail {
                    EmployeeId = emp.UserId,
                    LicenseNumber = dto.LicenseNumber ?? "N/A",
                    LicenseType = dto.LicenseType ?? "N/A"
                });
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return emp;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<string> GenerarSiguienteProfessionalId(string nombreRol)
    {
        nombreRol = nombreRol.ToLower().Trim();
        string prefijo = nombreRol == "chofer" ? "CH" : nombreRol.Substring(0, 1).ToUpper();

        var ultimoEmpleado = await _context.Employees
            .Where(e => e.ProfessionalId.StartsWith(prefijo))
            .OrderByDescending(e => e.ProfessionalId)
            .FirstOrDefaultAsync();

        int numeroSecuencia = 1;

        if (ultimoEmpleado != null)
        {
            string parteNumerica = ultimoEmpleado.ProfessionalId.Substring(prefijo.Length);
            if (int.TryParse(parteNumerica, out int ultimoNumero))
            {
                numeroSecuencia = ultimoNumero + 1;
            }
        }

        return $"{prefijo}{numeroSecuencia.ToString("D3")}";
    }
}