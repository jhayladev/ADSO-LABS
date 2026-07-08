namespace AdsoLabs.Application.DTOs.Monitoria;

public class MonitorDTO
{
    public int IdMonitor { get; set; }
    public int IdAprendiz { get; set; }
    public string NombreAprendiz { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaAsignacion { get; set; }
}
