using AdsoLabs.Application.DTOs.Dashboard;
using AdsoLabs.Application.Interfaces.Queries;

namespace AdsoLabs.Application.AppService.Dashboard;

public class DashboardAppService(IDashboardQueryService query)
{
    public Task<ResumenDashboardDTO?> ObtenerResumenAsync(int idUsuario)
        => query.ObtenerResumenAsync(idUsuario);

    public Task<EstadoCompetenciasDashboardDTO?> ObtenerEstadoCompetenciasAsync(int idUsuario)
        => query.ObtenerEstadoCompetenciasAsync(idUsuario);

    public Task<List<ProgresoFichaDashboardDTO>> ObtenerProgresoFichasAsync(int idUsuario)
        => query.ObtenerProgresoFichasAsync(idUsuario);

    public Task<AlertasDashboardDTO> ObtenerAlertasAsync(int idUsuario)
        => query.ObtenerAlertasAsync(idUsuario);
}
