using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.Admin;

[Authorize(Roles = "SuperAdmin,Admin")]
public class AuditoriaModel : PageModel
{
    private readonly IApiClient _api;

    public List<AuditoriaLogViewModel> Logs { get; set; } = [];
    public string? Error { get; set; }

    public AuditoriaModel(IApiClient api) => _api = api;

    public async Task OnGetAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.GetAsync<List<AuditoriaLogViewModel>>("api/auditoria?top=200", token);

        if (result?.Exitoso == true && result.Data is not null)
            Logs = result.Data;
        else
            Error = result?.Mensaje ?? "No se pudieron cargar los registros de auditoría.";
    }
}
