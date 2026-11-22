using System.Security.Claims;
using DTOs.Auth;
using Backend.Services;
using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Usuario> _userManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<Usuario> userManager,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        _logger.LogInformation("Intentando registrar nuevo usuario con correo {Email}", dto.Email);

        var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
        if (usuarioExistente != null)
        {
            _logger.LogWarning("Registro rechazado: el correo {Email} ya está registrado", dto.Email);
            return BadRequest(new { mensaje = "El correo electrónico ya está registrado." });
        }

        var nuevoUsuario = new Usuario
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nombre = dto.Nombre,
            Apellido = dto.Apellido
        };

        var resultado = await _userManager.CreateAsync(nuevoUsuario, dto.Password);

        if (!resultado.Succeeded)
        {
            var errores = resultado.Errors.Select(e => e.Description);
            _logger.LogWarning("Falló el registro para {Email}. Errores: {@Errores}", dto.Email, errores);
            return BadRequest(new { mensaje = "No se pudo registrar el usuario.", errores });
        }

        _logger.LogInformation("Usuario registrado exitosamente con ID {UsuarioId} y correo {Email}", nuevoUsuario.Id, dto.Email);
        return Ok(new { mensaje = "Usuario registrado exitosamente." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Intento de inicio de sesión para el correo {Email}", dto.Email);

        var usuario = await _userManager.FindByEmailAsync(dto.Email);

        if (usuario == null)
        {
            _logger.LogWarning("Login fallido: usuario no encontrado para {Email}", dto.Email);
            return Unauthorized(new { mensaje = "Credenciales inválidas." });
        }

        if (await _userManager.IsLockedOutAsync(usuario))
        {
            _logger.LogWarning("Login rechazado: cuenta bloqueada temporalmente para usuario {UsuarioId} ({Email})", usuario.Id, dto.Email);
            return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por múltiples intentos fallidos." });
        }

        if (!await _userManager.CheckPasswordAsync(usuario, dto.Password))
        {
            await _userManager.AccessFailedAsync(usuario);
            _logger.LogWarning("Login fallido: contraseña incorrecta para usuario {UsuarioId} ({Email})", usuario.Id, dto.Email);
            return Unauthorized(new { mensaje = "Credenciales inválidas." });
        }

        await _userManager.ResetAccessFailedCountAsync(usuario);

        var token = _jwtService.GenerarToken(usuario);
        _logger.LogInformation("Inicio de sesión exitoso para usuario {UsuarioId} ({Email})", usuario.Id, dto.Email);

        return Ok(new AuthResponseDto(
            Token: token,
            UsuarioId: usuario.Id,
            Email: usuario.Email!,
            Nombre: usuario.Nombre,
            Apellido: usuario.Apellido
        ));
    }

    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            _logger.LogWarning("Acceso a perfil sin claim NameIdentifier válido en el token");
            return Unauthorized();
        }

        _logger.LogInformation("Consultando perfil del usuario {UsuarioId}", userId);

        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario == null)
        {
            _logger.LogWarning("No se encontró en base de datos el usuario autenticado con ID {UsuarioId}", userId);
            return NotFound();
        }

        return Ok(new
        {
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email
        });
    }
}