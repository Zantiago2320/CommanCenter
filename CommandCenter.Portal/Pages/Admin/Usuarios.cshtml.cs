using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CommanCenter.Portal.Models;
using CommanCenter.Portal.Services;

namespace CommanCenter.Portal.Pages.Admin;

[Authorize(Roles = "SuperAdmin,Admin")]
public class UsuariosModel : PageModel
{
    private readonly IApiClient _api;
    private readonly ILogger<UsuariosModel> _logger;

    public List<UsuarioViewModel> Usuarios { get; set; } = [];
    public List<string> Roles { get; set; } = [];
    public string? Error { get; set; }
    public string? Success { get; set; }

    [BindProperty] public CrearUsuarioInput Input { get; set; } = new();

    public UsuariosModel(IApiClient api, ILogger<UsuariosModel> logger)
    {
        _api = api;
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

        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PostAsync<bool>("api/users",
            new { Usuario = Input.Usuario, Input.Password, Input.Rol }, token);

        if (result?.Exitoso == true)
        {
            _logger.LogInformation("Usuario {Usuario} creado desde el Portal con rol {Rol}", Input.Usuario, Input.Rol);
            TempData["Success"] = $"Usuario '{Input.Usuario}' creado correctamente.";
        }
        else
        {
            TempData["Error"] = result?.Mensaje ?? "No se pudo crear el usuario.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCambiarRolAsync(string id, string rol)
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.PutAsync<bool>($"api/users/{id}/rol", new { Rol = rol }, token);

        TempData[result?.Exitoso == true ? "Success" : "Error"] =
            result?.Exitoso == true ? "Rol actualizado correctamente." : result?.Mensaje ?? "No se pudo actualizar el rol.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(string id)
    {
        var token = HttpContext.Session.GetString("jwt_token");
        var result = await _api.DeleteAsync<bool>($"api/users/{id}", token);

        TempData[result?.Exitoso == true ? "Success" : "Error"] =
            result?.Exitoso == true ? "Usuario eliminado." : result?.Mensaje ?? "No se pudo eliminar el usuario.";

        return RedirectToPage();
    }

    private async Task CargarAsync()
    {
        var token = HttpContext.Session.GetString("jwt_token");

        var usuarios = await _api.GetAsync<List<UsuarioViewModel>>("api/users", token);
        if (usuarios?.Exitoso == true && usuarios.Data is not null)
            Usuarios = usuarios.Data;
        else
            Error ??= usuarios?.Mensaje ?? "No se pudieron cargar los usuarios.";

        var roles = await _api.GetAsync<List<string>>("api/users/roles", token);
        if (roles?.Exitoso == true && roles.Data is not null)
            Roles = roles.Data;
    }
}
