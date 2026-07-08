namespace AdsoLabs.Application.DTOs.Reportes;

/// <summary>
/// Reporte D — Juicios evaluativos por ficha.
/// Una fila por combinación aprendiz × resultado de aprendizaje.
/// </summary>
public class ReporteJuiciosDTO
{
    public string                   NumeroFicha { get; set; } = string.Empty;
    public List<ReporteJuicioFila>  Filas       { get; set; } = [];
}

public class ReporteJuicioFila
{
    public string    NombreCompleto    { get; set; } = string.Empty;
    public string    NumeroDocumento   { get; set; } = string.Empty;
    public string    CodigoCompetencia { get; set; } = string.Empty;
    public string    NombreCompetencia { get; set; } = string.Empty;
    public string    CodigoResultado   { get; set; } = string.Empty;
    public string    NombreResultado   { get; set; } = string.Empty;

    /// <summary>APROBADO | POR EVALUAR — null si no se ha registrado juicio.</summary>
    public string?   Juicio            { get; set; }
    public DateTime? FechaJuicio       { get; set; }
}
