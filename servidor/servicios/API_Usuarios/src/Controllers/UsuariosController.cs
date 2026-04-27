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

        [HttpPost("registro-completo")]
        public async Task<IActionResult> Registrar([FromBody] RegistroRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string rolLimpio = dto.RolSeleccionado?.Trim().ToLower() ?? "";

            if (rolLimpio == "chofer")
            {
                if (string.IsNullOrWhiteSpace(dto.LicenseNumber) || string.IsNullOrWhiteSpace(dto.LicenseType))
                {
                    return BadRequest(new { mensaje = "Para el rol Chofer, el número y tipo de licencia son obligatorios." });
                }
            }
            else if (new[] { "administrador", "vendedor", "tecnico", "dueño", "contador" }.Contains(rolLimpio))
            {
                if (string.IsNullOrWhiteSpace(dto.ProfessionalId))
                {
                    return BadRequest(new { mensaje = $"Para el rol {dto.RolSeleccionado}, la Cédula/ID Profesional es obligatoria." });
                }
            }
            else if (rolLimpio != "cliente")
            {
                return BadRequest(new { mensaje = $"El rol '{dto.RolSeleccionado}' no es válido en el sistema." });
            }

            var exito = await _usuarioService.RegistrarUsuarioCompletoAsync(dto);

            if (exito)
                return Ok(new { mensaje = "Registro exitoso en SIMAR" });
    
            return BadRequest(new { mensaje = "Error al procesar el registro (el correo, usuario o CURP podrían estar duplicados)" });
        }
    }
}