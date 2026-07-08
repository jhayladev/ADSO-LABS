using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Fichas;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Programacion;

public class ProgramacionAppService(
    IProgramacionQueryService query,
    IProgramacionRepository repo,
    INotificacionService notificacionService)
{
    public Task<List<ResultadoFichaDTO>> ObtenerResultadosPorFichaYCompetenciaAsync(
        string numeroFicha, int idCompetencia)
        => query.ObtenerResultadosPorFichaYCompetenciaAsync(numeroFicha, idCompetencia);

    public Task<List<InstructorSelectDTO>> ObtenerInstructoresActivosAsync()
        => query.ObtenerInstructoresActivosAsync();

    public Task<MensajeEstadoDTO> ConfigurarFichaCompetenciaAsync(ConfigurarFichaCompetenciaDTO dto)
        => repo.ConfigurarFichaCompetenciaAsync(dto);

    public async Task<MensajeEstadoDTO> ProgramarResultadoAsync(ProgramarResultadoDTO dto)
    {
        var resultado = await repo.ProgramarResultadoAsync(dto);
        if (resultado.esExitoso)
            await notificacionService.NotificarResultadoProgramadoAsync(
                dto.NumeroFicha, dto.IdCompetencia, dto.IdResultado, dto.IdInstructor);
        return resultado;
    }

    public Task<MensajeEstadoDTO> EliminarProgramacionResultadoAsync(int idFichaCompetenciaResultado)
        => repo.EliminarProgramacionResultadoAsync(idFichaCompetenciaResultado);
}
