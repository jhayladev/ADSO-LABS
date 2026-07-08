namespace AdsoLabs.Application.DTOs.Auth;

public class CambiarPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NuevaPassword { get; set; } = string.Empty;
}
