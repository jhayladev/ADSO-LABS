using AdsoLabs.Application.DTOs.Reportes;
using AdsoLabs.Application.Interfaces.Queries;

namespace AdsoLabs.Application.AppService.Reportes;

public class ReportesAppService(IReportesQueryService query)
{
    public Task<List<ReporteGlobalFichaDTO>> ObtenerResumenGlobalAsync(int idUsuario)
        => query.ObtenerResumenGlobalAsync(idUsuario);

    public Task<ReporteAsistenciaFichaDTO?> ObtenerAsistenciaFichaAsync(string numeroFicha, int idUsuario)
        => query.ObtenerAsistenciaFichaAsync(numeroFicha, idUsuario);

    public Task<ReporteProgresoCompetenciasDTO?> ObtenerProgresoCompetenciasAsync(string numeroFicha, int idUsuario)
        => query.ObtenerProgresoCompetenciasAsync(numeroFicha, idUsuario);

    public Task<ReporteJuiciosDTO?> ObtenerJuiciosAsync(string numeroFicha, int idUsuario)
        => query.ObtenerJuiciosAsync(numeroFicha, idUsuario);

    public Task<ReporteInstructorDTO?> ObtenerReporteInstructorAsync(int idInstructor, int idUsuario)
        => query.ObtenerReporteInstructorAsync(idInstructor, idUsuario);

    public Task<List<InstructorSelectorDTO>> ObtenerSelectorInstructoresAsync()
        => query.ObtenerSelectorInstructoresAsync();

    public Task<ReporteCompetenciaDTO?> ObtenerReporteCompetenciaAsync(int idCompetencia, int idUsuario)
        => query.ObtenerReporteCompetenciaAsync(idCompetencia, idUsuario);

    public Task<List<ReportePatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario)
        => query.ObtenerPatrociniosAsync(idUsuario);
}
