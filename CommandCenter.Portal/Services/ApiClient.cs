using CommanCenter.Portal.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CommanCenter.Portal.Services;

public interface IApiClient
{
    Task<ApiResponse<T>?> GetAsync<T>(string endpoint, string? token = null);
    Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object body, string? token = null);
    Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object body, string? token = null);
    Task<ApiResponse<T>?> PatchAsync<T>(string endpoint, object body, string? token = null);
    Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint, string? token = null);
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiClient> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, ILogger<ApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint, string? token = null)
    {
        SetBearer(token);
        try
        {
            var response = await _http.GetAsync(endpoint);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET {Endpoint} failed", endpoint);
            return ErrorResponse<T>(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object body, string? token = null)
    {
        SetBearer(token);
        try
        {
            var content = JsonContent(body);
            var response = await _http.PostAsync(endpoint, content);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST {Endpoint} failed", endpoint);
            return ErrorResponse<T>(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object body, string? token = null)
    {
        SetBearer(token);
        try
        {
            var content = JsonContent(body);
            var response = await _http.PutAsync(endpoint, content);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT {Endpoint} failed", endpoint);
            return ErrorResponse<T>(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> PatchAsync<T>(string endpoint, object body, string? token = null)
    {
        SetBearer(token);
        try
        {
            var content = JsonContent(body);
            var response = await _http.PatchAsync(endpoint, content);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PATCH {Endpoint} failed", endpoint);
            return ErrorResponse<T>(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint, string? token = null)
    {
        SetBearer(token);
        try
        {
            var response = await _http.DeleteAsync(endpoint);
            return await ParseAsync<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE {Endpoint} failed", endpoint);
            return ErrorResponse<T>(ex.Message);
        }
    }

    private void SetBearer(string? token)
    {
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task<ApiResponse<T>?> ParseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
    }

    private static StringContent JsonContent(object body)
    {
        var json = JsonSerializer.Serialize(body);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static ApiResponse<T> ErrorResponse<T>(string message) => new()
    {
        Exitoso = false,
        Mensaje = message,
        Timestamp = DateTime.UtcNow
    };
}
