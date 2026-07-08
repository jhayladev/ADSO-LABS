using AdsoLabs.Application.DTOs.Notificacion;
using AdsoLabs.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace AdsoLabs.API.Hubs;

public class SignalRNotificacionPusher(IHubContext<NotificacionHub> hubContext) : INotificacionPusher
{
    public Task EnviarAsync(int idUsuario, NotificacionDTO notificacion)
        => hubContext.Clients
            .Group($"user-{idUsuario}")
            .SendAsync("RecibirNotificacion", notificacion);
}
