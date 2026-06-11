using CommanCenter.Portal.Models;
using System.Security.Claims;

namespace CommanCenter.Portal.Services;

public interface IAuthPortalService
{
    Task<(bool Ok, string? Token, List<string> Roles, string? Error)> LoginAsync(string usuario, string password);
}

public class AuthPortalService : IAuthPortalService
{
    private readonly IApiClient _api;

    public AuthPortalService(IApiClient api) => _api = api;

    public async Task<(bool Ok, string? Token, List<string> Roles, string? Error)> LoginAsync(string usuario, string password)
    {
        var result = await _api.PostAsync<TokenResponse>("api/auth/login", new { usuario, password });

        if (result is null || !result.Exitoso || result.Data is null)
            return (false, null, [], result?.Mensaje ?? "Error al conectar con la API.");

        return (true, result.Data.AccessToken, result.Data.Roles ?? [], null);
    }
}

/// <summary>Matches TokenResponseDto from the API.</summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
