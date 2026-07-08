namespace AdsoLabs.Application.DTOs.Patrocinio;

public class PatrocinioDTO
{
    public int IdPatrocinio { get; set; }
    public int IdAprendiz   { get; set; }
    public int IdFicha      { get; set; }
    public string NombreAprendiz { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NumeroFicha { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public string Etapa { get; set; } = string.Empty;
    public DateOnly? FechaInicioEtapa { get; set; }
    public DateOnly? FechaFinEtapa { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? ContactoEmpresa { get; set; }
    public DateTime FechaRegistro { get; set; }
    public double PorcentajeAsistencia { get; set; }
}
