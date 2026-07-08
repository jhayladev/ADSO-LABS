using Microsoft.AspNetCore.SignalR;

namespace AdsoLabs.API.Hubs;

public class NotificacionHub : Hub
{
    // El cliente llama a este método tras conectar para unirse a su grupo personal.
    // El IdUsuario proviene del SesionService del componente Blazor.
    public Task RegistrarConexion(int idUsuario)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"user-{idUsuario}");
}
