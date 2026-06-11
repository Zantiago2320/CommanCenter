namespace CommandCenter.API.Application.DTOs.Common;

/// <summary>
/// Respuesta estandarizada para todos los endpoints del Command Center.
/// </summary>
public class ApiResponse<T>
{
    public bool Exitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errores { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string mensaje = "Operación exitosa") =>
        new() { Exitoso = true, Mensaje = mensaje, Data = data };

    public static ApiResponse<T> Fail(string mensaje, List<string>? errores = null) =>
        new() { Exitoso = false, Mensaje = mensaje, Errores = errores ?? new() };

    public static ApiResponse<T> NotFound(string entidad) =>
        Fail($"{entidad} no encontrado.");
}

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
}
