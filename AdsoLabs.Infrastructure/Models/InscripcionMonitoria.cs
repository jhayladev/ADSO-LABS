namespace AdsoLabs.Infrastructure.Models;

public class InscripcionMonitoria
{
    public int IdInscripcion { get; set; }
    public int IdSesionMonitoria { get; set; }
    public int IdAprendiz { get; set; }
    public DateTime FechaInscripcion { get; set; }

    // Navegación
    public SesionMonitoria SesionMonitoria { get; set; } = null!;
    public AprendizPerfil Aprendiz { get; set; } = null!;
}
