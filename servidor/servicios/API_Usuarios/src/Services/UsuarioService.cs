using API_Usuarios.src.Models;
using API_Usuarios.src.DTOs;
using API_Usuarios.src.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using System.Net.Http.Json; // Asegúrate de tener este using para PostAsJsonAsync

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
            // Iniciamos una transacción local para asegurar integridad en la DB de Usuarios
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. DETERMINAR MACRO-ROL PARA LA TABLA 'users'
                // Mapeamos los roles específicos a los dos únicos valores del ENUM en Postgres
                var macroRol = dto.RolSeleccionado.Equals("cliente", StringComparison.OrdinalIgnoreCase) 
                               ? RoleEnum.cliente 
                               : RoleEnum.empleado;

                // 2. Mapear al modelo de la base de datos de Usuarios (Auth)
                var nuevoUsuario = new Usuario
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = macroRol, 
                    IsActive = true
                };

                // 3. Guardar en DB local (Microservicio Usuarios)
                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // 4. SI EL ROL ES EMPLEADO, NOTIFICAR AL MICROSERVICIO DE EMPLEADOS
                if (macroRol == RoleEnum.empleado)
                {
                    // Agregamos los campos que antes se iban como NULL
                    var empleadoPayload = new
                    {
                        UserId = nuevoUsuario.IdUser,
                        FullName = dto.NombreCompleto,
                        Address = dto.Direccion,
                        Birthday = dto.Birthday, // <-- CORREGIDO: Ahora se envía la fecha
                        Curp = dto.Curp,
                        Rfc = dto.Rfc,
                        Phone = dto.Phone,       // <-- CORREGIDO: Ahora se envía el teléfono
                        Genre = dto.Genre,       // <-- CORREGIDO: Ahora se envía el género
                        Salary = dto.Salario,
                        RoleName = dto.RolSeleccionado,
                        dto.ProfessionalId,
                        dto.LicenseNumber,
                        dto.LicenseType
                    };

                    var baseUrl = _configuration["Microservices:EmpleadosUrl"];
                    var endpoint = $"{baseUrl}/api/employees";

                    // Llamada síncrona entre microservicios
                    var response = await _httpClient.PostAsJsonAsync(endpoint, empleadoPayload);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Si el microservicio de Empleados falla (ej. CURP duplicada), 
                        // hacemos Rollback para que no se cree el usuario aquí.
                        await transaction.RollbackAsync();
                        return false;
                    }
                }

                // Si todo el flujo fue exitoso, confirmamos los cambios
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Loguear el error sería ideal aquí: Console.WriteLine(ex.Message);
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}