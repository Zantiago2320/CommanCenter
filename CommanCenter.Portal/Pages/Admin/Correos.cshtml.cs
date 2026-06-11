using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    public void OnGet() { }

    public async Task<IActionResult> OnPostCumpleaniosAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>("api/notifications/cumpleanios-mes", new { }, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Recordatorio de cumpleaños del mes enviado manualmente.");
            Success = result.Mensaje ?? "Recordatorio de cumpleaños enviado correctamente.";
        }
        else
        {
            Error = result?.Mensaje ?? "No se pudo enviar el recordatorio de cumpleaños.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostReporteAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>("api/notifications/reporte-mensual", new { }, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Reporte mensual enviado manualmente.");
            Success = result.Mensaje ?? "Reporte mensual enviado correctamente.";
        }
        else
        {
            Error = result?.Mensaje ?? "No se pudo enviar el reporte mensual.";
        }

        return Page();
    }
}
