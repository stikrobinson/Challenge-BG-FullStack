using System.ComponentModel.DataAnnotations;

namespace DTOs.Auth
{
    public record RegistroDto(
        [Required, MaxLength(100)] string Nombre,
        [Required, MaxLength(100)] string Apellido,
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password
    );

    public record LoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

    public record AuthResponseDto(
        string Token,
        int UsuarioId,
        string Email,
        string Nombre,
        string Apellido
    );
}

