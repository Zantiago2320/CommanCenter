using CommandCenter.API.Domain.Common;

namespace CommandCenter.API.Domain.Entities;

/// <summary>
/// Notificaciones programadas. Aplica a TODOS los módulos.
/// </summary>
public class Notificacion : BaseEntity
{
    public string Modulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;         // Email, SMS, Push
    public string Destinatario { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Cuerpo { get; set; } = string.Empty;
    public bool Enviado { get; set; } = false;
    public DateTime? FechaEnvio { get; set; }
    public DateTime? FechaProgramada { get; set; }
    public int Intentos { get; set; } = 0;
    public string? ErrorMensaje { get; set; }
    public string? AdjuntoUrl { get; set; }
}
