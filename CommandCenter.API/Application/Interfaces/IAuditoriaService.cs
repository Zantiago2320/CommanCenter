using CommandCenter.API.Application.DTOs.Auditoria;
using CommandCenter.API.Application.DTOs.Common;

namespace CommandCenter.API.Application.Interfaces;

public interface IAuditoriaService
{
    Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetRecientesAsync(int top = 100);
    Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetByUsuarioAsync(string usuarioId);
    Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetByModuloAsync(string modulo);
    Task<ApiResponse<IEnumerable<AuditoriaLogDto>>> GetByFechaAsync(DateTime desde, DateTime hasta);
}
