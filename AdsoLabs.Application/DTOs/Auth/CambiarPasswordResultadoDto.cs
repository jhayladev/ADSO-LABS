namespace AdsoLabs.Application.DTOs.Auth;

public class CambiarPasswordResultadoDto
{
    public bool esExitoso { get; set; } = false;
    public string Mensaje { get; set; } = string.Empty;
}
