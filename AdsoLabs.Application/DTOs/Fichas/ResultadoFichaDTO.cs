namespace AdsoLabs.Application.DTOs.Fichas;

public class ResultadoFichaDTO
{
    public int    IdFichaCompetenciaResultado { get; set; }
    public int    IdResultado                 { get; set; }
    public string Codigo                      { get; set; } = "";
    public string Nombre                      { get; set; } = "";
    public int?   IdInstructor                { get; set; }
    public string NombreInstructor            { get; set; } = "";
    public DateOnly? FechaInicio              { get; set; }
    public DateOnly? FechaFin                 { get; set; }
    public TimeOnly? HoraInicio               { get; set; }
    public TimeOnly? HoraFin                  { get; set; }
    public int?   HorasProgramadas            { get; set; }
    public string Estado                      { get; set; } = "Pendiente";

    // ── Juicios evaluativos por aprendiz ─────────────────────────────────────
    /// <summary>
    /// Lista de todos los aprendices activos (EN FORMACION) con su juicio actual.
    /// Aprendices sin JuicioResultado en BD aparecen como "POR EVALUAR".
    /// </summary>
    public List<JuicioAprendizDTO> Juicios          { get; set; } = [];
    public int                     JuiciosAprobados  { get; set; }
    /// <summary>Total de aprendices EN FORMACION en la ficha (denominador del pill).</summary>
    public int                     TotalJuicios      { get; set; }
    /// <summary>
    /// Cuántos aprendices tienen registro real en JuicioResultado.
    /// El pill solo se muestra cuando este valor es mayor que 0.
    /// </summary>
    public int                     JuiciosCargados   { get; set; }
}
