using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Fichas;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IProgramacionRepository
{
    Task<MensajeEstadoDTO> ConfigurarFichaCompetenciaAsync(ConfigurarFichaCompetenciaDTO dto);
    Task<MensajeEstadoDTO> ProgramarResultadoAsync(ProgramarResultadoDTO dto);
    Task<MensajeEstadoDTO> EliminarProgramacionResultadoAsync(int idFichaCompetenciaResultado);
}
