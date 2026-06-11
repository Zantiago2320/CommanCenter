using CommandCenter.API.Domain.Common;

namespace CommandCenter.API.Domain.Entities;

/// <summary>
/// Auditoría centralizada. Aplica a TODOS los módulos del Command Center.
/// </summary>
public class AuditoriaLog : BaseEntity
{
    public string Modulo { get; set; } = string.Empty;       // DataTeam, RRHH, etc.
    public string Accion { get; set; } = string.Empty;       // CREATE, UPDATE, DELETE, LOGIN
    public string Entidad { get; set; } = string.Empty;      // Consultor, Celula, etc.
    public string? EntidadId { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Exitoso { get; set; } = true;
    public string? MensajeError { get; set; }
}
