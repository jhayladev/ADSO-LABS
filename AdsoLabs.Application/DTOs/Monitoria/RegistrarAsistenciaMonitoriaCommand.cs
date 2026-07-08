namespace AdsoLabs.Application.DTOs.Monitoria;

public class RegistrarAsistenciaMonitoriaCommand
{
    public int IdSesionMonitoria { get; set; }
    public int IdUsuarioMonitor { get; set; }
    public DateOnly Fecha { get; set; }
    public int IdInstructorDestinatario { get; set; }
    public List<AsistenciaMonitoriaItem> Asistencias { get; set; } = [];
}

public class AsistenciaMonitoriaItem
{
    public int IdAprendiz { get; set; }
    public string Estado { get; set; } = string.Empty;
}
