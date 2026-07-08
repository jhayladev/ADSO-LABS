namespace AdsoLabs.Infrastructure.Models;

public class AsistenciaMonitoria
{
    public int IdAsistenciaMonitoria { get; set; }
    public int IdSesionMonitoria { get; set; }
    public int IdAprendiz { get; set; }
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty; // Presente | Justificado | Ausente
    public int IdInstructorDestinatario { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Navegación
    public SesionMonitoria SesionMonitoria { get; set; } = null!;
    public AprendizPerfil Aprendiz { get; set; } = null!;
    public InstructorPerfil InstructorDestinatario { get; set; } = null!;
}
