using API_Usuarios.src.Models;
using API_Usuarios.src.DTOs;
using API_Usuarios.src.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

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
            // Iniciamos una transacción local en la DB de Usuarios
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Mapear DTO a Modelo de Base de Datos (Auth)
                // Nota: Usamos el Enum.Parse sin ToLower() para que coincida con el PascalCase del Enum
                var nuevoUsuario = new Usuario
                {
                    Username = dto.Username,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = Enum.Parse<RoleEnum>(dto.RolSeleccionado), 
                    IsActive = true
                };

                // 2. Guardar en la DB local (Microservicio Usuarios)
                // Aquí es donde Postgres genera el UUID automáticamente
                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                // 3. Preparar los datos para el Microservicio de Empleados
                // Incluimos los campos condicionales que agregamos al DTO
                var empleadoPayload = new
                {
                    UserId = nuevoUsuario.IdUser, // El UUID recién generado
                    FullName = dto.NombreCompleto,
                    dto.Curp,
                    dto.Rfc,
                    Address = dto.Direccion,
                    Salary = dto.Salario,
                    dto.ProfessionalId,
                    dto.LicenseNumber,
                    dto.LicenseType, // <-- El campo que faltaba
                    RoleName = dto.RolSeleccionado 
                };

                // 4. Llamada HTTP al Microservicio de Empleados
                // Obtenemos la URL de appsettings.json
                var baseUrl = _configuration["Microservices:EmpleadosUrl"];
                var endpoint = $"{baseUrl}/api/employees";

                var response = await _httpClient.PostAsJsonAsync(endpoint, empleadoPayload);

                if (response.IsSuccessStatusCode)
                {
                    // Si el Microservicio de Empleados aceptó el registro, confirmamos (Commit)
                    await transaction.CommitAsync();
                    return true;
                }
                else
                {
                    // Si el otro MS falló (ej. error 500 o CURP duplicada allá),
                    // deshacemos el usuario creado aquí para mantener la integridad.
                    await transaction.RollbackAsync();
                    return false;
                }
            }
            catch (Exception)
            {
                // Si hay un error de red o de base de datos, abortamos todo
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}