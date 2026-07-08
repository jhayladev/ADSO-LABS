namespace AdsoLabs.Infrastructure.Models;

public class MonitorPerfil
{
    public int IdMonitor { get; set; }
    public int IdAprendiz { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaAsignacion { get; set; }

    // Navegación
    public AprendizPerfil Aprendiz { get; set; } = null!;
    public ICollection<SesionMonitoria> Sesiones { get; set; } = [];
}
