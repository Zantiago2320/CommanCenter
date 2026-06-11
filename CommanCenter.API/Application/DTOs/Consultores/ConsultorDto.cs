using System.ComponentModel.DataAnnotations;

namespace CommanCenter.API.Application.DTOs.Consultores;

public class ConsultorDto
{
    public int Id { get; set; }
    public string? Cedula { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombre} {Apellido}";
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
    public string Estado { get; set; } = "Activo";
    public DateTime? FechaIngreso { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Habilitado { get; set; }
    public string? FotoUrl { get; set; }
    public string? Observaciones { get; set; }
    public List<string> Celulas { get; set; } = new();
    public List<int> CelulasIds { get; set; } = new();
    public string? MotivoDeshabilitacion { get; set; }
    public DateTime? FechaDeshabilitacion { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public class CrearConsultorDto
{
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [MaxLength(30)]
    public string Cedula { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "El celular es obligatorio.")]
    [MaxLength(20)]
    public string Celular { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [MaxLength(150)]
    public string Cargo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Rol { get; set; }

    [MaxLength(100)]
    public string? Tecnologia { get; set; }

    [MaxLength(50)]
    public string? NivelSeniority { get; set; }

    [MaxLength(50)]
    public string? Capacidad { get; set; }

    [Required(ErrorMessage = "La empresa es obligatoria.")]
    [MaxLength(150)]
    public string Empresa { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? Barrio { get; set; }

    [MaxLength(150)]
    public string? ContactoEmergenciaNombre { get; set; }

    [MaxLength(20)]
    public string? ContactoEmergenciaTelefono { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } = "Activo";

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    public DateTime? FechaIngreso { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime? FechaNacimiento { get; set; }

    public string? Observaciones { get; set; }

    [MaxLength(500)]
    public string? FotoUrl { get; set; }

    /// <summary>IDs de las células a las que se asignará el consultor (al menos una es obligatoria).</summary>
    [MinLength(1, ErrorMessage = "Debe asignar al menos una célula.")]
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
