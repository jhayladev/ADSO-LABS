using AdsoLabs.Application.DTOs.Patrocinio;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Patrocinio;

public class PatrocinioAppService(
    IPatrocinioQueryService query,
    IPatrocinioService      service)
{
    public Task<List<PatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario)
        => query.ObtenerPatrociniosAsync(idUsuario);

    public Task<PatrocinioDTO?> ObtenerPatrocinioAsync(int idPatrocinio)
        => query.ObtenerPatrocinioAsync(idPatrocinio);

    public Task<List<AsistenciaPatrocinioDTO>> ObtenerAsistenciasAsync(int idPatrocinio)
        => query.ObtenerAsistenciasAsync(idPatrocinio);

    public Task<int> AgregarPatrocinioAsync(AgregarPatrocinioCommand command)
        => service.AgregarPatrocinioAsync(command);

    public Task ActualizarPatrocinioAsync(ActualizarPatrocinioCommand command)
        => service.ActualizarPatrocinioAsync(command);

    public Task EliminarPatrocinioAsync(int idPatrocinio)
        => service.EliminarPatrocinioAsync(idPatrocinio);

    public Task RegistrarAsistenciasAsync(RegistrarAsistenciasCommand command)
        => service.RegistrarAsistenciasAsync(command);

    public Task<ResumenAsistenciaDTO> ObtenerResumenAsistenciaAsync(int idAprendiz, int idFicha)
        => query.ObtenerResumenAsistenciaAsync(idAprendiz, idFicha);

    public Task AsignarHorarioAsync(AsignarHorarioCommand command)
        => service.AsignarHorarioAsync(command);

    public Task EliminarHorarioAsync(int idPatrocinio)
        => service.EliminarHorarioAsync(idPatrocinio);
}
