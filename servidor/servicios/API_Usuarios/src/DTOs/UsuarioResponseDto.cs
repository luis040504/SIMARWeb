using System;

namespace API_Usuarios.src.DTOs
{
    public class UsuarioResponseDto
    {
        public Guid Id_User { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}