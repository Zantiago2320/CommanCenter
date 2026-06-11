using CommandCenter.API.Domain.Entities;
using CommandCenter.API.Domain.Interfaces;
using CommandCenter.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommandCenter.API.Infrastructure.Repositories;

public class CelulaRepository : Repository<Celula>, ICelulaRepository
{
    public CelulaRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Celula>> GetActivasAsync() =>
        await _context.Celulas
            .Include(c => c.Miembros).ThenInclude(m => m.Consultor)
            .Include(c => c.Lideres).ThenInclude(l => l.Consultor)
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

    public async Task<Celula?> GetWithMiembrosAsync(int celulaId) =>
        await _context.Celulas
            .Include(c => c.Miembros).ThenInclude(m => m.Consultor)
            .Include(c => c.Lideres).ThenInclude(l => l.Consultor)
            .FirstOrDefaultAsync(c => c.Id == celulaId && c.Activo);

    public async Task<IEnumerable<Celula>> GetByLiderAsync(int consultorId) =>
        await _context.Celulas
            .Where(c => c.Lideres.Any(l => l.ConsultorId == consultorId) && c.Activo)
            .ToListAsync();
}
