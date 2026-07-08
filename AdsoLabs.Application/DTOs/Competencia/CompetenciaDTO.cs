namespace AdsoLabs.Application.DTOs.Competencia;

public class CompetenciaDTO
{
    public int    IdCompetencia     { get; set; }
    public string CodigoCompetencia { get; set; } = string.Empty;
    public string NombreCompetencia { get; set; } = string.Empty;
    public int    HorasAsignadas    { get; set; }
    public string TipoCompetencia   { get; set; } = string.Empty;
    public string Estado            { get; set; } = string.Empty;
    /// <summary>Fase del proyecto formativo (Inducción|Análisis|Planeación|Ejecución|Evaluación|Transversal).</summary>
    public string FaseFormativa     { get; set; } = "Transversal";
}
