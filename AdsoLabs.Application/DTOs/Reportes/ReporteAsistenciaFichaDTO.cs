namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Reporte B — Asistencia por ficha.
/// Cabecera con el número de ficha y una fila por aprendiz EN FORMACION.
/// </summary>
public class ReporteAsistenciaFichaDTO
{
    public string                              NumeroFicha { get; set; } = string.Empty;
    public List<ReporteAsistenciaAprendizFila> Aprendices  { get; set; } = [];
}

public class ReporteAsistenciaAprendizFila
{
    public string NombreCompleto        { get; set; } = string.Empty;
    public string NumeroDocumento       { get; set; } = string.Empty;
    public string Estado                { get; set; } = string.Empty;
    public int    TotalSesiones         { get; set; }
    public int    Presentes             { get; set; }
    public int    Justificados          { get; set; }
    public int    Ausentes              { get; set; }
    public double PorcentajeAsistencia  { get; set; }
    public bool   EnRiesgo              { get; set; }
}
