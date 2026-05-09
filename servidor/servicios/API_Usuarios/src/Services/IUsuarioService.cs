using API_Usuarios.src.DTOs;

namespace API_Usuarios.src.Services
{
    public interface IUsuarioService
    {
        Task<bool> RegistrarUsuarioCompletoAsync(RegistroRequestDto registroDto);

        Task<List<UsuarioResponseDto>> ObtenerUsuariosAsync();

        Task<Guid?> ObtenerIdPorUsernameAsync(string username);
    }
}