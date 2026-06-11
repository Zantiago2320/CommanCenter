using CommanCenter.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommanCenter.API.Controllers;

/// <summary>
/// Consulta de la auditoría: quién hizo cada cambio, cuándo y qué cambió.
/// Solo accesible para SuperAdmin/Admin.
/// </summary>
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _service;

    public AuditoriaController(IAuditoriaService service)
    {
        _service = service;
    }

    /// <summary>Obtiene los registros de auditoría más recientes.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecientes([FromQuery] int top = 100) =>
        Ok(await _service.GetRecientesAsync(top));

    /// <summary>Obtiene la auditoría de un usuario concreto.</summary>
    [HttpGet("usuario/{usuario}")]
    public async Task<IActionResult> GetByUsuario(string usuario) =>
        Ok(await _service.GetByUsuarioAsync(usuario));

    /// <summary>Obtiene la auditoría de un módulo (ej. DataTeam, Auth).</summary>
    [HttpGet("modulo/{modulo}")]
    public async Task<IActionResult> GetByModulo(string modulo) =>
        Ok(await _service.GetByModuloAsync(modulo));

    /// <summary>Obtiene la auditoría dentro de un rango de fechas.</summary>
    [HttpGet("fecha")]
    public async Task<IActionResult> GetByFecha([FromQuery] DateTime desde, [FromQuery] DateTime hasta) =>
        Ok(await _service.GetByFechaAsync(desde, hasta));
}
