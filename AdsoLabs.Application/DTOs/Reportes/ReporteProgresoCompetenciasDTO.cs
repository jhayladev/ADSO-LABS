namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Reporte C — Progreso de competencias por ficha.
/// Una fila por competencia vinculada a la ficha.
/// </summary>
public class ReporteProgresoCompetenciasDTO
{
    public string                               NumeroFicha  { get; set; } = string.Empty;
    public List<ReporteCompetenciaFichaFila>    Competencias { get; set; } = [];
}

public class ReporteCompetenciaFichaFila
{
    public string  CodigoCompetencia  { get; set; } = string.Empty;
    public string  NombreCompetencia  { get; set; } = string.Empty;
    public string  Estado             { get; set; } = string.Empty;
    public string? NombreInstructor   { get; set; }
    public int     HorasPlaneadas     { get; set; }
    public int     HorasEjecutadas    { get; set; }
    public int     PorcentajeProgreso { get; set; }
}
