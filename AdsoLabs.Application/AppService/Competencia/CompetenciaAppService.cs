using AdsoLabs.Application.DTOs.Competencia;
using AdsoLabs.Application.Interfaces.Queries;

namespace AdsoLabs.Application.AppService.Competencia;

public class CompetenciaAppService(ICompetenciaQueryService competenciaQuery)
{
    // Queries → ICompetenciaQueryService (no hay commands aún)
    public Task<List<CompetenciaDTO>> ObtenerCompetenciasAsync()
        => competenciaQuery.ObtenerTodosAsync();

    public Task<FichaCompetenciaDTO> ObtenerDatosCompetenciasAsync(string nombreCompetencia)
        => competenciaQuery.ObtenerDatosAsync(nombreCompetencia);
}
