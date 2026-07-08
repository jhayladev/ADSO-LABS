namespace AdsoLabs.Application.DTOs.Patrocinio;

public class AsignarHorarioCommand
{
    public int       IdPatrocinio { get; set; }
    public TimeOnly? HoraInicio   { get; set; }
    public TimeOnly? HoraFin      { get; set; }
}
