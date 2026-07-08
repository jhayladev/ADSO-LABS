namespace AdsoLabs.Application.DTOs.Monitoria;

public class RegistrarInformeMonitoriaCommand
{
    public int IdSesionMonitoria { get; set; }
    public int IdUsuarioMonitor { get; set; }
    public int IdInstructorDestinatario { get; set; }
    public string Texto { get; set; } = string.Empty;
    public List<int> IdsAprendices { get; set; } = [];
}
