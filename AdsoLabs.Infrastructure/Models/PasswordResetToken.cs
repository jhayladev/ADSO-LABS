namespace AdsoLabs.Infrastructure.Models;

public class PasswordResetToken
{
    public int IdToken { get; set; }
    public int IdUsuario { get; set; }
    public byte[] TokenHash { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public DateTime? FechaUso { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
}
