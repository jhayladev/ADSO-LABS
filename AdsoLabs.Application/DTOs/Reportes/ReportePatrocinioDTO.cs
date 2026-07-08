namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Reporte H — Patrocinio.
/// Una fila por aprendiz patrocinado activo, con datos de empresa,
/// etapa y porcentaje de asistencia.
/// </summary>
public class ReportePatrocinioDTO
{
    public string    NombreCompleto      { get; set; } = string.Empty;
    public string    NumeroDocumento     { get; set; } = string.Empty;
    public string    NumeroFicha         { get; set; } = string.Empty;
    public string    NombreEmpresa       { get; set; } = string.Empty;
    public string?   ContactoEmpresa     { get; set; }
    public string    Etapa               { get; set; } = string.Empty;
    public DateOnly? FechaInicioEtapa    { get; set; }
    public DateOnly? FechaFinEtapa       { get; set; }
    public double    PorcentajeAsistencia{ get; set; }
}
