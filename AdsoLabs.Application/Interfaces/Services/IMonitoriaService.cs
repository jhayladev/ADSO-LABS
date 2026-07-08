using AdsoLabs.Application.DTOs.Monitoria;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IMonitoriaService
{
    Task<int> AsignarMonitorAsync(AsignarMonitorCommand command);
    Task DesactivarMonitorAsync(int idAprendiz);
    Task<int> CrearSesionAsync(CrearSesionMonitoriaCommand command);
    Task CancelarSesionAsync(int idSesionMonitoria, int idUsuarioMonitor);
    Task<int> InscribirseAsync(InscribirseMonitoriaCommand command);
    Task DesinscribirseAsync(int idInscripcion, int idUsuario);
    Task RegistrarAsistenciaAsync(RegistrarAsistenciaMonitoriaCommand command);
    Task RegistrarInformeAsync(RegistrarInformeMonitoriaCommand command);
}
