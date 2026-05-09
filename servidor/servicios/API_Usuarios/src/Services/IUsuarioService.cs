using API_Usuarios.src.DTOs;
using API_Usuarios.src.Models; // Necesario para retornar el objeto Usuario

namespace API_Usuarios.src.Services
{
    public interface IUsuarioService
    {
        Task<bool> RegistrarUsuarioCompletoAsync(RegistroRequestDto registroDto);
        Task<List<UsuarioResponseDto>> ObtenerUsuariosAsync();
        Task<Guid?> ObtenerIdPorUsernameAsync(string username);


        // Registrar usuario
        Task<Guid> RegistrarUsuarioSimpleAsync(UsuarioRegistroSimpleDto dto);

        // Modificar usuario
        Task<bool> ActualizarUsuarioAsync(Guid id, UsuarioUpdateDto dto);

        // Consultar por su id
        Task<UsuarioSimpleResponseDto?> ObtenerPorIdAsync(Guid id);
    }
}