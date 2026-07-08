using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IRecuperacionPasswordService
{
    Task<ResultadoEnvioToken> EnviarTokenAsync(string correo);
    Task<bool> ValidarTokenAsync(string token);
    Task<bool> CambiarPasswordAsync(string token, string nuevaPassword);
}
