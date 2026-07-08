namespace AdsoLabs.Application.DTOs.Patrocinio;

public class AsistenciaPatrocinioDTO
{
    public int IdAsistenciaPatrocinio { get; set; }
    public int IdPatrocinio { get; set; }
    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
}
