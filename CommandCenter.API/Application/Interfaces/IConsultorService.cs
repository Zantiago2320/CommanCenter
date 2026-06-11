using CommandCenter.API.Application.DTOs.Common;
using CommandCenter.API.Application.DTOs.Consultores;

namespace CommandCenter.API.Application.Interfaces;

public interface IConsultorService
{
    Task<ApiResponse<IEnumerable<ConsultorDto>>> GetAllAsync();
    Task<ApiResponse<IEnumerable<ConsultorDto>>> GetDeshabilitadosAsync();
    Task<ApiResponse<ConsultorDto>> GetByIdAsync(int id);
    Task<ApiResponse<ConsultorDto>> CrearAsync(CrearConsultorDto dto, string usuarioId);
    Task<ApiResponse<ConsultorDto>> ActualizarAsync(int id, ActualizarConsultorDto dto, string usuarioId);
    Task<ApiResponse<bool>> DeshabilitarAsync(int id, string motivo, string usuarioId);
    Task<ApiResponse<bool>> RehabilitarAsync(int id, string usuarioId);
    Task<ApiResponse<bool>> EliminarAsync(int id, string usuarioId);
    Task<ApiResponse<IEnumerable<ConsultorDto>>> GetByCelulaAsync(int celulaId);
}
