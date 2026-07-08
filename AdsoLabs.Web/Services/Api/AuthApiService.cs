using AdsoLabs.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AdsoLabs.Web.Services.Api;

public class AuthApiService(HttpClient httpClient, IHttpContextAccessor contextAccessor)
{
    public async Task<LoginResultadoDto> LoginAsync(LoginPeticionDto peticion)
    {
        var response = await httpClient.PostAsJsonAsync("api/login", peticion);
        return await response.Content.ReadFromJsonAsync<LoginResultadoDto>() ?? new();
    }

    public async Task LogOutAsync()
    {
        await contextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<CambiarPasswordResultadoDto> CambiarPasswordAsync(CambiarPasswordPrimerLoginDto peticion)
    {
        var response = await httpClient.PostAsJsonAsync("api/cambiar-password-primer-login", peticion);
        return await response.Content.ReadFromJsonAsync<CambiarPasswordResultadoDto>() ?? new();
    }

    public async Task<CambiarPasswordResultadoDto> CambiarPasswordAsync(CambiarPasswordDto peticion)
    {
        var response = await httpClient.PostAsJsonAsync("api/cambiar-password", peticion);
        return await response.Content.ReadFromJsonAsync<CambiarPasswordResultadoDto>() ?? new();
    }

    public async Task<EnviarTokenResultadoDto> EnviarTokenAsync(EnviarTokenDto peticion)
    {
        var response = await httpClient.PostAsJsonAsync("api/enviar-token", peticion);
        return await response.Content.ReadFromJsonAsync<EnviarTokenResultadoDto>() ?? new();
    }

    public async Task<bool> ValidarTokenAsync(string token)
    {
        var response = await httpClient.PostAsync($"api/validar-token?token={token}", null);
        return response.IsSuccessStatusCode;
    }
}
