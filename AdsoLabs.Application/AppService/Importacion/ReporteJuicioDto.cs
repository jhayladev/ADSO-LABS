namespace AdsoLabs.Application.AppService.Importacion;

/// <summary>
/// DTO que representa una fila normalizada del Reporte de Juicios Evaluativos de SOFIA Plus.
///
/// La Fase 1 emite TODAS las filas con juicio presente (APROBADO y POR EVALUAR).
/// Solo se descartan filas completamente vacías (sin documento ni competencia) o
/// filas cuyo juicio esté ausente/nulo.
///
/// JuicioEvaluativo puede ser "APROBADO" o "POR EVALUAR".
/// La Fase 2 usa este campo para decidir:
///   • APROBADO    → crea/actualiza el JuicioResultado en BD (Step 10).
///   • POR EVALUAR → solo garantiza la existencia del FCR (Steps 1-6) y
///                   el vínculo aprendiz-ficha (Steps 7-9). Sin juicio en BD.
///
/// Esto es necesario para que RecalcularEstadoFichaCompetenciaAsync conozca
/// el conjunto COMPLETO de RAs de la competencia (totalFcrs correcto).
/// </summary>
public sealed record ReporteJuicioDto
{
    // ── Metadatos de la ficha (del encabezado del Excel) ─────────────────────
    public required string    NumeroFicha    { get; init; }
    public required string    Denominacion   { get; init; }
    public required string    EstadoFicha    { get; init; }
    public           DateOnly? FechaInicio   { get; init; }
    public           DateOnly? FechaFin      { get; init; }
    public required string    Modalidad      { get; init; }
    public required string    Regional       { get; init; }
    public required string    Centro         { get; init; }

    // ── Datos del aprendiz ────────────────────────────────────────────────────
    public required string    TipoDocumento  { get; init; }
    public required string    Documento      { get; init; }
    public required string    Nombres        { get; init; }
    public required string    Apellidos      { get; init; }
    public required string    EstadoAprendiz { get; init; }

    // ── Competencia y resultado ───────────────────────────────────────────────
    // NombreCompetencia y NombreResultado son necesarios para crear nuevos registros
    // en BD cuando la competencia/resultado no existían previamente.
    public required string    CodigoCompetencia { get; init; }
    public required string    NombreCompetencia { get; init; }
    public required string    CodigoResultado   { get; init; }
    public required string    NombreResultado   { get; init; }

    // ── Juicio ───────────────────────────────────────────────────────────────
    // Puede ser "APROBADO" o "POR EVALUAR".
    // Solo las filas con APROBADO generan un JuicioResultado en BD (Fase 2, Step 10).
    // Las filas POR EVALUAR se usan únicamente para garantizar la existencia del FCR.
    public required string     JuicioEvaluativo { get; init; }
    public           DateTime? FechaJuicio      { get; init; }
    public required string     Funcionario      { get; init; }
}
