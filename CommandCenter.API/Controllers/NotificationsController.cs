using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CommandCenter.API.Application.DTOs.Common;
using CommandCenter.API.Domain.Entities;
using CommandCenter.API.Domain.Interfaces;
using CommandCenter.API.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommandCenter.API.Controllers;

/// <summary>
/// Envío de notificaciones por correo (SendGrid).
/// Aquí solo se define el "cableado": el destinatario se recibe en la petición.
/// Para enviar a una persona, basta con poner su correo en "To".
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notification;
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notification,
        AppDbContext db,
        ILogger<NotificationsController> logger)
    {
        _notification = notification;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Envía un correo a uno o varios destinatarios.
    /// El (los) destinatario(s) se define(n) en el campo "To" del cuerpo de la petición.
    /// </summary>
    [HttpPost("email")]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
