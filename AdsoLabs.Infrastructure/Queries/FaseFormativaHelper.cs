namespace AdsoLabs.Infrastructure.Queries;

/// <summary>
/// Orden de las fases del proyecto formativo ADSO (Induccion → Analisis → Planeacion →
/// Ejecucion → Evaluacion), usado para inferir en qué punto va una ficha y evitar sugerir
/// competencias de una fase muy adelantada a instructores. "Transversal" no participa del
/// orden: nunca bloquea ni es bloqueada (ej. Inglés, que aparece a lo largo de todo el
/// proceso formativo).
/// </summary>
public static class FaseFormativaHelper
{
    private static readonly Dictionary<string, int> OrdenFase = new()
    {
        ["Induccion"] = 0,
        ["Analisis"] = 1,
        ["Planeacion"] = 2,
        ["Ejecucion"] = 3,
        ["Evaluacion"] = 4
    };

    private static readonly string[] NombresFase = ["Inducción", "Análisis", "Planeación", "Ejecución", "Evaluación"];

    /// <summary>Nombre legible de una fase a partir de su orden numérico (0-4).</summary>
    public static string NombreFase(int orden) => orden >= 0 && orden < NombresFase.Length ? NombresFase[orden] : "Transversal";

    /// <summary>Nombre legible de una fase a partir de su valor almacenado (Induccion|Analisis|...|null).</summary>
    public static string NombreFase(string? faseFormativa) =>
        faseFormativa is not null && OrdenFase.TryGetValue(faseFormativa, out var orden) ? NombreFase(orden) : "Transversal";

    /// <summary>
    /// Margen de fases hacia adelante que se permite sugerir respecto a la fase actual de
    /// la ficha (1 = se permite la fase actual y la inmediatamente siguiente).
    /// </summary>
    public const int MargenFasesAdelante = 1;

    /// <summary>
    /// Fase más avanzada entre las competencias de la ficha que ya están Vista, En Curso o
    /// Programada. Si ninguna competencia de la ficha ha arrancado, asume Analisis (todas
    /// las fichas empiezan ahí una vez pasada la Inducción).
    /// </summary>
    public static int CalcularFaseActualFicha(IEnumerable<(string? FaseFormativa, string EstadoFichaCompetencia)> competenciasFicha)
    {
        var fasesEnCurso = competenciasFicha
            .Where(c => c.EstadoFichaCompetencia is "Vista" or "En Curso" or "Programada")
            .Select(c => OrdenFase.GetValueOrDefault(c.FaseFormativa ?? "", -1))
            .Where(orden => orden >= 0)
            .ToList();

        return fasesEnCurso.Count > 0 ? fasesEnCurso.Max() : OrdenFase["Analisis"];
    }

    /// <summary>
    /// True si la fase de la competencia candidata es sugerible dado el avance actual de su
    /// ficha (no está más de <see cref="MargenFasesAdelante"/> fases por delante). Las
    /// competencias sin fase clasificada o marcadas "Transversal" siempre son sugeribles.
    /// </summary>
    public static bool EsSugerible(string? faseFormativaCandidata, int faseActualFicha)
    {
        if (string.IsNullOrEmpty(faseFormativaCandidata) || !OrdenFase.TryGetValue(faseFormativaCandidata, out var ordenCandidata))
            return true; // Transversal o sin clasificar: no bloquea

        return ordenCandidata <= faseActualFicha + MargenFasesAdelante;
    }
}
