using Microsoft.AspNetCore.Mvc;
using API_Usuarios.src.Services;
using API_Usuarios.src.DTOs;

namespace API_Usuarios.src.Controllers
{
    [ApiController]
    [Route("api/usuarios")] 
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // === CONSULTAR TODOS LOS USUARIOS ===
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            var usuarios = await _usuarioService.ObtenerUsuariosAsync();
            return Ok(usuarios);
        }

        // === CREAR/REGISTRAR USUARIO/EMPLEADO ===
        [HttpPost("registro-completo")]
        public async Task<IActionResult> Registrar([FromBody] RegistroRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string rolLimpio = dto.RolSeleccionado?.Trim().ToLower() ?? "";

            string[] rolesPermitidos = { "administrador", "vendedor", "tecnico", "dueño", "contador", "chofer", "cliente" };
            
            if (!rolesPermitidos.Contains(rolLimpio))
            {
                return BadRequest(new { mensaje = $"El rol '{dto.RolSeleccionado}' no es válido en el sistema." });
            }

            if (rolLimpio == "chofer")
            {
                if (string.IsNullOrWhiteSpace(dto.LicenseNumber) || string.IsNullOrWhiteSpace(dto.LicenseType))
                {
                    return BadRequest(new { mensaje = "Para el rol Chofer, el número y tipo de licencia son obligatorios." });
                }
            }

            var exito = await _usuarioService.RegistrarUsuarioCompletoAsync(dto);

            if (exito)
                return Ok(new { mensaje = "Registro exitoso en SIMAR" });
    
            return BadRequest(new { mensaje = "Error al procesar el registro (el correo, usuario o CURP podrían estar duplicados)" });
        }


        // Obtener usuario por id
        [HttpGet("buscar-id/{username}")]
        public async Task<IActionResult> ObtenerIdPorUsername(string username)
        {
        var id = await _usuarioService.ObtenerIdPorUsernameAsync(username);

        if (id == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado" });
        }
        return Ok(new { id_user = id });
    }

    }
}