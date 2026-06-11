using CommandCenter.API.Domain.Common;

namespace CommandCenter.API.Domain.Entities;

/// <summary>
/// Módulo: DataTeam
/// Representa una célula de trabajo del equipo.
/// </summary>
public class Celula : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Color { get; set; } = "#28a745";
    public string? ImagenUrl { get; set; }

    // Relaciones
    public ICollection<CelulaMiembro> Miembros { get; set; } = new List<CelulaMiembro>();
    public ICollection<CelulaLider> Lideres { get; set; } = new List<CelulaLider>();
}
