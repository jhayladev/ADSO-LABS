namespace AdsoLabs.Application.Interfaces.Services;

public interface INotificacionService
{
    Task NotificarObservacionAgregadaAsync(int idAprendiz, int idUsuarioCreador);
    Task NotificarDatosPersonalesActualizadosAsync(int idAprendiz);
    Task NotificarDatosPersonalesActualizadosPorUsuarioAsync(int idUsuario);
    Task NotificarResultadoProgramadoAsync(string numeroFicha, int idCompetencia, int idResultado, int idInstructor);
}
