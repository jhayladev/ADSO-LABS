namespace AdsoLabs.Application.DTOs.Informe;

public class CompetenciaSinCalificarDTO
{
    public int          IdCompetencia        { get; set; }
    public string       NombreCompetencia    { get; set; } = string.Empty;
    /// <summary>
    /// Nombres de los resultados de aprendizaje que tienen juicio "Por Evaluar"
    /// para este aprendiz en la competencia programada.
    /// </summary>
    public List<string> ResultadosPendientes { get; set; } = [];
}
