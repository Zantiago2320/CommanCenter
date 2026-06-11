using CommandCenter.API.Application.DTOs.Auth;
using CommandCenter.API.Application.DTOs.Common;

namespace CommandCenter.API.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto, string? ip);
    Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<bool>> LogoutAsync(string usuarioId);
}
