using Microsoft.AspNetCore.Mvc;
using API_Usuarios.src.Services;
using API_Usuarios.src.DTOs;

namespace API_Usuarios.src.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
            // 1. Validación básica de atributos ([Required], [Email], etc.)
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. VALIDACIÓN LÓGICA POR ROL (Lo que platicamos)
            if (dto.RolSeleccionado == "Chofer")
            {
                if (string.IsNullOrWhiteSpace(dto.LicenseNumber) || string.IsNullOrWhiteSpace(dto.LicenseType))
                {
                    return BadRequest(new { mensaje = "Para el rol Chofer, el número y tipo de licencia son obligatorios." });
                }
            }
            else if (dto.RolSeleccionado == "Admin" || dto.RolSeleccionado == "Empleado")
            {
                if (string.IsNullOrWhiteSpace(dto.ProfessionalId))
                {
                    return BadRequest(new { mensaje = $"Para el rol {dto.RolSeleccionado}, la Cédula/ID Profesional es obligatoria." });
                }
            }

            // 3. Si todo está bien, llamamos al servicio
            var exito = await _usuarioService.RegistrarUsuarioCompletoAsync(dto);

            if (exito)
                return Ok(new { mensaje = "Registro exitoso en SIMAR" });
    
            return BadRequest(new { mensaje = "Hubo un error al procesar el registro coordinado (posiblemente el correo o CURP ya existen)" });
        }
    }
}