using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize(Roles = "SuperAdmin,Admin,Lider")]
public class ConsultoresDeshabilitadosModel : PageModel
{
    private readonly IApiClient _api;
    private readonly ILogger<ConsultoresDeshabilitadosModel> _logger;

    public List<ConsultorViewModel> Consultores { get; set; } = [];
    public string? Error { get; set; }
    public string? Success { get; set; }

    public ConsultoresDeshabilitadosModel(IApiClient api, ILogger<ConsultoresDeshabilitadosModel> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        if (TempData["Success"] is string ok) Success = ok;
        await CargarAsync();
    }

    public async Task<IActionResult> OnPostRehabilitarAsync(int id)
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PatchAsync<bool>(
            $"api/consultores/{id}/rehabilitar",
            new { },
            token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Consultor {Id} rehabilitado desde el Portal.", id);
            TempData["Success"] = "Consultor rehabilitado correctamente.";
        }
        else
        {
            TempData["Error"] = result?.Mensaje ?? "No se pudo rehabilitar el consultor.";
        }

        return RedirectToPage();
    }

    private async Task CargarAsync()
    {
        if (TempData["Error"] is string err) Error = err;

        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.GetAsync<List<ConsultorViewModel>>("api/consultores/deshabilitados", token);

        if (result?.Exitoso == true && result.Data is not null)
            Consultores = result.Data;
        else
            Error ??= result?.Mensaje ?? "No se pudieron cargar los consultores deshabilitados.";
    }
}
