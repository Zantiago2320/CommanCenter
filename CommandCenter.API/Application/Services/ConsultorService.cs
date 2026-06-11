using CommandCenter.API.Application.DTOs.Common;
using CommandCenter.API.Application.DTOs.Consultores;
using CommandCenter.API.Application.Interfaces;
using CommandCenter.API.Domain.Entities;
using CommandCenter.API.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommandCenter.API.Application.Services;

public class ConsultorService : IConsultorService
{
    private readonly IConsultorRepository _repo;
    private readonly IAuditoriaRepository _auditoria;
    private readonly ILogger<ConsultorService> _logger;

    public ConsultorService(
        IConsultorRepository repo,
        IAuditoriaRepository auditoria,
        ILogger<ConsultorService> logger)
    {
        _repo = repo;
        _auditoria = auditoria;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<ConsultorDto>>> GetAllAsync()
    {
        try
        {
            var consultores = await _repo.GetHabilitadosAsync();
            var dtos = consultores.Select(MapToDto);
            return ApiResponse<IEnumerable<ConsultorDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consultores");
            return ApiResponse<IEnumerable<ConsultorDto>>.Fail("Error interno al obtener consultores.");
        }
    }

    public async Task<ApiResponse<IEnumerable<ConsultorDto>>> GetDeshabilitadosAsync()
    {
        try
        {
            var consultores = await _repo.GetDeshabilitadosAsync();
            var dtos = consultores.Select(MapToDto);
            return ApiResponse<IEnumerable<ConsultorDto>>.Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener consultores deshabilitados");
            return ApiResponse<IEnumerable<ConsultorDto>>.Fail("Error interno al obtener consultores deshabilitados.");
        }
    }

    public async Task<ApiResponse<ConsultorDto>> GetByIdAsync(int id)
    {
        var consultor = await _repo.GetByIdAsync(id);
        if (consultor is null)
            return ApiResponse<ConsultorDto>.NotFound("Consultor");

        return ApiResponse<ConsultorDto>.Ok(MapToDto(consultor));
    }

    public async Task<ApiResponse<ConsultorDto>> CrearAsync(CrearConsultorDto dto, string usuarioId)
    {
        try
        {
            var existente = await _repo.GetByEmailAsync(dto.Email);
            if (existente is not null)
                return ApiResponse<ConsultorDto>.Fail($"Ya existe un consultor con el email {dto.Email}.");

            var consultor = new Consultor
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Telefono = dto.Telefono,
                Cargo = dto.Cargo,
                Tecnologia = dto.Tecnologia,
                NivelSeniority = dto.NivelSeniority,
                FechaIngreso = dto.FechaIngreso,
                FechaNacimiento = dto.FechaNacimiento,
                Observaciones = dto.Observaciones,
                FotoUrl = dto.FotoUrl,
                CreadoPor = usuarioId
            };

            // Asignar a una o varias células (sin duplicados)
            foreach (var celulaId in dto.CelulasIds.Distinct())
            {
                consultor.Celulas.Add(new CelulaMiembro { CelulaId = celulaId });
            }

            var creado = await _repo.AddAsync(consultor);
            await _repo.SaveChangesAsync();

            await _auditoria.RegistrarAsync("DataTeam", "CREATE", "Consultor",
                creado.Id.ToString(), null,
                $"{creado.Nombre} {creado.Apellido} | Células: {(dto.CelulasIds.Count > 0 ? string.Join(",", dto.CelulasIds.Distinct()) : "ninguna")}",
                usuarioId, null, null);

            _logger.LogInformation("Consultor {Id} creado por {Usuario} y asignado a {Total} célula(s)",
                creado.Id, usuarioId, dto.CelulasIds.Distinct().Count());
            return ApiResponse<ConsultorDto>.Ok(MapToDto(creado), "Consultor creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear consultor");
            return ApiResponse<ConsultorDto>.Fail("Error interno al crear el consultor.");
        }
    }

    public async Task<ApiResponse<ConsultorDto>> ActualizarAsync(int id, ActualizarConsultorDto dto, string usuarioId)
    {
        var consultor = await _repo.GetByIdAsync(id);
        if (consultor is null)
            return ApiResponse<ConsultorDto>.NotFound("Consultor");

        var anterior = $"{consultor.Nombre} {consultor.Apellido}";

        consultor.Nombre = dto.Nombre.Trim();
        consultor.Apellido = dto.Apellido.Trim();
        consultor.Email = dto.Email.Trim().ToLower();
        consultor.Telefono = dto.Telefono;
        consultor.Cargo = dto.Cargo;
        consultor.Tecnologia = dto.Tecnologia;
        consultor.NivelSeniority = dto.NivelSeniority;
        consultor.FechaIngreso = dto.FechaIngreso;
        consultor.FechaNacimiento = dto.FechaNacimiento;
        consultor.Habilitado = dto.Habilitado;
        consultor.Observaciones = dto.Observaciones;
        if (!string.IsNullOrWhiteSpace(dto.FotoUrl))
            consultor.FotoUrl = dto.FotoUrl;
        consultor.FechaModificacion = DateTime.UtcNow;
        consultor.ModificadoPor = usuarioId;

        await _repo.UpdateAsync(consultor);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "UPDATE", "Consultor",
            id.ToString(), anterior, $"{consultor.Nombre} {consultor.Apellido}", usuarioId, null, null);

        return ApiResponse<ConsultorDto>.Ok(MapToDto(consultor), "Consultor actualizado exitosamente.");
    }

    public async Task<ApiResponse<bool>> DeshabilitarAsync(int id, string motivo, string usuarioId)
    {
        var consultor = await _repo.GetByIdAsync(id);
        if (consultor is null)
            return ApiResponse<bool>.NotFound("Consultor");

        if (!consultor.Habilitado)
            return ApiResponse<bool>.Fail("El consultor ya se encuentra deshabilitado.");

        consultor.Habilitado = false;
        consultor.MotivoDeshabilitacion = motivo.Trim();
        consultor.FechaDeshabilitacion = DateTime.UtcNow;
        consultor.FechaModificacion = DateTime.UtcNow;
        consultor.ModificadoPor = usuarioId;

        await _repo.UpdateAsync(consultor);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "DISABLE", "Consultor",
            id.ToString(), "Habilitado=true",
            $"Habilitado=false | Motivo: {motivo.Trim()}", usuarioId, null, null);

        _logger.LogWarning("Consultor {Id} deshabilitado por {Usuario}. Motivo: {Motivo}",
            id, usuarioId, motivo.Trim());

        return ApiResponse<bool>.Ok(true, "Consultor deshabilitado.");
    }

    public async Task<ApiResponse<bool>> RehabilitarAsync(int id, string usuarioId)
    {
        var consultor = await _repo.GetByIdAsync(id);
        if (consultor is null)
            return ApiResponse<bool>.NotFound("Consultor");

        if (consultor.Habilitado)
            return ApiResponse<bool>.Fail("El consultor ya se encuentra habilitado.");

        var motivoAnterior = consultor.MotivoDeshabilitacion;
        consultor.Habilitado = true;
        consultor.MotivoDeshabilitacion = null;
        consultor.FechaDeshabilitacion = null;
        consultor.FechaModificacion = DateTime.UtcNow;
        consultor.ModificadoPor = usuarioId;

        await _repo.UpdateAsync(consultor);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "ENABLE", "Consultor",
            id.ToString(), $"Habilitado=false | Motivo: {motivoAnterior}",
            "Habilitado=true", usuarioId, null, null);

        _logger.LogInformation("Consultor {Id} rehabilitado por {Usuario}", id, usuarioId);

        return ApiResponse<bool>.Ok(true, "Consultor rehabilitado.");
    }

    public async Task<ApiResponse<bool>> EliminarAsync(int id, string usuarioId)
    {
        var consultor = await _repo.GetByIdAsync(id);
        if (consultor is null)
            return ApiResponse<bool>.NotFound("Consultor");

        consultor.Activo = false;
        consultor.FechaModificacion = DateTime.UtcNow;
        consultor.ModificadoPor = usuarioId;

        await _repo.UpdateAsync(consultor);
        await _repo.SaveChangesAsync();

        await _auditoria.RegistrarAsync("DataTeam", "DELETE", "Consultor",
            id.ToString(), consultor.Email, null, usuarioId, null, null);

        return ApiResponse<bool>.Ok(true, "Consultor eliminado.");
    }

    public async Task<ApiResponse<IEnumerable<ConsultorDto>>> GetByCelulaAsync(int celulaId)
    {
        var consultores = await _repo.GetByCelulaAsync(celulaId);
        return ApiResponse<IEnumerable<ConsultorDto>>.Ok(consultores.Select(MapToDto));
    }

    private static ConsultorDto MapToDto(Consultor c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Apellido = c.Apellido,
        Email = c.Email,
        Telefono = c.Telefono,
        Cargo = c.Cargo,
        Tecnologia = c.Tecnologia,
        NivelSeniority = c.NivelSeniority,
        FechaIngreso = c.FechaIngreso,
        FechaNacimiento = c.FechaNacimiento,
        Habilitado = c.Habilitado,
        FotoUrl = c.FotoUrl,
        FechaCreacion = c.FechaCreacion,
        MotivoDeshabilitacion = c.MotivoDeshabilitacion,
        FechaDeshabilitacion = c.FechaDeshabilitacion,
        Celulas = c.Celulas.Select(cm => cm.Celula?.Nombre ?? "").Where(n => n != "").ToList(),
        CelulasIds = c.Celulas.Select(cm => cm.CelulaId).ToList()
    };
}
