using CommanCenter.API.Application.DTOs.Celulas;
using CommanCenter.API.Application.DTOs.Common;
using CommanCenter.API.Application.Interfaces;
using CommanCenter.API.Domain.Entities;
using CommanCenter.API.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommanCenter.API.Application.Services;

public class CelulaService : ICelulaService
{
    private readonly ICelulaRepository _repo;
    private readonly IAuditoriaRepository _auditoria;
    private readonly ILogger<CelulaService> _logger;

    public CelulaService(ICelulaRepository repo, IAuditoriaRepository auditoria, ILogger<CelulaService> logger)
    {
        _repo = repo;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<CelulaDto>>> GetAllAsync()
    {
        var celulas = await _repo.GetActivasAsync();
        return ApiResponse<IEnumerable<CelulaDto>>.Ok(celulas.Select(MapToDto));
    }

    public async Task<ApiResponse<CelulaDto>> GetByIdAsync(int id)
    {
        var celula = await _repo.GetWithMiembrosAsync(id);
        if (celula is null) return ApiResponse<CelulaDto>.NotFound("Célula");
        return ApiResponse<CelulaDto>.Ok(MapToDto(celula));
    }

    public async Task<ApiResponse<CelulaDto>> CrearAsync(CrearCelulaDto dto, string usuarioId)
    {
        var celula = new Celula
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion,
            Color = dto.Color ?? "#28a745",
            ImagenUrl = dto.ImagenUrl,
            CreadoPor = usuarioId
        };

        var creada = await _repo.AddAsync(celula);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "CREATE", "Celula",
            creada.Id.ToString(), null, creada.Nombre, usuarioId, null, null);

        return ApiResponse<CelulaDto>.Ok(MapToDto(creada), "Célula creada exitosamente.");
    }

    public async Task<ApiResponse<CelulaDto>> ActualizarAsync(int id, ActualizarCelulaDto dto, string usuarioId)
    {
        var celula = await _repo.GetByIdAsync(id);
        if (celula is null) return ApiResponse<CelulaDto>.NotFound("Célula");

        celula.Nombre = dto.Nombre.Trim();
        celula.Descripcion = dto.Descripcion;
        celula.Color = dto.Color;
        celula.ImagenUrl = dto.ImagenUrl;
        celula.FechaModificacion = DateTime.UtcNow;
        celula.ModificadoPor = usuarioId;

        await _repo.UpdateAsync(celula);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "UPDATE", "Celula",
            id.ToString(), null, celula.Nombre, usuarioId, null, null);

        return ApiResponse<CelulaDto>.Ok(MapToDto(celula), "Célula actualizada.");
    }

    public async Task<ApiResponse<bool>> EliminarAsync(int id, string usuarioId)
    {
        var celula = await _repo.GetWithMiembrosAsync(id);
        if (celula is null) return ApiResponse<bool>.NotFound("Célula");

        // Validación 1: No permitir eliminar células especiales
        var celulasSinEliminacion = new[] { "Sin asignación" };
        if (celulasSinEliminacion.Contains(celula.Nombre))
            return ApiResponse<bool>.Fail($"No se puede eliminar la célula '{celula.Nombre}' — es obligatoria en el sistema.");

        // Validación 2: No permitir eliminar si tiene miembros asignados
        if (celula.Miembros.Any())
        {
            var totalMiembros = celula.Miembros.Count;
            return ApiResponse<bool>.Fail(
                $"No se puede eliminar '{celula.Nombre}' — tiene {totalMiembros} miembro(s) asignado(s). " +
                "Reasigne los miembros antes de intentar eliminarla.");
        }

        celula.Activo = false;
        celula.FechaModificacion = DateTime.UtcNow;
        celula.ModificadoPor = usuarioId;

        await _repo.UpdateAsync(celula);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "DELETE", "Celula",
            id.ToString(), celula.Nombre, $"Eliminada por {usuarioId}", usuarioId, null, null);

        _logger.LogInformation("Célula {CelulaId} '{NombreCelula}' eliminada por {Usuario}",
            id, celula.Nombre, usuarioId);

        return ApiResponse<bool>.Ok(true, $"Célula '{celula.Nombre}' eliminada.");
    }

    public async Task<ApiResponse<bool>> AsignarMiembroAsync(int celulaId, int consultorId, string usuarioId)
    {
        var celula = await _repo.GetWithMiembrosAsync(celulaId);
        if (celula is null) return ApiResponse<bool>.NotFound("Célula");

        if (celula.Miembros.Any(m => m.ConsultorId == consultorId))
            return ApiResponse<bool>.Fail("El consultor ya es miembro de esta célula.");

        celula.Miembros.Add(new CelulaMiembro { CelulaId = celulaId, ConsultorId = consultorId });
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "ASSIGN_MEMBER", "Celula",
            celulaId.ToString(), null, $"ConsultorId={consultorId}", usuarioId, null, null);

        return ApiResponse<bool>.Ok(true, "Miembro asignado.");
    }

    public async Task<ApiResponse<bool>> RemoverMiembroAsync(int celulaId, int consultorId, string usuarioId)
    {
        var celula = await _repo.GetWithMiembrosAsync(celulaId);
        if (celula is null) return ApiResponse<bool>.NotFound("Célula");

        var miembro = celula.Miembros.FirstOrDefault(m => m.ConsultorId == consultorId);
        if (miembro is null) return ApiResponse<bool>.Fail("El consultor no es miembro de esta célula.");

        celula.Miembros.Remove(miembro);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "REMOVE_MEMBER", "Celula",
            celulaId.ToString(), $"ConsultorId={consultorId}", null, usuarioId, null, null);

        return ApiResponse<bool>.Ok(true, "Miembro removido.");
    }

    public async Task<ApiResponse<bool>> AsignarLiderAsync(int celulaId, int consultorId, string usuarioId)
    {
        var celula = await _repo.GetWithMiembrosAsync(celulaId);
        if (celula is null) return ApiResponse<bool>.NotFound("Célula");

        if (celula.Lideres.Any(l => l.ConsultorId == consultorId))
            return ApiResponse<bool>.Fail("El consultor ya es líder de esta célula.");

        celula.Lideres.Add(new CelulaLider { CelulaId = celulaId, ConsultorId = consultorId });
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "ASSIGN_LEADER", "Celula",
            celulaId.ToString(), null, $"ConsultorId={consultorId}", usuarioId, null, null);

        return ApiResponse<bool>.Ok(true, "Líder asignado.");
    }

    private static CelulaDto MapToDto(Celula c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Descripcion = c.Descripcion,
        Color = c.Color,
        ImagenUrl = c.ImagenUrl,
        Activo = c.Activo,
        TotalMiembros = c.Miembros.Count,
        NombreLider = c.Lideres.FirstOrDefault()?.Consultor is { } lider
            ? $"{lider.Nombre} {lider.Apellido}" : null,
        FechaCreacion = c.FechaCreacion,
        Miembros = c.Miembros.Select(m => new MiembroCelulaDto
        {
            ConsultorId = m.ConsultorId,
            NombreCompleto = m.Consultor != null ? $"{m.Consultor.Nombre} {m.Consultor.Apellido}" : "",
            Cargo = m.Consultor?.Cargo,
            EsLider = c.Lideres.Any(l => l.ConsultorId == m.ConsultorId)
        }).ToList()
    };
}
