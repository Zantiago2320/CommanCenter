using System.ComponentModel.DataAnnotations;

namespace CommandCenter.API.Application.DTOs.Celulas;

public class CelulaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; }
    public int TotalMiembros { get; set; }
    public string? NombreLider { get; set; }
    public List<MiembroCelulaDto> Miembros { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
}

public class MiembroCelulaDto
{
    public int ConsultorId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public bool EsLider { get; set; }
}

public class CrearCelulaDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [MaxLength(20)]
    public string? Color { get; set; }

    public string? ImagenUrl { get; set; }
    public int? LiderId { get; set; }
}

public class ActualizarCelulaDto : CrearCelulaDto
{
    public int Id { get; set; }
}
