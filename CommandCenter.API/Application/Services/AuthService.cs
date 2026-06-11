using CommandCenter.API.Application.DTOs.Auth;
using CommandCenter.API.Application.DTOs.Common;
using CommandCenter.API.Application.Interfaces;
using CommandCenter.API.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CommandCenter.API.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;
    private readonly IAuditoriaRepository _auditoria;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        IConfiguration config,
        IAuditoriaRepository auditoria,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _config = config;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto, string? ip)
    {
        var user = await _userManager.FindByNameAsync(dto.Usuario);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _auditoria.RegistrarAsync("Auth", "LOGIN_FAIL", "Usuario",
                null, null, dto.Usuario, null, dto.Usuario, ip, exitoso: false, error: "Credenciales inválidas");
            return ApiResponse<TokenResponseDto>.Fail("Credenciales inválidas.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerarJwt(user, roles);
        var refreshToken = GenerarRefreshToken();

        await _auditoria.RegistrarAsync("Auth", "LOGIN", "Usuario",
            user.Id, null, user.UserName, user.Id, user.UserName, ip);

        _logger.LogInformation("Login exitoso: {Usuario} desde IP {Ip}", user.UserName, ip);

        return ApiResponse<TokenResponseDto>.Ok(new TokenResponseDto
        {
            AccessToken = token.Token,
            RefreshToken = refreshToken,
            Expiracion = token.Expiracion,
            Email = user.Email ?? string.Empty,
            NombreCompleto = user.UserName ?? string.Empty,
            Roles = roles.ToList()
        });
    }

    public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        // Implementación básica — en producción validar contra DB
        return ApiResponse<TokenResponseDto>.Fail("RefreshToken no implementado aún.");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string usuarioId)
    {
        await _auditoria.RegistrarAsync("Auth", "LOGOUT", "Usuario",
            usuarioId, null, null, usuarioId, null, null);
        return ApiResponse<bool>.Ok(true);
    }

    private (string Token, DateTime Expiracion) GenerarJwt(IdentityUser user, IList<string> roles)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var expiracion = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"] ?? "480"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }

    private static string GenerarRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
