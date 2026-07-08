using AdsoLabs.Application.DTOs.Auth;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResultadoDto> ValidarCredencialesAsync(LoginPeticionDto peticion);
    bool VerificarPassword(string password, byte[]? hash, byte[]? salt);
    Task<bool> CambiarPasswordPrimerLoginAsync(int idUsuario, string nuevaPassword, bool consentimientoAceptado);
}
