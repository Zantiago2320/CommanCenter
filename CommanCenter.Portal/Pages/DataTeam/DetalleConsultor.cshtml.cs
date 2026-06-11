using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize(Roles = "Admin,Supervisor,Senior")]
public class DetalleConsultorModel : PageModel
{
    private readonly IApiClient _api;
    private readonly IImageStorageService _imagenes;
    private readonly ILogger<DetalleConsultorModel> _logger;

    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public EditarConsultorViewModel Input { get; set; } = new();
    [BindProperty] public IFormFile? Foto { get; set; }

    public List<CelulaViewModel> CelulasDisponibles { get; set; } = [];

    /// <summary>Solo Admin y Supervisor pueden editar; Senior solo visualiza.</summary>
    public bool PuedeEditar => User.IsInRole("Admin") || User.IsInRole("Supervisor");

    public string? Error { get; set; }

    public DetalleConsultorModel(IApiClient api, IImageStorageService imagenes, ILogger<DetalleConsultorModel> logger)
    {
        _api = api;
        _imagenes = imagenes;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await CargarCelulasAsync();

        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.GetAsync<ConsultorViewModel>($"api/consultores/{Id}", token);

        if (result?.Exitoso != true || result.Data is null)
        {
            Error = result?.Mensaje ?? "No se encontró el consultor.";
            return Page();
        }

        Input = MapToInput(result.Data);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarCelulasAsync();

        if (!PuedeEditar)
        {
            Error = "No tiene permisos para editar consultores.";
            return Page();
        }

        if (!ModelState.IsValid)
        {
            Error = "Revise los campos obligatorios marcados.";
            return Page();
        }

        var token = HttpContext.Session.GetString("jwt_token");

        // La foto es opcional al editar: si no se sube una nueva, se conserva la actual.
        if (Foto is { Length: > 0 })
        {
            var (fotoUrl, errorFoto) = await GuardarImagenAsync(Foto);
            if (errorFoto is not null)
            {
                Error = errorFoto;
                return Page();
            }
            Input.FotoUrl = fotoUrl;
        }

        var body = new
        {
            Input.Id,
            Input.Cedula,
            Input.Nombre,
            Input.Apellido,
            Input.Email,
            Input.Telefono,
            Input.Celular,
            Input.Cargo,
            Input.Rol,
            Input.Tecnologia,
            Input.NivelSeniority,
            Input.Capacidad,
            Input.Empresa,
            Input.Direccion,
            Input.Barrio,
            Input.ContactoEmergenciaNombre,
            Input.ContactoEmergenciaTelefono,
            Input.Estado,
            Input.FechaIngreso,
            Input.FechaNacimiento,
            Input.Observaciones,
            Input.FotoUrl,
            Input.Habilitado,
            CelulasIds = Input.CelulasIds
        };

        var result = await _api.PutAsync<ConsultorViewModel>($"api/consultores/{Id}", body, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Consultor {Id} actualizado desde el Portal", Id);
            TempData["Success"] = $"Consultor {Input.Nombre} {Input.Apellido} actualizado correctamente.";
            return RedirectToPage("/DataTeam/Consultores");
        }

        Error = result?.Mensaje ?? "No se pudo actualizar el consultor.";
        return Page();
    }

    private static EditarConsultorViewModel MapToInput(ConsultorViewModel c) => new()
    {
        Id = c.Id,
        Cedula = c.Cedula ?? string.Empty,
        Nombre = c.Nombre,
        Apellido = c.Apellido,
        Email = c.Email,
        Telefono = c.Telefono,
        Celular = c.Celular ?? string.Empty,
        Cargo = c.Cargo ?? string.Empty,
        Rol = c.Rol,
        Tecnologia = c.Tecnologia,
        NivelSeniority = c.NivelSeniority,
        Capacidad = c.Capacidad,
        Empresa = c.Empresa ?? string.Empty,
        Direccion = c.Direccion,
        Barrio = c.Barrio,
        ContactoEmergenciaNombre = c.ContactoEmergenciaNombre,
        ContactoEmergenciaTelefono = c.ContactoEmergenciaTelefono,
        Estado = string.IsNullOrWhiteSpace(c.Estado) ? "Activo" : c.Estado,
        FechaIngreso = c.FechaIngreso,
        FechaNacimiento = c.FechaNacimiento,
        Observaciones = c.Observaciones,
        FotoUrl = c.FotoUrl,
        Habilitado = c.Habilitado,
        CelulasIds = c.CelulasIds
    };

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
