using AdsoLabs.Application.DTOs.Fichas;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IProgramacionQueryService
{
    /// <summary>
    /// Devuelve los ResultadoAprendizaje de la competencia indicada, enriquecidos con
    /// el estado de programación que tenga esa competencia dentro de la ficha dada.
    /// </summary>
    Task<List<ResultadoFichaDTO>> ObtenerResultadosPorFichaYCompetenciaAsync(
        string numeroFicha, int idCompetencia);

    Task<List<InstructorSelectDTO>> ObtenerInstructoresActivosAsync();
}
