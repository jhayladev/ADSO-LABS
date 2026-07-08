namespace AdsoLabs.Application.DTOs.Monitoria;

public class InformeMonitoriaDTO
{
    public int IdInforme { get; set; }
    public int IdSesionMonitoria { get; set; }
    public string NombreSesion { get; set; } = string.Empty;
    public int IdInstructorDestinatario { get; set; }
    public string NombreInstructorDestinatario { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public List<string> Aprendices { get; set; } = [];
}
