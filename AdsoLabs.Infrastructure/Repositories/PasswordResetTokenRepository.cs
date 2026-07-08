using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class PasswordResetTokenRepository(AdsoDbContext context) : IPasswordResetTokenRepository
{
    // EF model stores TokenHash as byte[], domain entity uses hex string.
    // We convert between the two representations in this repository.

    private static byte[] ToBytes(string hexHash) => Convert.FromHexString(hexHash);

    public async Task<bool> ExisteTokenEnCooldownAsync(int idUsuario, DateTime limite)
        => await context.PasswordResetToken
            .AnyAsync(t => t.IdUsuario == idUsuario
                        && t.FechaCreacion >= limite
                        && t.FechaUso == null);

    public async Task<bool> ExisteTokenValidoAsync(string tokenHash)
    {
        var hashBytes = ToBytes(tokenHash);
        return await context.PasswordResetToken
            .AnyAsync(t => t.TokenHash == hashBytes
                        && t.FechaUso == null
                        && t.FechaExpiracion > DateTime.UtcNow);
    }

    public async Task InvalidarTokensPendientesAsync(int idUsuario)
    {
        var pendientes = await context.PasswordResetToken
            .Where(t => t.IdUsuario == idUsuario
                     && t.FechaUso == null
                     && t.FechaExpiracion > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in pendientes)
            token.FechaUso = DateTime.UtcNow;

        await context.SaveChangesAsync();
    }

    public async Task<Core.Entities.PasswordResetToken?> ObtenerPorHashValidoAsync(string tokenHash)
    {
        var hashBytes = ToBytes(tokenHash);
        var t = await context.PasswordResetToken
            .FirstOrDefaultAsync(t => t.TokenHash == hashBytes
                                   && t.FechaUso == null
                                   && t.FechaExpiracion > DateTime.UtcNow);
        if (t == null) return null;
        return new Core.Entities.PasswordResetToken
        {
            IdToken = t.IdToken,
            IdUsuario = t.IdUsuario,
            TokenHash = Convert.ToHexString(t.TokenHash),
            FechaCreacion = t.FechaCreacion,
            FechaExpiracion = t.FechaExpiracion,
            FechaUso = t.FechaUso
        };
    }

    public async Task AgregarAsync(Core.Entities.PasswordResetToken domainToken)
    {
        var efToken = new Models.PasswordResetToken
        {
            IdUsuario = domainToken.IdUsuario,
            TokenHash = ToBytes(domainToken.TokenHash),
            FechaCreacion = domainToken.FechaCreacion,
            FechaExpiracion = domainToken.FechaExpiracion,
            FechaUso = domainToken.FechaUso
        };
        context.PasswordResetToken.Add(efToken);
        await context.SaveChangesAsync();
        domainToken.IdToken = efToken.IdToken;
    }

    public async Task MarcarComoUsadoAsync(int idToken)
    {
        var t = await context.PasswordResetToken.FindAsync(idToken);
        if (t == null) return;
        t.FechaUso = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
