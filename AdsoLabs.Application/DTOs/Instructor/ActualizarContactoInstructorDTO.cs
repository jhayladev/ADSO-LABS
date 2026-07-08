namespace AdsoLabs.Application.DTOs.Instructor;

public class ActualizarContactoInstructorDTO
{
    public int     IdUsuario { get; set; }
    public string  Correo    { get; set; } = string.Empty;
    public string? Telefono  { get; set; }
}
