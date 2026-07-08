using AdsoLabs.Application.DTOs.Competencia;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface ICompetenciaQueryService
{
    Task<List<CompetenciaDTO>> ObtenerTodosAsync();
    Task<FichaCompetenciaDTO> ObtenerDatosAsync(string nombreCompetencia);
}
