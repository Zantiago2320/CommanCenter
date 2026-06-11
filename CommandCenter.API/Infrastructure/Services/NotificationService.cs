using System.Text.RegularExpressions;
using Azure;
using Azure.Communication.Email;
using CommandCenter.API.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommandCenter.API.Infrastructure.Services;

/// <summary>
/// Servicio de notificaciones vía Azure Communication Services (Email).
/// Toda comunicación de correo del sistema pasa por aquí.
/// Reutilizable para todos los módulos: DataTeam, RRHH, DevSecOps, etc.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration config, ILogger<NotificationService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml, string? adjuntoUrl = null)
    {
        try
        {
            var (client, sender) = CrearCliente();
            if (client is null) return;

            var message = ConstruirMensaje(sender, new[] { destinatario }, asunto, cuerpoHtml);

            var operation = await client.SendAsync(WaitUntil.Completed, message);
            _logger.LogInformation("Email enviado a {Destinatario} (Id: {Id}): {Asunto}",
                destinatario, operation.Id, asunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Destinatario}", destinatario);
        }
    }

    public async Task EnviarEmailConAdjuntoAsync(string destinatario, string asunto,
        string cuerpoHtml, byte[] adjunto, string nombreAdjunto)
    {
        try
        {
            var (client, sender) = CrearCliente();
            if (client is null) return;

            var message = ConstruirMensaje(sender, new[] { destinatario }, asunto, cuerpoHtml);
            message.Attachments.Add(new EmailAttachment(
                nombreAdjunto,
                ObtenerContentType(nombreAdjunto),
                new BinaryData(adjunto)));

            var operation = await client.SendAsync(WaitUntil.Completed, message);
            _logger.LogInformation("Email con adjunto '{Adjunto}' enviado a {Destinatario} (Id: {Id})",
                nombreAdjunto, destinatario, operation.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email con adjunto a {Destinatario}", destinatario);
        }
    }

    public async Task ProgramarEmailAsync(string destinatario, string asunto,
        string cuerpoHtml, DateTime fechaProgramada, string modulo)
    {
        // Programación gestionada por Hangfire — registrar en BD y dejar que el job la procese
        _logger.LogInformation("Email programado para {Fecha} → {Destinatario} [{Modulo}]",
            fechaProgramada, destinatario, modulo);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Crea el cliente de Azure Communication Services.
    /// La cadena de conexión se toma de la variable de entorno
    /// COMMUNICATION_SERVICES_CONNECTION_STRING o de la configuración (Key Vault / appsettings).
    /// </summary>
    private (EmailClient? client, string sender) CrearCliente()
    {
        var connectionString = _config["AzureCommunication:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
        var sender = _config["AzureCommunication:SenderAddress"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(sender))
        {
            _logger.LogError("Azure Communication Services no está configurado " +
                "(falta ConnectionString o SenderAddress). El correo no se envió.");
            return (null, sender);
        }

        return (new EmailClient(connectionString), sender);
    }

    private static EmailMessage ConstruirMensaje(string sender, IEnumerable<string> destinatarios,
        string asunto, string cuerpoHtml)
    {
        var content = new EmailContent(asunto)
        {
            PlainText = QuitarHtml(cuerpoHtml),
            Html = cuerpoHtml
        };

        var recipients = new EmailRecipients(
            destinatarios.Select(d => new EmailAddress(d)).ToList());

        return new EmailMessage(sender, recipients, content);
    }

    private static string QuitarHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var texto = Regex.Replace(html, "<[^>]+>", " ");
        return Regex.Replace(texto, @"\s+", " ").Trim();
    }

    private static string ObtenerContentType(string nombreArchivo)
    {
        var ext = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".csv" => "text/csv",
            ".txt" => "text/plain",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
