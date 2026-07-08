namespace AdsoLabs.Application.DTOs.Auth;

public class UsuarioSesionDTO
{
    public int IdUsuario { get; set; }
    public string NombreRol { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool PrimerLogin { get; set; } = false;
    public int IdInstructor { get; set; }
}
