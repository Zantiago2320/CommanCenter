using System.ComponentModel.DataAnnotations;

namespace CommandCenter.API.Application.DTOs.Consultores;

public class ConsultorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre} {Apellido}";
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Cargo { get; set; }
    public string? Tecnologia { get; set; }
    public string? NivelSeniority { get; set; }
    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Habilitado { get; set; }
    public string? FotoUrl { get; set; }
    public List<string> Celulas { get; set; } = new();
    public List<int> CelulasIds { get; set; } = new();
    public string? MotivoDeshabilitacion { get; set; }
    public DateTime? FechaDeshabilitacion { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CrearConsultorDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    public string? Cargo { get; set; }

    [MaxLength(100)]
    public string? Tecnologia { get; set; }

    [MaxLength(50)]
    public string? NivelSeniority { get; set; }

    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Observaciones { get; set; }

    [MaxLength(500)]
    public string? FotoUrl { get; set; }

    /// <summary>IDs de las células a las que se asignará el consultor (puede ser una o varias).</summary>
    public List<int> CelulasIds { get; set; } = new();
}

public class ActualizarConsultorDto : CrearConsultorDto
{
    public int Id { get; set; }
    public bool Habilitado { get; set; } = true;
}

/// <summary>Datos para deshabilitar un consultor indicando el motivo.</summary>
public class DeshabilitarConsultorDto
{
    [Required(ErrorMessage = "El motivo de deshabilitación es obligatorio.")]
    [MaxLength(500)]
    public string Motivo { get; set; } = string.Empty;
}
