namespace AdsoLabs.Application.DTOs.Asistencia;

public class CrearSesionDTO
{
    public int      IdFichaCompetenciaResultado { get; set; }
    public DateOnly Fecha                       { get; set; }
    public TimeOnly HoraInicio                  { get; set; }
    public TimeOnly HoraFin                     { get; set; }
}
