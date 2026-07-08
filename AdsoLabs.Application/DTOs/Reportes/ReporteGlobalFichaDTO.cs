namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Fila del reporte E — Resumen global de fichas.
/// Un elemento por ficha visible para el usuario autenticado.
/// </summary>
public class ReporteGlobalFichaDTO
{
    public string NumeroFicha              { get; set; } = string.Empty;
    public string Estado                   { get; set; } = string.Empty;
    public int    TotalAprendices          { get; set; }
    public int    AprendicesActivos        { get; set; }
    public int    AprendicesEnRiesgo       { get; set; }
    public double PorcentajeAsistencia     { get; set; }
    public int    ProgresoGeneral          { get; set; }
    public int    CompetenciasVista        { get; set; }
    public int    CompetenciasEnCurso      { get; set; }
    public int    CompetenciasPendientes   { get; set; }
}
