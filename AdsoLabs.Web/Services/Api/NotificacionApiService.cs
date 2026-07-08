using AdsoLabs.Application.DTOs.Notificacion;
using System.Net.Http.Json;

namespace AdsoLabs.Web.Services.Api;

// idUsuario ya no se manda: se deriva del token interno (ver InternalTokenHandler).
public class NotificacionApiService(HttpClient http)
{
    public Task<List<NotificacionDTO>?> ObtenerBandejaAsync(int idUsuario)
        => http.GetFromJsonAsync<List<NotificacionDTO>>("api/notificaciones");

    public Task<int> ContarNoLeidasAsync(int idUsuario)
        => http.GetFromJsonAsync<int>("api/notificaciones/no-leidas");

    public Task MarcarLeidaAsync(int idNotificacion, int idUsuario)
        => http.PutAsync($"api/notificaciones/{idNotificacion}/leida", null);

    public Task MarcarTodasLeidasAsync(int idUsuario)
        => http.PutAsync("api/notificaciones/marcar-todas-leidas", null);
}
