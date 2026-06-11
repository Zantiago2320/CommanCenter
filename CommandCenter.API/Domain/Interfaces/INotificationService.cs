namespace CommandCenter.API.Domain.Interfaces;

/// <summary>
/// Contrato para envío de notificaciones. Implementado en la capa de Infrastructure.
/// Toda notificación del sistema pasa por aquí, nunca directamente desde servicios de negocio.
/// </summary>
public interface INotificationService
{
    Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml, string? adjuntoUrl = null);
    Task EnviarEmailConAdjuntoAsync(string destinatario, string asunto, string cuerpoHtml, byte[] adjunto, string nombreAdjunto);
    Task ProgramarEmailAsync(string destinatario, string asunto, string cuerpoHtml, DateTime fechaProgramada, string modulo);
}
