using AdsoLabs.Application.DTOs.PrediccionIA;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IPrediccionIAService
{
    Task<CompatibilidadResponseDTO> ObtenerCompatibilidadAsync(int idUsuario, CancellationToken ct = default);
    Task<List<CompetenciaAsignadaDTO>> ObtenerCompetenciasAsignadasAsync(int idUsuario, CancellationToken ct = default);
    Task<PrediccionAprendizajeDTO> ObtenerPrediccionAprendizajeAsync(int idUsuario, int idFichaCompetencia, bool forzarActualizacion = false, CancellationToken ct = default);
    Task EnviarSolicitudAsync(EnviarSolicitudCommand command, CancellationToken ct = default);
    Task<List<SolicitudAsignacionDTO>> ObtenerSolicitudesAsync(CancellationToken ct = default);
    Task ResponderSolicitudAsync(ResponderSolicitudCommand command, CancellationToken ct = default);
}
