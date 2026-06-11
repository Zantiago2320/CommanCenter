using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize]
public class GestionarCelulaModel : PageModel
{
    private readonly IApiClient _api;
    private readonly ILogger<GestionarCelulaModel> _logger;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public CelulaViewModel? Celula { get; set; }

    /// <summary>Consultores que aún NO son miembros de esta célula (para el desplegable de agregar).</summary>
    public List<ConsultorViewModel> ConsultoresDisponibles { get; set; } = [];

    public string? Error { get; set; }
    public string? Success { get; set; }

    public GestionarCelulaModel(IApiClient api, ILogger<GestionarCelulaModel> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (TempData["Success"] is string ok) Success = ok;
        if (TempData["Error"] is string err) Error = err;
        await CargarAsync();

        if (Celula is null)
            return RedirectToPage("/DataTeam/Celulas");

        return Page();
    }

    public async Task<IActionResult> OnPostAgregarMiembroAsync(int consultorId)
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>($"api/celulas/{Id}/miembros/{consultorId}", new { }, token);

        TempData[result?.Exitoso == true ? "Success" : "Error"] =
            result?.Exitoso == true ? "Miembro agregado a la célula." : result?.Mensaje ?? "No se pudo agregar el miembro.";

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRemoverMiembroAsync(int consultorId)
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.DeleteAsync<bool>($"api/celulas/{Id}/miembros/{consultorId}", token);

        TempData[result?.Exitoso == true ? "Success" : "Error"] =
            result?.Exitoso == true ? "Miembro removido de la célula." : result?.Mensaje ?? "No se pudo remover el miembro.";

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostAsignarLiderAsync(int consultorId)
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>($"api/celulas/{Id}/lider/{consultorId}", new { }, token);

        TempData[result?.Exitoso == true ? "Success" : "Error"] =
            result?.Exitoso == true ? "Líder asignado correctamente." : result?.Mensaje ?? "No se pudo asignar el líder.";

        return RedirectToPage(new { id = Id });
    }

    private async Task CargarAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");

        var celula = await _api.GetAsync<CelulaViewModel>($"api/celulas/{Id}", token);
        if (celula?.Exitoso == true && celula.Data is not null)
            Celula = celula.Data;
        else
        {
            Error ??= celula?.Mensaje ?? "No se pudo cargar la célula.";
            return;
        }

        // Consultores que no son miembros aún
        var consultores = await _api.GetAsync<List<ConsultorViewModel>>("api/consultores", token);
        if (consultores?.Exitoso == true && consultores.Data is not null)
        {
            var idsMiembros = Celula.Miembros.Select(m => m.ConsultorId).ToHashSet();
            ConsultoresDisponibles = consultores.Data
                .Where(c => !idsMiembros.Contains(c.Id))
                .ToList();
        }
    }
}
