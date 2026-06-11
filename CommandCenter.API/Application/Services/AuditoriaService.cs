using CommandCenter.API.Application.DTOs.Auditoria;
using CommandCenter.API.Application.DTOs.Common;
using CommandCenter.API.Application.Interfaces;
using CommandCenter.API.Domain.Entities;
using CommandCenter.API.Domain.Interfaces;

namespace CommandCenter.API.Application.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _repo;

    public AuditoriaService(IAuditoriaRepository repo)
    {
        _repo = repo;
    }

    public async Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetRecientesAsync(int top = 100)
    {
        if (top is < 1 or > 500) top = 100;
        var logs = await _repo.GetRecientesAsync(top);
        return ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetByUsuarioAsync(string usuarioId)
    {
        var logs = await _repo.GetByUsuarioAsync(usuarioId);
        return ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetByModuloAsync(string modulo)
    {
        var logs = await _repo.GetByModuloAsync(modulo);
        return ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto));
    }

    public async Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetByFechaAsync(DateTime desde, DateTime hasta)
    {
        var logs = await _repo.GetByFechaAsync(desde, hasta);
        return ApiResponse<IEnumerable<AuditoriaLogDto>>.Ok(logs.Select(MapToDto));
    }

    private static AuditoriaLogDto MapToDto(AuditoriaLog a) => new()
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
