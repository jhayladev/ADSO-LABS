using AdsoLabs.Application.DTOs.Notificacion;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface INotificacionRepository
{
    Task<List<NotificacionDTO>> ObtenerPorUsuarioAsync(int idUsuario, int cantidad = 50);
    Task<int> ContarNoLeidasAsync(int idUsuario);
    Task MarcarLeidaAsync(int idNotificacion, int idUsuario);
    Task MarcarTodasLeidasAsync(int idUsuario);
    Task<int> CrearAsync(int idUsuarioDestinatario, string tipo, string titulo, string mensaje,
        int? referenciaId = null, string? referenciaTipo = null);
}
