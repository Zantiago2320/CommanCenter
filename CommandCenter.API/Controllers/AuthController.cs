using CommandCenter.API.Application.DTOs.Auth;
using CommandCenter.API.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommandCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Inicia sesión y retorna JWT.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(dto, ip);
        return result.Exitoso ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Refresca el token de acceso.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return result.Exitoso ? Ok(result) : Unauthorized(result);
    }
}
