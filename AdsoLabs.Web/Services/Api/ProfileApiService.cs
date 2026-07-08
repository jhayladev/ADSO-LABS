using AdsoLabs.Application.DTOs.Perfil;
using System.Net.Http.Json;

namespace AdsoLabs.Web.Services.Api;

// idUsuario ya no se manda: se deriva del token interno (ver InternalTokenHandler).
public class ProfileApiService(HttpClient httpClient)
{
    public async Task<PerfilAprendizDTO?> ObtenerPerfilAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/perfil/yo");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PerfilAprendizDTO>();
    }

    public async Task<bool> ActualizarContactoAsync(int idUsuario, ActualizarContactoDTO dto)
    {
        var response = await httpClient.PutAsJsonAsync("api/perfil/editar-contacto", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<CronogramaAprendizDTO?> ObtenerCronogramaAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/perfil/cronograma");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CronogramaAprendizDTO>();
    }
}
