namespace AdsoLabs.Application.DTOs.Monitoria;

public class SesionMonitoriaDTO
{
    public int IdSesionMonitoria { get; set; }
    public int IdMonitor { get; set; }
    public int IdAprendizMonitor { get; set; }
    public string NombreMonitor { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Jornada { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int TotalInscritos { get; set; }
    public bool InscritoUsuarioActual { get; set; }
    public int? IdInscripcionUsuarioActual { get; set; }
}
