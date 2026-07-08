using AdsoLabs.Application.DTOs.Monitoria;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IMonitoriaQueryService
{
    Task<EstadoMonitorDTO> ObtenerEstadoMonitorAsync(int idUsuario);
    Task<List<MonitorDTO>> ObtenerMonitoresAsync();
    Task<List<SesionMonitoriaDTO>> ObtenerMonitoriasVigentesAsync(int idUsuario);
    Task<List<SesionMonitoriaDTO>> ObtenerMisSesionesAsync(int idUsuario);
    Task<List<InscripcionMonitoriaDTO>> ObtenerInscritosAsync(int idSesionMonitoria);
    Task<List<AsistenciaMonitoriaDTO>> ObtenerAsistenciasRecibidasAsync(int? idInstructor);
    Task<List<InformeMonitoriaDTO>> ObtenerInformesRecibidosAsync(int? idInstructor);
}
