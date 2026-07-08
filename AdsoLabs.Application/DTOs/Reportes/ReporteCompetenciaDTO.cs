namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Reporte G — Por competencia.
/// En qué fichas aparece, estado, instructor asignado y porcentaje de aprobación.
/// </summary>
public class ReporteCompetenciaDTO
{
    public int    IdCompetencia  { get; set; }
    public string Codigo         { get; set; } = string.Empty;
    public string Nombre         { get; set; } = string.Empty;
    public int    HorasAsignadas { get; set; }
    public int    TotalFichas    { get; set; }

    public List<ReporteCompetenciaEnFichaFila> Fichas { get; set; } = [];
}

public class ReporteCompetenciaEnFichaFila
{
    public string  NumeroFicha         { get; set; } = string.Empty;
    public string  Estado              { get; set; } = string.Empty;
    public string? NombreInstructor    { get; set; }
    public int     HorasPlaneadas      { get; set; }
    public int     HorasEjecutadas     { get; set; }
    public int     AprendicesAprobados { get; set; }
    public int     TotalAprendices     { get; set; }
}
