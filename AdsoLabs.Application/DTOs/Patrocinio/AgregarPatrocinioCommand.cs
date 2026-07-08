namespace AdsoLabs.Application.DTOs.Patrocinio;

public class AgregarPatrocinioCommand
{
    public int IdAprendiz { get; set; }
    public int IdFicha { get; set; }
    public string Etapa { get; set; } = "Lectiva";
    public DateOnly? FechaInicioEtapa { get; set; }
    public DateOnly? FechaFinEtapa { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? ContactoEmpresa { get; set; }
}
