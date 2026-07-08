using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Asistencia;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;

namespace AdsoLabs.Application.AppService.Asistencia;

public class AsistenciaAppService(
    IAsistenciaQueryService query,
    IAsistenciaRepository   repo)
{
    public Task<List<ResultadoProgramadoDTO>> ObtenerResultadosProgramadosAsync(string numeroFicha)
        => query.ObtenerResultadosProgramadosAsync(numeroFicha);

    public Task<ResultadoProgramadoDTO?> ObtenerResultadoProgramadoAsync(int idFichaCompetenciaResultado)
        => query.ObtenerResultadoProgramadoAsync(idFichaCompetenciaResultado);

    public Task<List<SesionDTO>> ObtenerSesionesPorResultadoAsync(int idFichaCompetenciaResultado)
        => query.ObtenerSesionesPorResultadoAsync(idFichaCompetenciaResultado);

    public Task<SesionDTO?> ObtenerInfoSesionAsync(int idSesion)
        => query.ObtenerInfoSesionAsync(idSesion);

    public Task<List<AprendizAsistenciaDTO>> ObtenerAprendicesSesionAsync(int idSesion)
        => query.ObtenerAprendicesSesionAsync(idSesion);

    public Task<MensajeEstadoDTO> CrearSesionAsync(CrearSesionDTO dto)
        => repo.CrearSesionAsync(dto);

    public Task<MensajeEstadoDTO> GuardarAsistenciaAsync(GuardarAsistenciaDTO dto)
        => repo.GuardarAsistenciaAsync(dto);
}
