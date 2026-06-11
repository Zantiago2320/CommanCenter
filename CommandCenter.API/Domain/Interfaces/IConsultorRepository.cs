using CommandCenter.API.Domain.Entities;

namespace CommandCenter.API.Domain.Interfaces;

public interface IConsultorRepository : IRepository<Consultor>
{
    Task<IEnumerable<Consultor>> GetHabilitadosAsync();
    Task<IEnumerable<Consultor>> GetDeshabilitadosAsync();
    Task<IEnumerable<Consultor>> GetByCelulaAsync(int celulaId);
    Task<Consultor?> GetByEmailAsync(string email);
    Task<IEnumerable<Consultor>> GetCumpleaniosHoyAsync();
}
