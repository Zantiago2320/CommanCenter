using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize]
public class CelulasModel : PageModel
{
    private readonly IApiClient _api;
    private readonly IImageStorageService _imagenes;
    private readonly ILogger<CelulasModel> _logger;

    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    public List<CelulaViewModel> Celulas { get; set; } = [];
    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty] public CrearCelulaInput Input { get; set; } = new();
    [BindProperty] public IFormFile? Imagen { get; set; }

    public CelulasModel(IApiClient api, IImageStorageService imagenes, ILogger<CelulasModel> logger)
    {
        _api = api;
        _imagenes = imagenes;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        if (TempData["Success"] is string ok) Success = ok;
        if (TempData["Error"] is string err) Error = err;
        await CargarAsync();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarAsync();
            Error = "Revise los datos del formulario.";
            return Page();
        }

        // Subida de foto opcional de la célula
        string? imagenUrl = null;
        if (Imagen is { Length: > 0 })
        {
            var (ruta, error) = await GuardarImagenAsync(Imagen);
            if (error is not null)
            {
                await CargarAsync();
                Error = error;
                return Page();
            }
            imagenUrl = ruta;
        }

        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<CelulaViewModel>("api/celulas",
            new { Input.Nombre, Input.Descripcion, Input.Color, ImagenUrl = imagenUrl }, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Célula {Nombre} creada desde el Portal", Input.Nombre);
            TempData["Success"] = $"Célula '{Input.Nombre}' creada correctamente.";
        }
        else
        {
            TempData["Error"] = result?.Mensaje ?? "No se pudo crear la célula.";
        }

        return RedirectToPage();
    }

    /// <summary>
    /// Sube la imagen (Blob Storage o local) y devuelve la URL pública.
    /// </summary>
    private async Task<(string? ruta, string? error)> GuardarImagenAsync(IFormFile imagen)
    {
        var extension = Path.GetExtension(imagen.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
            return (null, "Formato de imagen no permitido. Use JPG, PNG, GIF o WEBP.");

        if (imagen.Length > TamanoMaximoBytes)
            return (null, "La imagen supera el tamaño máximo de 5 MB.");

        var nombreArchivo = $"celula_{Guid.NewGuid():N}{extension}";
        await using var stream = imagen.OpenReadStream();
        var url = await _imagenes.SubirAsync(stream, nombreArchivo, "celulas", imagen.ContentType);
        return (url, null);
    }

    private async Task CargarAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.GetAsync<List<CelulaViewModel>>("api/celulas", token);

        if (result?.Exitoso == true && result.Data is not null)
            Celulas = result.Data;
        else
            Error ??= result?.Mensaje ?? "No se pudieron cargar las células.";
    }
}
