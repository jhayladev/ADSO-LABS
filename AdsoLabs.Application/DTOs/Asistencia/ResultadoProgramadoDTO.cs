namespace AdsoLabs.Application.DTOs.Asistencia;

public class ResultadoProgramadoDTO
{
    public int      IdFichaCompetenciaResultado { get; set; }
    public int      IdFichaCompetencia          { get; set; }
    public string   NombreCompetencia           { get; set; } = "";
    public string   NombreResultado             { get; set; } = "";
    public DateOnly FechaInicio                 { get; set; }
    public DateOnly FechaFin                    { get; set; }
    public TimeOnly HoraInicio                  { get; set; }
    public TimeOnly HoraFin                     { get; set; }
    public int      IdInstructor                { get; set; }
    public string   NombreInstructor            { get; set; } = "";
    public string   EstadoResultado             { get; set; } = ""; // Programado | Completado
    public int?     HorasProgramadas            { get; set; }
    public double   HorasEjecutadas             { get; set; }  // suma de duraciones de sesiones no canceladas
}
