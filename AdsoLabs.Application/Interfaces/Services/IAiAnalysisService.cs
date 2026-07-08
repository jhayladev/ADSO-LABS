namespace AdsoLabs.Application.Interfaces.Services;

// Datos de entrada que el servicio de IA recibe desde la capa de infraestructura
public record InstructorIAContext(
    string NombreInstructor,
    string Especialidad,
    IReadOnlyList<CompetenciaContextItem> CompetenciasHistoricas,
    IReadOnlyList<FichaCompetenciaContextItem> FichasDisponibles
);

public record CompetenciaContextItem(
    string NombreCompetencia,
    string CodigoCompetencia,
    int VecesImpartida,
    int TotalAprendices
);

public record FichaCompetenciaContextItem(
    int IdFichaCompetencia,
    string NombreCompetencia,
    string CodigoCompetencia,
    string NumeroFicha,
    string Jornada,
    string? FaseFormativa,
    int ProgresoFicha
);

// Resultado devuelto por el servicio de IA
public record CompatibilidadIAResult(
    int PorcentajeGeneral,
    string ResumenIA,
    IReadOnlyList<string> Fortalezas,
    IReadOnlyList<SugerenciaIAItem> Sugerencias
);

public record SugerenciaIAItem(
    int IdFichaCompetencia,
    int Compatibilidad,
    string Razonamiento
);

public record PrediccionAprendizajeIAResult(
    string TendenciaGeneral,
    string DescripcionTendencia,
    IReadOnlyList<TematicaIAItem> Tematicas,
    IReadOnlyList<PasoRutaIAItem> RutaAprendizaje,
    IReadOnlyList<KpiIAItem> Kpis
);

public record TematicaIAItem(int Numero, string Icono, string Titulo, string Descripcion, string Relevancia);
public record PasoRutaIAItem(int Num, string Titulo, string Descripcion);
public record KpiIAItem(string Etiqueta, string Valor, string Icono, string Color);

// Contexto específico por competencia para la predicción de aprendizaje
public record PrediccionCompetenciaContext(
    string NombreInstructor,
    string Especialidad,
    string NombreCompetencia,
    string CodigoCompetencia,
    string NumeroFicha,
    string? DescripcionCompetencia,
    IReadOnlyList<string> ResultadosAsignados
);

public interface IAiAnalysisService
{
    Task<CompatibilidadIAResult> AnalizarCompatibilidadAsync(InstructorIAContext context, CancellationToken ct = default);
    Task<PrediccionAprendizajeIAResult> PredecirAprendizajeAsync(PrediccionCompetenciaContext context, CancellationToken ct = default);
}
