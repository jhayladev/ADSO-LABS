using AdsoLabs.Application.DTOs.Patrocinio;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IPatrocinioService
{
    Task<int> AgregarPatrocinioAsync(AgregarPatrocinioCommand command);
    Task ActualizarPatrocinioAsync(ActualizarPatrocinioCommand command);
    Task EliminarPatrocinioAsync(int idPatrocinio);
    Task RegistrarAsistenciasAsync(RegistrarAsistenciasCommand command);
    Task AsignarHorarioAsync(AsignarHorarioCommand command);
    Task EliminarHorarioAsync(int idPatrocinio);
}
