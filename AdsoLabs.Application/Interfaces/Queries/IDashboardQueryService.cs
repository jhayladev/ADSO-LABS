using AdsoLabs.Application.DTOs.Dashboard;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IDashboardQueryService
{
    /// <summary>
    /// Resumen de cabecera del dashboard filtrado por el instructor autenticado.
    /// Devuelve null si el IdUsuario no corresponde a ningún InstructorPerfil.
    /// </summary>
    Task<ResumenDashboardDTO?> ObtenerResumenAsync(int idUsuario);

    /// <summary>
    /// Conteo de competencias por estado para las fichas del instructor.
    /// Devuelve null si el IdUsuario no corresponde a ningún InstructorPerfil.
    /// </summary>
    Task<EstadoCompetenciasDashboardDTO?> ObtenerEstadoCompetenciasAsync(int idUsuario);

    /// <summary>
    /// Horas planeadas vs ejecutadas por ficha activa del instructor.
    /// </summary>
    Task<List<ProgresoFichaDashboardDTO>> ObtenerProgresoFichasAsync(int idUsuario);

    /// <summary>
    /// Alertas clasificadas para las fichas activas del instructor.
    /// </summary>
    Task<AlertasDashboardDTO> ObtenerAlertasAsync(int idUsuario);
}
