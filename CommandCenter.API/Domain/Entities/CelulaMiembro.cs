namespace CommandCenter.API.Domain.Entities;

/// <summary>
/// Tabla de unión: Consultor ↔ Célula (miembros)
/// </summary>
public class CelulaMiembro
{
    public int Id { get; set; }
    public int CelulaId { get; set; }
    public int ConsultorId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    public Celula Celula { get; set; } = null!;
    public Consultor Consultor { get; set; } = null!;
}
