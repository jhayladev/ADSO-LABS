namespace AdsoLabs.Application.DTOs.Patrocinio;

public class RegistrarAsistenciasCommand
{
    public int IdPatrocinio { get; set; }
    public List<RegistrarAsistenciaItem> Asistencias { get; set; } = [];
}

public class RegistrarAsistenciaItem
{
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = "Presente";
    public string? Observacion { get; set; }
}
