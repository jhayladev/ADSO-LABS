using AdsoLabs.Application.DTOs.Aprendiz;

namespace AdsoLabs.Web.Services.Api;

public class ActivacionCuentaApiService(HttpClient httpClient)
{
    public async Task<ActivarUsuarioResultadoDTO> SolicitarActivacionAsync(ActivarUsuarioSolicitudDTO peticion)
    {
        var response = await httpClient.PostAsJsonAsync("api/solicitar", peticion);
        return await response.Content.ReadFromJsonAsync<ActivarUsuarioResultadoDTO>() ?? new();
    }

    public async Task<ActivarUsuarioResultadoDTO> ValidarTokenActivacionAsync(ActivarUsuarioTokenDTO peticion)
    {
        var response = await httpClient.GetAsync($"api/validar?token={peticion.Token}");
        return await response.Content.ReadFromJsonAsync<ActivarUsuarioResultadoDTO>() ?? new();
    }

    public async Task<ActivarUsuarioResultadoDTO> ActivarUsuarioAsync(ActivarUsuarioTokenDTO peticion)
    {
        var response = await httpClient.PostAsJsonAsync("api/activar", peticion);
        return await response.Content.ReadFromJsonAsync<ActivarUsuarioResultadoDTO>() ?? new();
    }
}
