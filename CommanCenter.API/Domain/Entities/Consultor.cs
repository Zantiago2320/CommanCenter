using CommanCenter.API.Domain.Common;

namespace CommanCenter.API.Domain.Entities;

/// <summary>
/// Módulo: DataTeam
/// Representa un consultor activo del equipo.
/// </summary>
public class Consultor : BaseEntity
{
    public string? Cedula { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Celular { get; set; }
    public string? Cargo { get; set; }
    public string? Rol { get; set; }
    public string? Tecnologia { get; set; }
    public string? NivelSeniority { get; set; }
    public string? Capacidad { get; set; }
    public string? Empresa { get; set; }
    public string? Direccion { get; set; }
    public string? Barrio { get; set; }
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    /// <summary>Estado laboral del consultor: "Activo" o "Retirado".</summary>
    public string Estado { get; set; } = "Activo";
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
