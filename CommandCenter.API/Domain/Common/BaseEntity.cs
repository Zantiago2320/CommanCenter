namespace CommandCenter.API.Domain.Common;

/// <summary>
/// Entidad base para todas las entidades del dominio.
/// Aplica a todos los módulos: DataTeam, RRHH, DevSecOps, etc.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
    public string? CreadoPor { get; set; }
    public string? ModificadoPor { get; set; }
    public bool Activo { get; set; } = true;
}
