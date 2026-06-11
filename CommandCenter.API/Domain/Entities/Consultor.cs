using CommandCenter.API.Domain.Common;

namespace CommandCenter.API.Domain.Entities;

/// <summary>
/// Módulo: DataTeam
/// Representa un consultor activo del equipo.
/// </summary>
public class Consultor : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public string? Tecnologia { get; set; }
    public string? NivelSeniority { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Habilitado { get; set; } = true;
    public string? FotoUrl { get; set; }
    public string? Observaciones { get; set; }

    // Deshabilitación
    public string? MotivoDeshabilitacion { get; set; }
    public DateTime? FechaDeshabilitacion { get; set; }

    // Relaciones
    public ICollection<CelulaMiembro> Celulas { get; set; } = new List<CelulaMiembro>();
    public ICollection<CelulaLider> CelulasLideradas { get; set; } = new List<CelulaLider>();
}
