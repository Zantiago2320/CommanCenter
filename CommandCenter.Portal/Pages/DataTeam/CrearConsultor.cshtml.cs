using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize]
public class CrearConsultorModel : PageModel
{
    private readonly IApiClient _api;
    private readonly IImageStorageService _imagenes;
    private readonly ILogger<CrearConsultorModel> _logger;

    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    [BindProperty] public CrearConsultorViewModel Input { get; set; } = new();
    [BindProperty] public IFormFile? Foto { get; set; }

    /// <summary>Células disponibles para asignar (se cargan desde la API).</summary>
    public List<CelulaViewModel> CelulasDisponibles { get; set; } = [];

    public string? Error { get; set; }
    public string? Success { get; set; }

    public CrearConsultorModel(IApiClient api, IImageStorageService imagenes, ILogger<CrearConsultorModel> logger)
    {
        _api = api;
        _imagenes = imagenes;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        await CargarCelulasAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarCelulasAsync();

        if (string.IsNullOrWhiteSpace(Input.Nombre) ||
            string.IsNullOrWhiteSpace(Input.Apellido) ||
            string.IsNullOrWhiteSpace(Input.Email))
        {
            Error = "Nombre, apellido y email son obligatorios.";
            return Page();
        }

        var token = HttpContext.Session.GetString("jwt_token");

        // Subida de foto de perfil opcional
        string? fotoUrl = null;
        if (Foto is { Length: > 0 })
        {
            var (ruta, error) = await GuardarImagenAsync(Foto);
            if (error is not null)
            {
                Error = error;
                return Page();
            }
            fotoUrl = ruta;
        }

        // El cuerpo coincide con CrearConsultorDto de la API (incluye CelulasIds).
        var body = new
        {
            Input.Nombre,
            Input.Apellido,
            Input.Email,
            Input.Telefono,
            Input.Cargo,
            Input.Tecnologia,
            Input.NivelSeniority,
            Input.FechaIngreso,
            Input.FechaNacimiento,
            Input.Observaciones,
            FotoUrl = fotoUrl,
            CelulasIds = Input.CelulasIds
        };

        var result = await _api.PostAsync<ConsultorViewModel>("api/consultores", body, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Consultor {Email} creado desde el Portal", Input.Email);
            TempData["Success"] = $"Consultor {Input.Nombre} {Input.Apellido} creado correctamente.";
            return RedirectToPage("/DataTeam/Consultores");
        }

        Error = result?.Mensaje ?? "No se pudo crear el consultor.";
        return Page();
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

        var nombreArchivo = $"consultor_{Guid.NewGuid():N}{extension}";
        await using var stream = imagen.OpenReadStream();
        var url = await _imagenes.SubirAsync(stream, nombreArchivo, "consultores", imagen.ContentType);
        return (url, null);
    }

    private async Task CargarCelulasAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.GetAsync<List<CelulaViewModel>>("api/celulas", token);
        if (result?.Exitoso == true && result.Data is not null)
            CelulasDisponibles = result.Data;
    }
}
