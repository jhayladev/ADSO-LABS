namespace AdsoLabs.Application.DTOs.Fichas;

/// <summary>
/// Comando para programar un resultado de aprendizaje dentro de una ficha.
/// La ficha se identifica por <see cref="NumeroFicha"/> y la competencia por
/// <see cref="IdCompetencia"/>; el backend resuelve internamente el IdFichaCompetencia.
/// </summary>
public class ProgramarResultadoDTO
{
    public string   NumeroFicha      { get; set; } = "";
    public int      IdCompetencia    { get; set; }
    public int      IdResultado      { get; set; }
    public int      IdInstructor     { get; set; }
    public DateOnly FechaInicio      { get; set; }
    public DateOnly FechaFin         { get; set; }
    public TimeOnly HoraInicio       { get; set; }
    public TimeOnly HoraFin          { get; set; }
    public int      HorasProgramadas { get; set; }
}
