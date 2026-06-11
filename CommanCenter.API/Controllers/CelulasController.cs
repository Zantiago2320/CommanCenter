using CommanCenter.API.Application.Interfaces;
using CommanCenter.API.Application.DTOs.Celulas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommanCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CelulasController : ControllerBase
{
    private readonly ICelulaService _service;

    public CelulasController(ICelulaService service)
    {
        _service = service;
    }

    /// <summary>Obtiene todas las células activas.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    /// <summary>Obtiene una célula con sus miembros.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    /// <summary>Crea una nueva célula.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Crear([FromBody] CrearCelulaDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var result = await _service.CrearAsync(dto, userId);
        return result.Exitoso ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>Actualiza una célula.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarCelulaDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var result = await _service.ActualizarAsync(id, dto, userId);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    /// <summary>Elimina lógicamente una célula.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var result = await _service.EliminarAsync(id, userId);
        return result.Exitoso ? Ok(result) : NotFound(result);
    }

    /// <summary>Asigna un consultor como miembro de una célula.</summary>
    [HttpPost("{celulaId:int}/miembros/{consultorId:int}")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> AsignarMiembro(int celulaId, int consultorId)
    {
        var userId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var result = await _service.AsignarMiembroAsync(celulaId, consultorId, userId);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    /// <summary>Remueve un consultor de una célula.</summary>
    [HttpDelete("{celulaId:int}/miembros/{consultorId:int}")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> RemoverMiembro(int celulaId, int consultorId)
    {
        var userId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var result = await _service.RemoverMiembroAsync(celulaId, consultorId, userId);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }

    /// <summary>Asigna un líder a una célula.</summary>
    [HttpPost("{celulaId:int}/lider/{consultorId:int}")]
    [Authorize(Roles = "Admin,Supervisor")]
    public async Task<IActionResult> AsignarLider(int celulaId, int consultorId)
    {
        var userId = User.FindFirstValue(ClaimTypes.Name) ?? "system";
        var result = await _service.AsignarLiderAsync(celulaId, consultorId, userId);
        return result.Exitoso ? Ok(result) : BadRequest(result);
    }
}
