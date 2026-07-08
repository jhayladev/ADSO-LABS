using AdsoLabs.Application.DTOs.Monitoria;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Monitoria;

public class MonitoriaAppService(
    IMonitoriaQueryService query,
    IMonitoriaService      service)
{
    public Task<EstadoMonitorDTO> ObtenerEstadoMonitorAsync(int idUsuario)
        => query.ObtenerEstadoMonitorAsync(idUsuario);

    public Task<List<MonitorDTO>> ObtenerMonitoresAsync()
        => query.ObtenerMonitoresAsync();

    public Task<List<SesionMonitoriaDTO>> ObtenerMonitoriasVigentesAsync(int idUsuario)
        => query.ObtenerMonitoriasVigentesAsync(idUsuario);

    public Task<List<SesionMonitoriaDTO>> ObtenerMisSesionesAsync(int idUsuario)
        => query.ObtenerMisSesionesAsync(idUsuario);

    public Task<List<InscripcionMonitoriaDTO>> ObtenerInscritosAsync(int idSesionMonitoria)
        => query.ObtenerInscritosAsync(idSesionMonitoria);

    public Task<List<AsistenciaMonitoriaDTO>> ObtenerAsistenciasRecibidasAsync(int? idInstructor)
        => query.ObtenerAsistenciasRecibidasAsync(idInstructor);

    public Task<List<InformeMonitoriaDTO>> ObtenerInformesRecibidosAsync(int? idInstructor)
        => query.ObtenerInformesRecibidosAsync(idInstructor);

    public Task<int> AsignarMonitorAsync(AsignarMonitorCommand command)
        => service.AsignarMonitorAsync(command);

    public Task DesactivarMonitorAsync(int idAprendiz)
        => service.DesactivarMonitorAsync(idAprendiz);

    public Task<int> CrearSesionAsync(CrearSesionMonitoriaCommand command)
        => service.CrearSesionAsync(command);

    public Task CancelarSesionAsync(int idSesionMonitoria, int idUsuarioMonitor)
        => service.CancelarSesionAsync(idSesionMonitoria, idUsuarioMonitor);

    public Task<int> InscribirseAsync(InscribirseMonitoriaCommand command)
        => service.InscribirseAsync(command);

    public Task DesinscribirseAsync(int idInscripcion, int idUsuario)
        => service.DesinscribirseAsync(idInscripcion, idUsuario);

    public Task RegistrarAsistenciaAsync(RegistrarAsistenciaMonitoriaCommand command)
        => service.RegistrarAsistenciaAsync(command);

    public Task RegistrarInformeAsync(RegistrarInformeMonitoriaCommand command)
        => service.RegistrarInformeAsync(command);
}
