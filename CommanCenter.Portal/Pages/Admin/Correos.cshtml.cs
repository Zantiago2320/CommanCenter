using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.Admin;

[Authorize(Roles = "Admin,Senior")]
public class CorreosModel : PageModel
{
    private readonly IApiClient _api;
    private readonly ILogger<CorreosModel> _logger;

    public CorreosModel(IApiClient api, ILogger<CorreosModel> logger)
    {
        _api = api;
        _logger = logger;
    }

    public string? Error { get; set; }
    public string? Success { get; set; }

    public PreviewCorreoViewModel? Cumpleanios { get; set; }
    public PreviewCorreoViewModel? Reporte { get; set; }

    public async Task OnGetAsync()
    {
        if (TempData["Success"] is string ok) Success = ok;
        if (TempData["Error"] is string err) Error = err;
        await CargarPreviewsAsync();
    }

    public async Task<IActionResult> OnPostCumpleaniosAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>("api/notifications/cumpleanios-mes", new { }, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Recordatorio de cumpleaños del mes enviado manualmente.");
            TempData["Success"] = result.Mensaje ?? "Recordatorio de cumpleaños enviado correctamente.";
        }
        else
        {
            TempData["Error"] = result?.Mensaje ?? "No se pudo enviar el recordatorio de cumpleaños.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReporteAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>("api/notifications/reporte-mensual", new { }, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Reporte mensual enviado manualmente.");
            TempData["Success"] = result.Mensaje ?? "Reporte mensual enviado correctamente.";
        }
        else
        {
            TempData["Error"] = result?.Mensaje ?? "No se pudo enviar el reporte mensual.";
        }

        return RedirectToPage();
    }

    private async Task CargarPreviewsAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");

        var cumple = await _api.GetAsync<PreviewCorreoViewModel>("api/notifications/cumpleanios-mes/preview", token);
        if (cumple?.Exitoso == true && cumple.Data is not null)
            Cumpleanios = cumple.Data;

        var reporte = await _api.GetAsync<PreviewCorreoViewModel>("api/notifications/reporte-mensual/preview", token);
        if (reporte?.Exitoso == true && reporte.Data is not null)
            Reporte = reporte.Data;
    }
}
