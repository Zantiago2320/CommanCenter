using CommanCenter.Portal.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CommanCenter.Portal.Services;

public interface INotificationApiService
{
    Task<ApiResponse<bool>?> SendEmailAsync(SendEmailRequest request, string? token = null);
    Task<ApiResponse<IEnumerable<NotificationViewModel>>?> GetNotificationsAsync(string userId, string? token = null);
}

public class NotificationApiService : INotificationApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<NotificationApiService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationApiService(HttpClient http, ILogger<NotificationApiService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>?> SendEmailAsync(SendEmailRequest request, string? token = null)
    {
        SetBearer(token);
        try
        {
            var content = JsonContent(request);
            var response = await _http.PostAsync("api/notifications/email", content);
            return await ParseAsync<bool>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación de email a {To}", request.To);
            return ErrorResponse<bool>(ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<NotificationViewModel>>?> GetNotificationsAsync(string userId, string? token = null)
    {
        SetBearer(token);
        try
        {
            var response = await _http.GetAsync($"api/notifications/{userId}");
            return await ParseAsync<IEnumerable<NotificationViewModel>>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones del usuario {UserId}", userId);
            return ErrorResponse<IEnumerable<NotificationViewModel>>(ex.Message);
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

public class SendEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}

public class NotificationViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
}
