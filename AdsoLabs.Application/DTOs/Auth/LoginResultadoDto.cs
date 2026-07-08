namespace AdsoLabs.Application.DTOs.Auth;

public class LoginResultadoDto
{
    public bool esExitoso { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public UsuarioSesionDTO? Usuario { get; set; } = null;
}
