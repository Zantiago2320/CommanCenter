namespace CommanCenter.Portal.Models;

public class CelulaViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Color { get; set; } = "#28a745";
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; }
    public int TotalMiembros { get; set; }
    public string? NombreLider { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<MiembroCelulaViewModel> Miembros { get; set; } = [];
}

public class MiembroCelulaViewModel
{
    public int ConsultorId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Cargo { get; set; }
    public bool EsLider { get; set; }
}

public class ConsultorViewModel
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
    public bool Habilitado { get; set; }
    public string? FotoUrl { get; set; }
    public string? CelulaNombre { get; set; }
    public List<string> Celulas { get; set; } = [];
    public List<int> CelulasIds { get; set; } = [];
    public string? MotivoDeshabilitacion { get; set; }
    public DateTime? FechaDeshabilitacion { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>Datos del formulario para crear un consultor desde el Portal.</summary>
public class CrearConsultorViewModel
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
    public string? Observaciones { get; set; }

    /// <summary>IDs de las células seleccionadas (una o varias).</summary>
    public List<int> CelulasIds { get; set; } = [];
}

/// <summary>Usuario del sistema (login por nombre, no por correo).</summary>
public class UsuarioViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

/// <summary>Datos del formulario para crear un usuario desde el Portal.</summary>
public class CrearUsuarioInput
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    public string Usuario { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La contraseña es obligatoria.")]
    [System.ComponentModel.DataAnnotations.MinLength(8, ErrorMessage = "Mínimo 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}

/// <summary>Datos del formulario para crear una célula desde el Portal.</summary>
public class CrearCelulaInput
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El nombre es obligatorio.")]
    [System.ComponentModel.DataAnnotations.MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Descripcion { get; set; }

    public string Color { get; set; } = "#28a745";
}

/// <summary>Registro de auditoría: quién hizo qué y cuándo.</summary>
public class AuditoriaLogViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? Usuario { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public bool Exitoso { get; set; }
    public string? Error { get; set; }
}