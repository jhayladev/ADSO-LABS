using AdsoLabs.Application.DTOs.Auth;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Auth;

public class AuthAppService(IAuthService authService)
{
    public Task<LoginResultadoDto> ValidarCredencialesAsync(LoginPeticionDto peticion)
        => authService.ValidarCredencialesAsync(peticion);

    public Task<bool> CambiarPasswordPrimerLoginAsync(int idUsuario, string nuevaPassword, bool consentimientoAceptado)
        => authService.CambiarPasswordPrimerLoginAsync(idUsuario, nuevaPassword, consentimientoAceptado);
}
