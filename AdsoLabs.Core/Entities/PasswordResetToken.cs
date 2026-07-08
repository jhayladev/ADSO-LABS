namespace AdsoLabs.Core.Entities;

public class PasswordResetToken
{
    public int IdToken { get; set; }
    public int IdUsuario { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public DateTime? FechaUso { get; set; }
}
