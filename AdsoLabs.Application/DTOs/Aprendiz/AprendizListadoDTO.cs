namespace AdsoLabs.Application.DTOs.Aprendiz;

public class AprendizListadoDTO
{
    public int IdAprendiz { get; set; }
    /// <summary>Id de la FichaAprendiz de la que se deriva Estado (la más reciente o la filtrada).</summary>
    public int IdFicha    { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public List<string> Ficha { get; set; } = new List<string>();
    public string IndicadorAsistencia { get; set; } = "Al día";
    public decimal PorcentajeAsistencia { get; set; } = 100m;
}
