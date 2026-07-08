namespace AdsoLabs.Application.DTOs.PrediccionIA;

public class SolicitudAsignacionDTO
{
    public int IdSolicitud { get; set; }
    public string NombreInstructor { get; set; } = null!;
    public string NombreCompetencia { get; set; } = null!;
    public string NumeroFicha { get; set; } = null!;
    public int PorcentajeCompatibilidad { get; set; }
    public string RazonamientoIA { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public DateTime FechaSolicitud { get; set; }
    public string? ObservacionAdmin { get; set; }
}
