namespace AdsoLabs.Application.DTOs.Monitoria;

public class CrearSesionMonitoriaCommand
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Jornada { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
}
