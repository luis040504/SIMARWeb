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
        RoleEnum macroRol;
        
        if (dto.RolSeleccionado.Equals("cliente", StringComparison.OrdinalIgnoreCase))
        {
            macroRol = RoleEnum.cliente;
        }
        else
        {
            macroRol = RoleEnum.empleado;
        }

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
                await transaction.RollbackAsync();
                return false;
            }
        }

        await transaction.CommitAsync();
        return true;
    }
    catch (Exception ex)
    {
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

        // 1. Registro Simple: Solo datos de la DB de usuarios y devuelve el ID
        public async Task<Guid> RegistrarUsuarioSimpleAsync(UsuarioRegistroSimpleDto dto)
        {
            // Creamos la entidad Usuario con los datos mínimos
            var nuevoUsuario = new Usuario
            {
                IdUser = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                // Hasheamos la contraseña antes de guardar
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                // Convertimos el string del DTO al Enum de la base de datos
                Role = Enum.TryParse<RoleEnum>(dto.Role.ToLower(), out var roleResult) 
                ? roleResult 
                : RoleEnum.cliente, // Por defecto si falla el parse
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return nuevoUsuario.IdUser; // Retornamos el GUID recién generado
        }

        // 2. Modificar información del usuario
        public async Task<bool> ActualizarUsuarioAsync(Guid id, UsuarioUpdateDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
    
            if (usuario == null) return false; // Si no existe, avisamos al controlador

            usuario.Username = dto.Username;
            usuario.Email = dto.Email;
    
            // Actualizamos el rol si viene en el DTO
            if (!string.IsNullOrEmpty(dto.Role))
            {
                usuario.Role = Enum.TryParse<RoleEnum>(dto.Role.ToLower(), out var roleResult) 
                ? roleResult 
                : usuario.Role;
            }

            usuario.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // 3. Consultar un usuario por su ID
        public async Task<UsuarioSimpleResponseDto?> ObtenerPorIdAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
    
            if (usuario == null) return null;

            return new UsuarioSimpleResponseDto
            {
                IdUser = usuario.IdUser,
                Username = usuario.Username,
                Email = usuario.Email,
                Role = usuario.Role.ToString(),
                IsActive = usuario.IsActive,
                CreatedAt = usuario.CreatedAt
            };
        }
    }
}