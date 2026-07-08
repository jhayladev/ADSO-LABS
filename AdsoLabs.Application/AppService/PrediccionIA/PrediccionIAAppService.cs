using AdsoLabs.Application.DTOs.PrediccionIA;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.PrediccionIA;

public class PrediccionIAAppService(IPrediccionIAService service)
{
    public Task<CompatibilidadResponseDTO> ObtenerCompatibilidadAsync(int idUsuario, CancellationToken ct = default)
        => service.ObtenerCompatibilidadAsync(idUsuario, ct);

    public Task<List<CompetenciaAsignadaDTO>> ObtenerCompetenciasAsignadasAsync(int idUsuario, CancellationToken ct = default)
        => service.ObtenerCompetenciasAsignadasAsync(idUsuario, ct);

    public Task<PrediccionAprendizajeDTO> ObtenerPrediccionAprendizajeAsync(int idUsuario, int idFichaCompetencia, bool forzarActualizacion = false, CancellationToken ct = default)
        => service.ObtenerPrediccionAprendizajeAsync(idUsuario, idFichaCompetencia, forzarActualizacion, ct);

    public Task EnviarSolicitudAsync(EnviarSolicitudCommand command, CancellationToken ct = default)
        => service.EnviarSolicitudAsync(command, ct);

    public Task<List<SolicitudAsignacionDTO>> ObtenerSolicitudesAsync(CancellationToken ct = default)
        => service.ObtenerSolicitudesAsync(ct);

    public Task ResponderSolicitudAsync(ResponderSolicitudCommand command, CancellationToken ct = default)
        => service.ResponderSolicitudAsync(command, ct);
}
