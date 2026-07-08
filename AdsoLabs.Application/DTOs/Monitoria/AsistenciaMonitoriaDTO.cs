namespace AdsoLabs.Application.DTOs.Monitoria;

public class AsistenciaMonitoriaDTO
{
    public int IdAsistenciaMonitoria { get; set; }
    public int IdSesionMonitoria { get; set; }
    public string NombreSesion { get; set; } = string.Empty;
    public int IdAprendiz { get; set; }
    public string NombreAprendiz { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int IdInstructorDestinatario { get; set; }
    public string NombreInstructorDestinatario { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}
