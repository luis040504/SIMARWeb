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
        // 1. DETERMINAR EL ENUM (Para la base de datos de Usuarios)
        // Convertimos el string que viene del DTO al Enum que espera el modelo
        RoleEnum macroRol;
        
        if (dto.RolSeleccionado.Equals("cliente", StringComparison.OrdinalIgnoreCase))
        {
            macroRol = RoleEnum.cliente;
        }
        else
        {
            // Cualquier otro (admin, vendedor, etc.) entra como 'empleado' en users_db
            macroRol = RoleEnum.empleado;
        }

        var nuevoUsuario = new Usuario
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = macroRol, // <--- AHORA SÍ COINCIDEN LOS TIPOS (Enum = Enum)
            IsActive = true
        };

        _context.Usuarios.Add(nuevoUsuario);
        await _context.SaveChangesAsync();

        // 2. LÓGICA PARA EL MICROSERVICIO DE EMPLEADOS
        // Seguimos usando RoleEnum.empleado para decidir si mandamos el payload
        if (macroRol == RoleEnum.empleado)
        {
            var empleadoPayload = new
            {
                UserId = nuevoUsuario.IdUser,
                FullName = dto.NombreCompleto,
                Address = dto.Direccion,
                Birthday = dto.Birthday.HasValue 
                            ? dto.Birthday.Value.ToString("yyyy-MM-dd") 
                            : null,
                Curp = dto.Curp?.Trim(),
                Rfc = dto.Rfc?.Trim(),
                Phone = dto.Phone,
                Genre = dto.Genre,
                Salary = dto.Salario,
                // Aquí mandamos el rol específico (vendedor, tecnico, etc.) como string
                RoleName = dto.RolSeleccionado.ToLower().Trim(), 
                LicenseNumber = dto.LicenseNumber,
                LicenseType = dto.LicenseType
            };

            var baseUrl = _configuration["Microservices:EmpleadosUrl"];
            var endpoint = $"{baseUrl}/api/employees";

            var response = await _httpClient.PostAsJsonAsync(endpoint, empleadoPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                // ... (tus logs de error)
                await transaction.RollbackAsync();
                return false;
            }
        }

        await transaction.CommitAsync();
        return true;
    }
    catch (Exception ex)
    {
        // ... (tus logs de error)
        await transaction.RollbackAsync();
        return false;
    }
}

        public async Task<List<UsuarioResponseDto>> ObtenerUsuariosAsync()
        {
            return await _context.Usuarios
            .Select(u => new UsuarioResponseDto
            {
                Id_User = u.IdUser, 
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();
        }

        public async Task<Guid?> ObtenerIdPorUsernameAsync(string username)
        {
            var usuarioId = await _context.Usuarios
            .Where(u => u.Username.ToLower() == username.ToLower())
            .Select(u => u.IdUser) 
            .FirstOrDefaultAsync();
            return usuarioId == Guid.Empty ? null : usuarioId;
        }
    }
}