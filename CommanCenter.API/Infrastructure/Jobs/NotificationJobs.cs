using System.Text;
using CommanCenter.API.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CommanCenter.API.Infrastructure.Jobs;

/// <summary>
/// Jobs automáticos de notificaciones por correo, ejecutados por Hangfire.
/// - Cumpleaños: felicita a los consultores que cumplen años hoy.
/// - Reporte mensual: envía un resumen del equipo a los destinatarios configurados.
/// Los correos destinatarios se configuran en appsettings.json → sección "Notificaciones".
/// </summary>
public class NotificationJobs
{
    private readonly IConsultorRepository _consultorRepo;
    private readonly INotificationService _notificacion;
    private readonly IExcelExportService _excel;
    private readonly IConfiguration _config;
    private readonly ILogger<NotificationJobs> _logger;

    public NotificationJobs(
        IConsultorRepository consultorRepo,
        INotificationService notificacion,
        IExcelExportService excel,
        IConfiguration config,
        ILogger<NotificationJobs> logger)
    {
        _consultorRepo = consultorRepo;
        _notificacion = notificacion;
        _excel = excel;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Envía un recordatorio a destinatarios específicos con la lista de
    /// consultores que cumplen años en el mes actual.
    /// Los correos destinatarios se configuran en appsettings.json →
    /// "Notificaciones:RecordatorioCumpleaniosDestinatarios".
    /// </summary>
    public async Task EnviarRecordatorioCumpleaniosDelMesAsync()
    {
        var destinatarios = (_config.GetSection("Notificaciones:RecordatorioCumpleaniosDestinatarios").Get<string[]>()
            ?? Array.Empty<string>())
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .ToArray();

        if (destinatarios.Length == 0)
        {
            _logger.LogWarning("Job recordatorio cumpleaños: no hay destinatarios configurados " +
                "(Notificaciones:RecordatorioCumpleaniosDestinatarios). No se envía.");
            return;
        }

        var mesActual = DateTime.UtcNow.Month;
        var nombreMes = DateTime.UtcNow.ToString("MMMM");
        var cumpleanieros = (await _consultorRepo.GetCumpleaniosDelMesAsync(mesActual)).ToList();

        var sb = new StringBuilder();
        sb.Append($"<h2>Recordatorio de cumpleaños — {nombreMes}</h2>");

        if (cumpleanieros.Count == 0)
        {
            sb.Append($"<p>No hay consultores que cumplan años en {nombreMes}.</p>");
        }
        else
        {
            sb.Append($"<p>Estos consultores cumplen años en {nombreMes}:</p>");
            sb.Append("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>");
            sb.Append("<tr><th>Día</th><th>Nombre</th><th>Apellido</th><th>Cargo</th><th>Célula</th></tr>");
            foreach (var c in cumpleanieros)
            {
                var dia = c.FechaNacimiento!.Value.Day;
                var celulas = c.Celulas != null && c.Celulas.Count > 0
                    ? string.Join(", ", c.Celulas.Select(cm => cm.Celula?.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)))
                    : "—";
                sb.Append("<tr>");
                sb.Append($"<td>{dia} de {nombreMes}</td>");
                sb.Append($"<td>{c.Nombre}</td>");
                sb.Append($"<td>{c.Apellido}</td>");
                sb.Append($"<td>{(string.IsNullOrWhiteSpace(c.Cargo) ? "—" : c.Cargo)}</td>");
                sb.Append($"<td>{celulas}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
        }

        var asunto = $"🎂 Cumpleaños del mes — {nombreMes}";
        var cuerpo = sb.ToString();

        // Adjunta el Excel con el listado de cumpleaños del mes.
        if (cumpleanieros.Count > 0)
        {
            var excel = _excel.GenerarCumpleaniosDelMes(cumpleanieros, nombreMes);
            var nombreArchivo = $"cumpleanios_{nombreMes}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            foreach (var dest in destinatarios)
                await _notificacion.EnviarEmailConAdjuntoAsync(dest, asunto, cuerpo, excel, nombreArchivo);
        }
        else
        {
            foreach (var dest in destinatarios)
                await _notificacion.EnviarEmailAsync(dest, asunto, cuerpo);
        }

        _logger.LogInformation("Job recordatorio cumpleaños: {Cantidad} cumpleañeros en {Mes}, enviado a {Destinatarios} destinatarios.",
            cumpleanieros.Count, nombreMes, destinatarios.Length);
    }

    /// <summary>Envía el reporte mensual con los trabajadores actuales (activos) a los destinatarios configurados.</summary>
    public async Task EnviarReporteMensualAsync()
    {
        var destinatarios = (_config.GetSection("Notificaciones:ReporteMensualDestinatarios").Get<string[]>()
            ?? Array.Empty<string>())
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .ToArray();

        if (destinatarios.Length == 0)
        {
            _logger.LogWarning("Job reporte mensual: no hay destinatarios configurados " +
                "(Notificaciones:ReporteMensualDestinatarios). No se envía.");
            return;
        }

        var habilitados = (await _consultorRepo.GetHabilitadosAsync()).ToList();
        var periodo = DateTime.UtcNow.ToString("MMMM yyyy");

        var sb = new StringBuilder();
        sb.Append($"<h2>Trabajadores actuales — {periodo}</h2>");
        sb.Append($"<p>Total de consultores activos: <strong>{habilitados.Count}</strong></p>");

        if (habilitados.Count == 0)
        {
            sb.Append("<p>No hay trabajadores activos registrados.</p>");
        }
        else
        {
            sb.Append("<table border='1' cellpadding='6' cellspacing='0' style='border-collapse:collapse;'>");
            sb.Append("<tr><th>Nombre</th><th>Apellido</th><th>Cargo</th><th>Célula</th><th>Email</th></tr>");
            foreach (var c in habilitados)
            {
                var celulas = c.Celulas != null && c.Celulas.Count > 0
                    ? string.Join(", ", c.Celulas.Select(cm => cm.Celula?.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)))
                    : "—";
                sb.Append("<tr>");
                sb.Append($"<td>{c.Nombre}</td>");
                sb.Append($"<td>{c.Apellido}</td>");
                sb.Append($"<td>{(string.IsNullOrWhiteSpace(c.Cargo) ? "—" : c.Cargo)}</td>");
                sb.Append($"<td>{celulas}</td>");
                sb.Append($"<td>{c.Email}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
        }

        var asunto = $"📊 Trabajadores actuales — {periodo}";
        var cuerpo = sb.ToString();

        // Adjunta el Excel con el reporte de trabajadores activos.
        if (habilitados.Count > 0)
        {
            var excel = _excel.GenerarReporteMensual(habilitados, periodo);
            var nombreArchivo = $"reporte_mensual_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            foreach (var dest in destinatarios)
                await _notificacion.EnviarEmailConAdjuntoAsync(dest, asunto, cuerpo, excel, nombreArchivo);
        }
        else
        {
            foreach (var dest in destinatarios)
                await _notificacion.EnviarEmailAsync(dest, asunto, cuerpo);
        }

        _logger.LogInformation("Job reporte mensual: {Cantidad} trabajadores, enviado a {Destinatarios} destinatarios.",
            habilitados.Count, destinatarios.Length);
    }

    /// <summary>
    /// Job programado: envía el recordatorio de cumpleaños del mes SOLO si hoy
    /// es el primer día hábil del mes (lunes a viernes). Se programa para correr
    /// los primeros días del mes; si no es el primer día hábil, no hace nada.
    /// </summary>
    public async Task EnviarRecordatorioCumpleaniosPrimerDiaHabilAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        if (!EsPrimerDiaHabilDelMes(hoy))
        {
            _logger.LogInformation("Job cumpleaños programado: hoy {Fecha} no es el primer día hábil del mes. No se envía.", hoy);
            return;
        }

        await EnviarRecordatorioCumpleaniosDelMesAsync();
    }

    /// <summary>Determina si la fecha dada es el primer día hábil (lunes a viernes) del mes.</summary>
    private static bool EsPrimerDiaHabilDelMes(DateTime fecha)
    {
        var primerDiaHabil = new DateTime(fecha.Year, fecha.Month, 1);
        while (primerDiaHabil.DayOfWeek == DayOfWeek.Saturday || primerDiaHabil.DayOfWeek == DayOfWeek.Sunday)
            primerDiaHabil = primerDiaHabil.AddDays(1);

        return fecha.Date == primerDiaHabil.Date;
    }
}
