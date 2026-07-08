namespace AdsoLabs.Infrastructure.Models;

public class SesionMonitoria
{
    public int IdSesionMonitoria { get; set; }
    public int IdMonitor { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Jornada { get; set; } = string.Empty;   // Mañana | Tarde
    public string Modalidad { get; set; } = string.Empty; // Presencial | Virtual
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string Estado { get; set; } = "Activa"; // Activa | Finalizada | Cancelada
    public DateTime FechaCreacion { get; set; }

    // Navegación
    public MonitorPerfil Monitor { get; set; } = null!;
    public ICollection<InscripcionMonitoria> Inscripciones { get; set; } = [];
    public ICollection<AsistenciaMonitoria> Asistencias { get; set; } = [];
    public ICollection<InformeMonitoria> Informes { get; set; } = [];
}
