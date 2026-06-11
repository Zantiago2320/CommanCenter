using CommandCenter.API.Domain.Entities;
using CommandCenter.API.Domain.Interfaces;
using CommandCenter.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommandCenter.API.Infrastructure.Repositories;

public class AuditoriaRepository : Repository<AuditoriaLog>, IAuditoriaRepository
{
    public AuditoriaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditoriaLog>> GetByModuloAsync(string modulo) =>
        await _context.AuditoriaLogs
            .Where(a => a.Modulo == modulo)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

    public async Task<IEnumerable<AuditoriaLog>> GetByUsuarioAsync(string usuarioId) =>
        await _context.AuditoriaLogs
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

    public async Task<IEnumerable<AuditoriaLog>> GetByFechaAsync(DateTime desde, DateTime hasta) =>
        await _context.AuditoriaLogs
            .Where(a => a.FechaCreacion >= desde && a.FechaCreacion <= hasta)
            .OrderByDescending(a => a.FechaCreacion)
            .ToListAsync();

    public async Task<IEnumerable<AuditoriaLog>> GetRecientesAsync(int top = 100) =>
        await _context.AuditoriaLogs
            .OrderByDescending(a => a.FechaCreacion)
            .Take(top)
            .ToListAsync();

    public async Task RegistrarAsync(string modulo, string accion, string entidad,
        string? entidadId, string? valorAnterior, string? valorNuevo,
        string? usuarioId, string? usuarioEmail, string? ip,
        bool exitoso = true, string? error = null)
    {
        var log = new AuditoriaLog
        {
            Modulo = modulo,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo,
            UsuarioId = usuarioId,
            UsuarioEmail = usuarioEmail,
            IpAddress = ip,
            Exitoso = exitoso,
            MensajeError = error,
            FechaCreacion = DateTime.UtcNow
        };

        await _context.AuditoriaLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
