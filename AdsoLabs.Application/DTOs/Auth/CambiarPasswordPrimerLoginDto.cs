namespace AdsoLabs.Application.DTOs.Auth;

public class CambiarPasswordPrimerLoginDto
{
    public int IdUsuario { get; set; }
    public string NuevaPassword { get; set; } = string.Empty;
    public bool ConsentimientoAceptado { get; set; }
}
