namespace AdsoLabs.Infrastructure.Models;

public class InformeMonitoria
{
    public int IdInforme { get; set; }
    public int IdSesionMonitoria { get; set; }
    public int IdInstructorDestinatario { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }

    // Navegación
    public SesionMonitoria SesionMonitoria { get; set; } = null!;
    public InstructorPerfil InstructorDestinatario { get; set; } = null!;
    public ICollection<InformeMonitoriaAprendiz> Aprendices { get; set; } = [];
}
