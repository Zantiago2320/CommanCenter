using ClosedXML.Excel;
using CommanCenter.API.Domain.Entities;
using CommanCenter.API.Domain.Interfaces;

namespace CommanCenter.API.Infrastructure.Services;

/// <summary>
/// Generación de archivos Excel (.xlsx) usando ClosedXML.
/// Reutilizable para exportaciones y adjuntos de correo.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    private const string ColorEncabezado = "#28a745";

    public byte[] GenerarDirectorio(IEnumerable<Consultor> consultores)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Consultores");

        string[] encabezados =
        {
            "Cédula", "Nombre", "Apellido", "Email", "Teléfono", "Celular",
            "Cargo", "Rol", "Tecnología", "Seniority", "Capacidad", "Empresa",
            "Dirección", "Barrio", "Contacto emergencia", "Tel. emergencia",
            "Estado", "Fecha ingreso", "Fecha nacimiento", "Células", "Habilitado"
        };

        EscribirEncabezados(ws, encabezados);

        var fila = 2;
        foreach (var c in consultores)
        {
            var col = 1;
            ws.Cell(fila, col++).Value = c.Cedula ?? "—";
            ws.Cell(fila, col++).Value = c.Nombre;
            ws.Cell(fila, col++).Value = c.Apellido;
            ws.Cell(fila, col++).Value = c.Email;
            ws.Cell(fila, col++).Value = c.Telefono ?? "—";
            ws.Cell(fila, col++).Value = c.Celular ?? "—";
            ws.Cell(fila, col++).Value = c.Cargo ?? "—";
            ws.Cell(fila, col++).Value = c.Rol ?? "—";
            ws.Cell(fila, col++).Value = c.Tecnologia ?? "—";
            ws.Cell(fila, col++).Value = c.NivelSeniority ?? "—";
            ws.Cell(fila, col++).Value = c.Capacidad ?? "—";
            ws.Cell(fila, col++).Value = c.Empresa ?? "—";
            ws.Cell(fila, col++).Value = c.Direccion ?? "—";
            ws.Cell(fila, col++).Value = c.Barrio ?? "—";
            ws.Cell(fila, col++).Value = c.ContactoEmergenciaNombre ?? "—";
            ws.Cell(fila, col++).Value = c.ContactoEmergenciaTelefono ?? "—";
            ws.Cell(fila, col++).Value = c.Estado;
            ws.Cell(fila, col++).Value = c.FechaIngreso?.ToString("dd/MM/yyyy") ?? "—";
            ws.Cell(fila, col++).Value = c.FechaNacimiento?.ToString("dd/MM/yyyy") ?? "—";
            ws.Cell(fila, col++).Value = ObtenerCelulas(c);
            ws.Cell(fila, col++).Value = c.Habilitado ? "Sí" : "No";
            fila++;
        }

        Finalizar(ws, encabezados.Length);
        return Serializar(workbook);
    }

    public byte[] GenerarCumpleaniosDelMes(IEnumerable<Consultor> cumpleanieros, string nombreMes)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Cumpleaños {nombreMes}");

        string[] encabezados = { "Día", "Nombre", "Apellido", "Cargo", "Célula", "Email" };
        EscribirEncabezados(ws, encabezados);

        var fila = 2;
        foreach (var c in cumpleanieros.OrderBy(x => x.FechaNacimiento?.Day ?? 0))
        {
            var col = 1;
            ws.Cell(fila, col++).Value = c.FechaNacimiento.HasValue
                ? $"{c.FechaNacimiento.Value.Day} de {nombreMes}"
                : "—";
            ws.Cell(fila, col++).Value = c.Nombre;
            ws.Cell(fila, col++).Value = c.Apellido;
            ws.Cell(fila, col++).Value = c.Cargo ?? "—";
            ws.Cell(fila, col++).Value = ObtenerCelulas(c);
            ws.Cell(fila, col++).Value = c.Email;
            fila++;
        }

        Finalizar(ws, encabezados.Length);
        return Serializar(workbook);
    }

    public byte[] GenerarReporteMensual(IEnumerable<Consultor> consultores, string periodo)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Reporte mensual");

        string[] encabezados = { "Nombre", "Apellido", "Cargo", "Empresa", "Célula", "Email", "Estado" };
        EscribirEncabezados(ws, encabezados);

        var fila = 2;
        foreach (var c in consultores)
        {
            var col = 1;
            ws.Cell(fila, col++).Value = c.Nombre;
            ws.Cell(fila, col++).Value = c.Apellido;
            ws.Cell(fila, col++).Value = c.Cargo ?? "—";
            ws.Cell(fila, col++).Value = c.Empresa ?? "—";
            ws.Cell(fila, col++).Value = ObtenerCelulas(c);
            ws.Cell(fila, col++).Value = c.Email;
            ws.Cell(fila, col++).Value = c.Estado;
            fila++;
        }

        Finalizar(ws, encabezados.Length);
        return Serializar(workbook);
    }

    private static string ObtenerCelulas(Consultor c) =>
        c.Celulas != null && c.Celulas.Count > 0
            ? string.Join(", ", c.Celulas.Select(cm => cm.Celula?.Nombre).Where(n => !string.IsNullOrWhiteSpace(n)))
            : "—";

    private static void EscribirEncabezados(IXLWorksheet ws, string[] encabezados)
    {
        for (var i = 0; i < encabezados.Length; i++)
        {
            var celda = ws.Cell(1, i + 1);
            celda.Value = encabezados[i];
            celda.Style.Font.Bold = true;
            celda.Style.Font.FontColor = XLColor.White;
            celda.Style.Fill.BackgroundColor = XLColor.FromHtml(ColorEncabezado);
            celda.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
    }

    private static void Finalizar(IXLWorksheet ws, int columnas)
    {
        ws.SheetView.FreezeRows(1);
        ws.Range(1, 1, 1, columnas).SetAutoFilter();
        ws.Columns().AdjustToContents();
    }

    private static byte[] Serializar(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
