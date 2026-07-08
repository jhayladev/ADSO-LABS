namespace AdsoLabs.Infrastructure.Models;

public class InformeMonitoriaAprendiz
{
    public int IdInforme { get; set; }
    public int IdAprendiz { get; set; }

    // Navegación
    public InformeMonitoria Informe { get; set; } = null!;
    public AprendizPerfil Aprendiz { get; set; } = null!;
}
