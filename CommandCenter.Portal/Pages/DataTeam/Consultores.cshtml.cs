using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize]
public class ConsultoresModel : PageModel
{
    private readonly IApiClient _api;
    private readonly ILogger<ConsultoresModel> _logger;

    public List<ConsultorViewModel> Consultores { get; set; } = [];
    public List<CelulaViewModel> Celulas { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public int? CelulaFiltro { get; set; }

    public string? Error { get; set; }
    public string? Success { get; set; }

    public ConsultoresModel(IApiClient api, ILogger<ConsultoresModel> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        if (TempData["Success"] is string ok) Success = ok;
        await CargarAsync();
    }

    public async Task<IActionResult> OnPostDeshabilitarAsync(int id, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            TempData["Error"] = "Debe indicar el motivo de deshabilitación.";
            return RedirectToPage();
        }

        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PatchAsync<bool>(
            $"api/consultores/{id}/deshabilitar",
            new { Motivo = motivo },
            token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Consultor {Id} deshabilitado desde el Portal. Motivo: {Motivo}", id, motivo);
            TempData["Success"] = "Consultor deshabilitado correctamente.";
        }
        else
        {
            TempData["Error"] = result?.Mensaje ?? "No se pudo deshabilitar el consultor.";
        }

        return RedirectToPage();
    }

    private async Task CargarAsync()
    {
        if (TempData["Error"] is string err) Error = err;

        var token = HttpContext.Session.GetString("jwt_token");

        var celulasResult = await _api.GetAsync<List<CelulaViewModel>>("api/celulas", token);
        if (celulasResult?.Exitoso == true && celulasResult.Data is not null)
            Celulas = celulasResult.Data;

        var endpoint = CelulaFiltro.HasValue
            ? $"api/consultores/celula/{CelulaFiltro.Value}"
            : "api/consultores";

        var result = await _api.GetAsync<List<ConsultorViewModel>>(endpoint, token);

        if (result?.Exitoso == true && result.Data is not null)
            Consultores = result.Data
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();
        else
            Error ??= result?.Mensaje ?? "No se pudieron cargar los consultores.";
    }
}
