using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.DataTeam;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IApiClient _api;

    public List<CelulaViewModel> Celulas { get; set; } = [];
    public int TotalConsultores { get; set; }
    public string? Error { get; set; }

    public IndexModel(IApiClient api) => _api = api;

    public async Task OnGetAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");

        var result = await _api.GetAsync<List<CelulaViewModel>>("api/celulas", token);

        if (result?.Exitoso == true && result.Data is not null)
        {
            Celulas = result.Data;
            TotalConsultores = Celulas.Sum(c => c.TotalMiembros);
        }
        else
        {
            Error = result?.Mensaje ?? "No se pudieron cargar las células.";
        }
    }
}
