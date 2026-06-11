using CommandCenter.API.Domain.Entities;
using CommandCenter.API.Domain.Interfaces;
using CommandCenter.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommandCenter.API.Infrastructure.Repositories;

public class ConsultorRepository : Repository<Consultor>, IConsultorRepository
{
    public ConsultorRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Consultor>> GetHabilitadosAsync() =>
        await _context.Consultores
            .Include(c => c.Celulas).ThenInclude(cm => cm.Celula)
            .Where(c => c.Habilitado && c.Activo)
            .OrderByDescending(c => c.FechaCreacion)
            .ThenBy(c => c.Apellido).ThenBy(c => c.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Consultor>> GetDeshabilitadosAsync() =>
        await _context.Consultores
            .Include(c => c.Celulas).ThenInclude(cm => cm.Celula)
            .Where(c => !c.Habilitado && c.Activo)
            .OrderByDescending(c => c.FechaDeshabilitacion)
            .ThenBy(c => c.Apellido).ThenBy(c => c.Nombre)
            .ToListAsync();

    public async Task<IEnumerable<Consultor>> GetByCelulaAsync(int celulaId) =>
        await _context.Consultores
            .Include(c => c.Celulas).ThenInclude(cm => cm.Celula)
            .Where(c => c.Celulas.Any(cm => cm.CelulaId == celulaId) && c.Habilitado && c.Activo)
            .OrderByDescending(c => c.FechaCreacion)
            .ThenBy(c => c.Apellido).ThenBy(c => c.Nombre)
            .ToListAsync();

    public async Task<Consultor?> GetByEmailAsync(string email) =>
        await _context.Consultores
            .FirstOrDefaultAsync(c => c.Email == email.ToLower() && c.Activo);

    public async Task<IEnumerable<Consultor>> GetCumpleaniosHoyAsync()
    {
        var hoy = DateTime.UtcNow;
        return await _context.Consultores
            .Where(c => c.FechaNacimiento.HasValue
                     && c.FechaNacimiento.Value.Month == hoy.Month
                     && c.FechaNacimiento.Value.Day == hoy.Day
                     && c.Habilitado && c.Activo)
            .ToListAsync();
    }
}
