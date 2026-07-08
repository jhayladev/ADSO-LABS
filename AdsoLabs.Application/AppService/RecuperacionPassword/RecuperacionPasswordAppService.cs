using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.AppService.RecuperacionPassword;

public class RecuperacionPasswordAppService(IRecuperacionPasswordService recuperacionService)
{
    public Task<ResultadoEnvioToken> EnviarTokenAsync(string correo)
        => recuperacionService.EnviarTokenAsync(correo);

    public Task<bool> ValidarTokenAsync(string token)
        => recuperacionService.ValidarTokenAsync(token);

    public Task<bool> CambiarPasswordAsync(string token, string nuevaPassword)
        => recuperacionService.CambiarPasswordAsync(token, nuevaPassword);
}
