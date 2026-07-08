using AdsoLabs.Core.Entities;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<bool> ExisteTokenEnCooldownAsync(int idUsuario, DateTime limite);
    Task<bool> ExisteTokenValidoAsync(string tokenHash);
    Task InvalidarTokensPendientesAsync(int idUsuario);
    Task<PasswordResetToken?> ObtenerPorHashValidoAsync(string tokenHash);
    Task AgregarAsync(PasswordResetToken token);
    Task MarcarComoUsadoAsync(int idToken);
}
