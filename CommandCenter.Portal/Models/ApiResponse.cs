namespace CommanCenter.Portal.Models;

/// <summary>Envuelve todas las respuestas que llegan de la API.</summary>
public class ApiResponse<T>
{
    public bool Exitoso { get; set; }
    public string? Mensaje { get; set; }
    public T? Data { get; set; }
    public List<string> Errores { get; set; } = [];
    public DateTime Timestamp { get; set; }
}

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
