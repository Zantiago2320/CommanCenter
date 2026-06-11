using CommandCenter.API.Application.DTOs.Celulas;
using CommandCenter.API.Application.DTOs.Common;

namespace CommandCenter.API.Application.Interfaces;

public interface ICelulaService
{
    Task<ApiResponse<IEnumerable<CelulaDto>>> GetAllAsync();
    Task<ApiResponse<CelulaDto>> GetByIdAsync(int id);
    Task<ApiResponse<CelulaDto>> CrearAsync(CrearCelulaDto dto, string usuarioId);
    Task<ApiResponse<CelulaDto>> ActualizarAsync(int id, ActualizarCelulaDto dto, string usuarioId);
    Task<ApiResponse<bool>> EliminarAsync(int id, string usuarioId);
    Task<ApiResponse<bool>> AsignarMiembroAsync(int celulaId, int consultorId, string usuarioId);
    Task<ApiResponse<bool>> RemoverMiembroAsync(int celulaId, int consultorId, string usuarioId);
    Task<ApiResponse<bool>> AsignarLiderAsync(int celulaId, int consultorId, string usuarioId);
}
