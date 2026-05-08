using API_Usuarios.src.Models;
using API_Usuarios.src.DTOs;
using API_Usuarios.src.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace API_Usuarios.src.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UsuarioService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> RegistrarUsuarioCompletoAsync(RegistroRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                string macroRol = dto.RolSeleccionado.Equals("cliente", StringComparison.OrdinalIgnoreCase) 
                                ? RoleTypes.Cliente 
                                : RoleTypes.Empleado;

                var nuevoUsuario = new Usuario
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = macroRol,
                    IsActive = true
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                if (macroRol == RoleTypes.Empleado)
                {
                    var empleadoPayload = new
                    {
                        UserId = nuevoUsuario.IdUser,
                        FullName = dto.NombreCompleto,
                        Address = dto.Direccion,
                        Birthday = dto.Birthday.HasValue 
                                    ? dto.Birthday.Value.ToString("yyyy-MM-dd") 
                                    : null,
                        Curp = dto.Curp.Trim(),
                        Rfc = dto.Rfc.Trim(),
                        Phone = dto.Phone,
                        Genre = dto.Genre,
                        Salary = dto.Salario,
                        RoleName = dto.RolSeleccionado.ToLower().Trim(), 
                        ProfessionalId = dto.ProfessionalId,
                        LicenseNumber = dto.LicenseNumber,
                        LicenseType = dto.LicenseType
                    };

                    var baseUrl = _configuration["Microservices:EmpleadosUrl"];
                    var endpoint = $"{baseUrl}/api/employees";

                    var response = await _httpClient.PostAsJsonAsync(endpoint, empleadoPayload);

                    if (!response.IsSuccessStatusCode)
                    {
                        // --- DIAGNÓSTICO DE ERROR 400 ---
                        var errorDetail = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("*********************************************");
                        Console.WriteLine($"FALLO EN API EMPLEADOS: {response.StatusCode}");
                        Console.WriteLine($"DETALLE DEL ERROR: {errorDetail}");
                        Console.WriteLine("*********************************************");

                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR CRÍTICO EN REGISTRO: {ex.Message}");
                if (ex.InnerException != null) 
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");

                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}