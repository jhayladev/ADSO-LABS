using AdsoLabs.Application.DTOs.Notificacion;

namespace AdsoLabs.Application.Interfaces.Services;

public interface INotificacionPusher
{
    Task EnviarAsync(int idUsuario, NotificacionDTO notificacion);
}
