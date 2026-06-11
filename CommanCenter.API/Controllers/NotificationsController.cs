using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CommanCenter.API.Application.DTOs.Common;
using CommanCenter.API.Domain.Entities;
using CommanCenter.API.Domain.Interfaces;
using CommanCenter.API.Infrastructure.Data;
using CommanCenter.API.Infrastructure.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommanCenter.API.Controllers;

/// <summary>
/// Envío de notificaciones por correo.
/// El envío manual de correos está reservado a Admin y Senior.
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notification;
    private readonly NotificationJobs _jobs;
    private readonly IConsultorRepository _consultorRepo;
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notification,
        NotificationJobs jobs,
        IConsultorRepository consultorRepo,
        AppDbContext db,
        ILogger<NotificationsController> logger)
    {
        _notification = notification;
        _jobs = jobs;
        _consultorRepo = consultorRepo;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Vista previa del correo de cumpleaños del mes: muestra a quiénes se
    /// enviará, los destinatarios configurados y los datos que irán en el Excel adjunto.
    /// </summary>
    [HttpGet("cumpleanios-mes/preview")]
    [Authorize(Roles = "Admin,Senior")]
    public async Task<IActionResult> PreviewCumpleaniosDelMes()
    {
        var nombreMes = DateTime.UtcNow.ToString("MMMM");
        var cumpleanieros = (await _consultorRepo.GetCumpleaniosDelMesAsync(DateTime.UtcNow.Month)).ToList();

        var preview = new PreviewCorreoDto
        {
            Titulo = $"Cumpleaños del mes — {nombreMes}",
            Periodo = nombreMes,
            Destinatarios = ObtenerDestinatarios("Notificaciones:RecordatorioCumpleaniosDestinatarios"),
            Total = cumpleanieros.Count,
            Adjunto = $"cumpleanios_{nombreMes}.xlsx",
            Items = cumpleanieros.Select(c => new PreviewItemDto
            {
                Detalle = c.FechaNacimiento.HasValue ? $"{c.FechaNacimiento.Value.Day} de {nombreMes}" : "—",
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Cargo = c.Cargo ?? "—",
                Celula = ObtenerCelulas(c),
                Email = c.Email
            }).ToList()
        };

        return Ok(ApiResponse<PreviewCorreoDto>.Ok(preview));
    }

    /// <summary>
    /// Vista previa del reporte mensual: trabajadores activos, destinatarios
    /// configurados y datos que irán en el Excel adjunto.
    /// </summary>
    [HttpGet("reporte-mensual/preview")]
    [Authorize(Roles = "Admin,Senior")]
    public async Task<IActionResult> PreviewReporteMensual()
    {
        var periodo = DateTime.UtcNow.ToString("MMMM yyyy");
        var habilitados = (await _consultorRepo.GetHabilitadosAsync()).ToList();

        var preview = new PreviewCorreoDto
        {
            Titulo = $"Trabajadores actuales — {periodo}",
            Periodo = periodo,
            Destinatarios = ObtenerDestinatarios("Notificaciones:ReporteMensualDestinatarios"),
            Total = habilitados.Count,
            Adjunto = "reporte_mensual.xlsx",
            Items = habilitados.Select(c => new PreviewItemDto
            {
                Detalle = c.Estado,
                Nombre = c.Nombre,
                Apellido = c.Apellido,
                Cargo = c.Cargo ?? "—",
                Celula = ObtenerCelulas(c),
                Email = c.Email
            }).ToList()
        };

        return Ok(ApiResponse<PreviewCorreoDto>.Ok(preview));
    }

    private List<string> ObtenerDestinatarios(string clave)
    {
        var config = HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
        return (config?.GetSection(clave).Get<string[]>() ?? Array.Empty<string>())
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .ToList();
    }

    private static string ObtenerCelulas(Domain.Entities.Consultor c) =>
        c.Celulas != null && c.Celulas.Count > 0
            ? string.Join(", ", c.Celulas.Select(cm => cm.Celula?.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)))
            : "—";

    /// <summary>
    /// Envía un correo a uno o varios destinatarios.
    /// El (los) destinatario(s) se define(n) en el campo "To" del cuerpo de la petición.
    /// </summary>
    [HttpPost("email")]
    [Authorize(Roles = "Admin,Senior")]
    public async Task<IActionResult> EnviarEmail([FromBody] SendEmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.Fail("Datos inválidos.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var usuarioId = User.FindFirstValue(ClaimTypes.Name) ?? "system";

        // Permite separar varios destinatarios por coma o punto y coma.
        var destinatarios = request.To
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();

        if (destinatarios.Count == 0)
            return BadRequest(ApiResponse<bool>.Fail("Debe indicar al menos un destinatario."));

        var conError = new List<string>();

        foreach (var destinatario in destinatarios)
        {
            // Registro de la notificación (queda trazada en la BD aunque falle el envío)
            var registro = new Notificacion
            {
                Modulo = string.IsNullOrWhiteSpace(request.Modulo) ? "General" : request.Modulo,
                Tipo = "Email",
                Destinatario = destinatario,
                Asunto = request.Subject,
                Cuerpo = request.Body,
                FechaProgramada = request.FechaProgramada,
                CreadoPor = usuarioId
            };

            try
            {
                // Si no hay fecha programada o ya pasó, se envía de inmediato.
                if (request.FechaProgramada is null || request.FechaProgramada <= DateTime.UtcNow)
                {
                    await _notification.EnviarEmailAsync(destinatario, request.Subject, request.Body);
                    registro.Enviado = true;
                    registro.FechaEnvio = DateTime.UtcNow;
                    registro.Intentos = 1;
                }
                else
                {
                    // Queda registrado como pendiente para envío programado.
                    await _notification.ProgramarEmailAsync(destinatario, request.Subject,
                        request.Body, request.FechaProgramada.Value, registro.Modulo);
                    registro.Enviado = false;
                }
            }
            catch (Exception ex)
            {
                registro.Enviado = false;
                registro.Intentos = 1;
                registro.ErrorMensaje = ex.Message;
                conError.Add(destinatario);
                _logger.LogError(ex, "Error al enviar correo a {Destinatario}", destinatario);
            }

            await _db.Notificaciones.AddAsync(registro);
        }

        await _db.SaveChangesAsync();

        if (conError.Count > 0)
            return Ok(ApiResponse<bool>.Fail(
                $"Algunos correos no se pudieron enviar: {string.Join(", ", conError)}.",
                conError));

        return Ok(ApiResponse<bool>.Ok(true,
            destinatarios.Count == 1
                ? "Correo enviado correctamente."
                : $"Correo enviado a {destinatarios.Count} destinatarios."));
    }

    /// <summary>
    /// Envía manualmente el recordatorio con los cumpleaños del mes actual
    /// a los destinatarios configurados en appsettings (Notificaciones).
    /// </summary>
    [HttpPost("cumpleanios-mes")]
    [Authorize(Roles = "Admin,Senior")]
    public async Task<IActionResult> EnviarCumpleaniosDelMes()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        await _jobs.EnviarRecordatorioCumpleaniosDelMesAsync();
        _logger.LogInformation("Recordatorio de cumpleaños del mes enviado manualmente por {Usuario}", usuarioId);
        return Ok(ApiResponse<bool>.Ok(true, "Recordatorio de cumpleaños del mes enviado a los destinatarios configurados."));
    }

    /// <summary>
    /// Envía manualmente el reporte mensual con los trabajadores actuales
    /// a los destinatarios configurados en appsettings (Notificaciones).
    /// </summary>
    [HttpPost("reporte-mensual")]
    [Authorize(Roles = "Admin,Senior")]
    public async Task<IActionResult> EnviarReporteMensual()
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        await _jobs.EnviarReporteMensualAsync();
        _logger.LogInformation("Reporte mensual enviado manualmente por {Usuario}", usuarioId);
        return Ok(ApiResponse<bool>.Ok(true, "Reporte mensual enviado a los destinatarios configurados."));
    }

    /// <summary>Obtiene el historial de notificaciones enviadas a un destinatario (por correo).</summary>
    [HttpGet("{destinatario}")]
    public async Task<IActionResult> GetByDestinatario(string destinatario)
    {
        var lista = _db.Notificaciones
            .Where(n => n.Destinatario == destinatario)
            .OrderByDescending(n => n.FechaCreacion)
            .Select(n => new NotificacionDto
            {
                Id = n.Id.ToString(),
                Tipo = n.Tipo,
                Asunto = n.Asunto,
                Mensaje = n.Cuerpo,
                Enviado = n.Enviado,
                FechaCreacion = n.FechaCreacion,
                FechaEnvio = n.FechaEnvio
            })
            .ToList();

        return Ok(ApiResponse<IEnumerable<NotificacionDto>>.Ok(lista));
    }
}

/// <summary>Petición para enviar un correo. El destinatario va en "To".</summary>
public class SendEmailRequest
{
    /// <summary>Correo(s) destino. Se pueden separar varios con coma o punto y coma.</summary>
    [Required(ErrorMessage = "El destinatario es obligatorio.")]
    public string To { get; set; } = string.Empty;

    [Required(ErrorMessage = "El asunto es obligatorio.")]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cuerpo del correo es obligatorio.")]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    /// <summary>Módulo que origina el correo (opcional). Ej: DataTeam, RRHH.</summary>
    public string? Modulo { get; set; }

    /// <summary>Fecha programada de envío (opcional). Si es null, se envía de inmediato.</summary>
    public DateTime? FechaProgramada { get; set; }
}

public class NotificacionDto
{
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Asunto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Enviado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaEnvio { get; set; }
}

/// <summary>Vista previa de un correo automatizado (cumpleaños o reporte mensual).</summary>
public class PreviewCorreoDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Periodo { get; set; } = string.Empty;
    public List<string> Destinatarios { get; set; } = new();
    public int Total { get; set; }
    public string Adjunto { get; set; } = string.Empty;
    public List<PreviewItemDto> Items { get; set; } = new();
}

public class PreviewItemDto
{
    public string Detalle { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Celula { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
