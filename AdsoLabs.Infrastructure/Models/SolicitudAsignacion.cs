namespace AdsoLabs.Infrastructure.Models;

public class SolicitudAsignacion
{
    public int IdSolicitud { get; set; }
    public int IdInstructor { get; set; }
    public int IdFichaCompetencia { get; set; }
    public string Estado { get; set; } = "Pendiente";   // Pendiente | Aprobada | Rechazada
    public string RazonamientoIA { get; set; } = null!;
    public int PorcentajeCompatibilidad { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public string? ObservacionAdmin { get; set; }

    // Navegación
    public InstructorPerfil Instructor { get; set; } = null!;
    public FichaCompetencia FichaCompetencia { get; set; } = null!;
}
