using CommandCenter.API.Application.DTOs.Common;
using CommandCenter.API.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommandCenter.API.Controllers;

/// <summary>
/// Consulta de la auditoría: quién hizo cada cambio, cuándo y qué cambió.
/// Solo accesible para SuperAdmin/Admin.
/// </summary>
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "SuperAdmin,Admin")]
[Produces("application/json")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaRepository _repo;

    public AuditoriaController(IAuditoriaRepository repo)
    {
        _repo = repo;
    }

    /// <summary>Obtiene los registros de auditoría más recientes.</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecientes([FromQuery] int top = 100)
    {
        if (top is < 1 or > 500) top = 100;
        var logs = await _repo.GetRecientesAsync(top);
        return Ok(ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto)));
    }

    /// <summary>Obtiene la auditoría de un usuario concreto.</summary>
    [HttpGet("usuario/{usuario}")]
    public async Task<IActionResult> GetByUsuario(string usuario)
    {
        var logs = await _repo.GetByUsuarioAsync(usuario);
        return Ok(ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto)));
    }

    /// <summary>Obtiene la auditoría de un módulo (ej. DataTeam, Auth).</summary>
    [HttpGet("modulo/{modulo}")]
    public async Task<IActionResult> GetByModulo(string modulo)
    {
        var logs = await _repo.GetByModuloAsync(modulo);
        return Ok(ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto)));
    }

    private static AuditoriaLogDto MapToDto(Domain.Entities.AuditoriaLog a) => new()
    {
        Id = a.Id,
        Fecha = a.FechaCreacion,
        Usuario = a.UsuarioId,
        Modulo = a.Modulo,
        Accion = a.Accion,
        Entidad = a.Entidad,
        EntidadId = a.EntidadId,
        ValorAnterior = a.ValorAnterior,
        ValorNuevo = a.ValorNuevo,
        Exitoso = a.Exitoso,
        Error = a.MensajeError
    };
}

public class AuditoriaLogDto
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? Usuario { get; set; }
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public bool Exitoso { get; set; }
    public string? Error { get; set; }
}
