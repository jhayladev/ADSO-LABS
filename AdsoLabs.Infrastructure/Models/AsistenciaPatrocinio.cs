namespace AdsoLabs.Infrastructure.Models;

public class AsistenciaPatrocinio
{
    public int IdAsistenciaPatrocinio { get; set; }
    public int IdPatrocinio { get; set; }
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = "Presente"; // Presente | Ausente | Justificado
    public string? Observacion { get; set; }
    public DateTime FechaRegistro { get; set; }

    public Patrocinio Patrocinio { get; set; } = null!;
}
