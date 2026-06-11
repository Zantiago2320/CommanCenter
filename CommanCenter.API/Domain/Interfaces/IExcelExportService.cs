using CommanCenter.API.Domain.Entities;

namespace CommanCenter.API.Domain.Interfaces;

/// <summary>
/// Generación de archivos Excel (.xlsx) para exportación y adjuntos de correo.
/// </summary>
public interface IExcelExportService
{
    /// <summary>Genera el directorio completo de consultores con todos sus campos.</summary>
    byte[] GenerarDirectorio(IEnumerable<Consultor> consultores);

    /// <summary>Genera el listado de cumpleaños del mes (día, nombre, cargo, célula).</summary>
    byte[] GenerarCumpleaniosDelMes(IEnumerable<Consultor> cumpleanieros, string nombreMes);

    /// <summary>Genera el reporte mensual de trabajadores activos.</summary>
    byte[] GenerarReporteMensual(IEnumerable<Consultor> consultores, string periodo);
}
