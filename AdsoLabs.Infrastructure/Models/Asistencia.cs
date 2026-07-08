namespace AdsoLabs.Infrastructure.Models;

public class Asistencia
{
    public int IdAsistencia { get; set; }
    public int IdSesion { get; set; }
    public int IdAprendiz { get; set; }
    public string Estado { get; set; } = null!; // Presente | Ausente
    public string? MotivoInasistencia { get; set; }

    // Navegación
    public Sesion Sesion { get; set; } = null!;
    public AprendizPerfil Aprendiz { get; set; } = null!;
}
