namespace AdsoLabs.Application.DTOs.Patrocinio;

public class ResumenAsistenciaDTO
{
    public int    TotalSesiones       { get; set; }
    public int    Presentes           { get; set; }
    public int    Ausentes            { get; set; }
    public int    Justificados        { get; set; }
    public double PorcentajeAsistencia { get; set; }
}
