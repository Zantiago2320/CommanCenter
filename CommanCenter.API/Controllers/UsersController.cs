using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CommanCenter.API.Application.DTOs.Common;
using CommanCenter.API.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommanCenter.API.Controllers;

/// <summary>
/// Gestión de usuarios del sistema. Solo SuperAdmin/Admin pueden crear usuarios y asignar roles.
/// El inicio de sesión es por NOMBRE de usuario (no por correo).
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAuditoriaRepository _auditoria;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAuditoriaRepository auditoria,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _auditoria = auditoria;
        _logger = logger;
    }

    /// <summary>Lista todos los usuarios con sus roles.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = _userManager.Users.ToList();
        var lista = new List<UsuarioDto>();

        foreach (var u in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(u);
            lista.Add(new UsuarioDto
            {
                Id = u.Id,
                Usuario = u.UserName ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        return Ok(ApiResponse<IEnumerable<UsuarioDto>>.Ok(lista.OrderBy(x => x.Usuario)));
    }

    /// <summary>Lista los roles disponibles para asignar.</summary>
    [HttpGet("roles")]
    public IActionResult GetRoles()
    {
        var roles = _roleManager.Roles.Select(r => r.Name).Where(n => n != null).ToList();
        return Ok(ApiResponse<IEnumerable<string?>>.Ok(roles));
    }

    /// <summary>Crea un nuevo usuario y le asigna un rol.</summary>
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<bool>.Fail("Datos inválidos.",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var ejecutor = User.FindFirstValue(ClaimTypes.Name) ?? "system";

        if (await _userManager.FindByNameAsync(dto.Usuario) is not null)
            return BadRequest(ApiResponse<bool>.Fail($"Ya existe un usuario con el nombre '{dto.Usuario}'."));

        if (!await _roleManager.RoleExistsAsync(dto.Rol))
            return BadRequest(ApiResponse<bool>.Fail($"El rol '{dto.Rol}' no existe."));

        var nuevo = new IdentityUser { UserName = dto.Usuario.Trim(), EmailConfirmed = true };
        var result = await _userManager.CreateAsync(nuevo, dto.Password);

        if (!result.Succeeded)
            return BadRequest(ApiResponse<bool>.Fail("No se pudo crear el usuario.",
                result.Errors.Select(e => e.Description).ToList()));

        await _userManager.AddToRoleAsync(nuevo, dto.Rol);

        await _auditoria.RegistrarAsync("Auth", "CREATE_USER", "Usuario",
            nuevo.Id, null, $"{dto.Usuario} [{dto.Rol}]", ejecutor, ejecutor, null);

        _logger.LogInformation("Usuario {Usuario} creado con rol {Rol} por {Ejecutor}",
            dto.Usuario, dto.Rol, ejecutor);

        return Ok(ApiResponse<bool>.Ok(true, $"Usuario '{dto.Usuario}' creado con rol '{dto.Rol}'."));
    }

    /// <summary>Cambia el rol de un usuario existente.</summary>
    [HttpPut("{id}/rol")]
    public async Task<IActionResult> CambiarRol(string id, [FromBody] CambiarRolDto dto)
    {
        var ejecutor = User.FindFirstValue(ClaimTypes.Name) ?? "system";

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<bool>.Fail("Usuario no encontrado."));

        if (!await _roleManager.RoleExistsAsync(dto.Rol))
            return BadRequest(ApiResponse<bool>.Fail($"El rol '{dto.Rol}' no existe."));

        var rolesActuales = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, rolesActuales);
        await _userManager.AddToRoleAsync(user, dto.Rol);

        await _auditoria.RegistrarAsync("Auth", "CHANGE_ROLE", "Usuario",
            user.Id, string.Join(",", rolesActuales), dto.Rol, ejecutor, ejecutor, null);

        _logger.LogInformation("Rol de {Usuario} cambiado a {Rol} por {Ejecutor}",
            user.UserName, dto.Rol, ejecutor);

        return Ok(ApiResponse<bool>.Ok(true, $"Rol actualizado a '{dto.Rol}'."));
    }

    /// <summary>Elimina un usuario.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(string id)
    {
        var ejecutor = User.FindFirstValue(ClaimTypes.Name) ?? "system";

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<bool>.Fail("Usuario no encontrado."));

        // Evitar que un usuario se elimine a sí mismo
        if (string.Equals(user.UserName, ejecutor, StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<bool>.Fail("No puedes eliminar tu propio usuario."));

        var nombre = user.UserName;
        await _userManager.DeleteAsync(user);

        await _auditoria.RegistrarAsync("Auth", "DELETE_USER", "Usuario",
            id, nombre, null, ejecutor, ejecutor, null);

        _logger.LogWarning("Usuario {Usuario} eliminado por {Ejecutor}", nombre, ejecutor);

        return Ok(ApiResponse<bool>.Ok(true, $"Usuario '{nombre}' eliminado."));
    }
}

public class UsuarioDto
{
    public string Id { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class CrearUsuarioDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [MaxLength(100)]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}

public class CambiarRolDto
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = string.Empty;
}
