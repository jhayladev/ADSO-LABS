using AdsoLabs.Application.AppService.Importacion;
using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.Interfaces;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using CompetenciaModel = AdsoLabs.Infrastructure.Models.Competencia;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AdsoLabs.Application.Importacion;

// ═══════════════════════════════════════════════════════════════════════════════
//
//  ImportadorReporteJuicios  —  Importador del Reporte de Juicios Evaluativos SOFIA Plus
//
//  RESPONSABILIDADES (lo que SÍ hace):
//    ✅ Leer y normalizar el Excel de SOFIA Plus (Fase 1 — sin BD).
//    ✅ Actualizar estados de aprendices, competencias y resultados (Fase 2).
//    ✅ Registrar trazabilidad en ImportacionArchivo e ImportacionError.
//    ✅ Recalcular estados de FichaCompetencia al finalizar.
//
//  RESTRICCIONES (lo que NO hace):
//    ❌ NO crea fichas → la ficha debe existir antes de importar.
//    ❌ NO crea aprendices → el aprendiz debe estar registrado previamente.
//    ❌ NO define programación → instructor, fechas y horas se ingresan manualmente.
//    ❌ NO completa datos faltantes → refleja la realidad, no la asume.
//
//  ARQUITECTURA — dos fases desacopladas:
//
//  ┌─────────────────────────────────────────────────────────────────────────┐
//  │  FASE 1 — NormalizarReporteSofia(Stream)                                │
//  │           Excel sucio de SOFIA → List<ReporteJuicioDto>                 │
//  │   • Pure function — sin EF Core, sin BD                                 │
//  │   • Emite TODAS las filas con juicio (APROBADO y POR EVALUAR)           │
//  │   • Solo descarta filas vacías o sin juicio presente                    │
//  │   • Detecta dinámicamente la fila de encabezados                        │
//  │   • Mapea columnas por nombre (tolerante a variaciones del formato)     │
//  └─────────────────────────────────────────────────────────────────────────┘
//           ↓ List<ReporteJuicioDto>  (APROBADO + POR EVALUAR — todos los RAs)
//  ┌─────────────────────────────────────────────────────────────────────────┐
//  │  FASE 2 — ImportarNormalizadoAsync(dtos, idUsuario, idArchivo)          │
//  │           List<ReporteJuicioDto> → BD                                   │
//  │   • Valida que la ficha exista (rechaza todo el archivo si no)          │
//  │   • Valida que cada aprendiz exista (error por fila si no)              │
//  │   • Upserts idempotentes: Competencia, FichaCompetencia,                │
//  │     PlanConcertado, ResultadoAprendizaje, FichaCompetenciaResultado     │
//  │   • Actualiza estado de FichaAprendiz si el vínculo ya existe           │
//  │   • Cada DTO en su propia transacción (un error no detiene los demás)   │
//  │   • Recalcula estados de FichaCompetencia al final                      │
//  └─────────────────────────────────────────────────────────────────────────┘
//
// ═══════════════════════════════════════════════════════════════════════════════

public class ImportadorReporteJuicios : IImportadorReporteJuicios
{
    private readonly AdsoDbContext                      _db;
    private readonly IAprendizService                   _aprendizService;
    private readonly ILogger<ImportadorReporteJuicios>  _log;

    public ImportadorReporteJuicios(
        AdsoDbContext                      db,
        IAprendizService                   aprendizService,
        ILogger<ImportadorReporteJuicios>  logger)
    {
        _db              = db;
        _aprendizService = aprendizService;
        _log             = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ORQUESTADOR — abre el archivo, ejecuta Fase 1 y luego Fase 2
    // ═══════════════════════════════════════════════════════════════════════

    public async Task<ResultadoImportacion> ImportarAsync(
        string rutaExcel,
        int    idUsuarioAdmin,
        int    idArchivoRegistro,
        int    idInstructorDefault)
    {
        // ExcelDataReader requiere este proveedor para archivos .xls (BIFF8 legacy)
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // ── Fase 1: Excel → DTOs (sin BD, pure function) ─────────────────────
        List<ReporteJuicioDto> dtos;
        await using (var stream = File.OpenRead(rutaExcel))
            dtos = NormalizarReporteSofia(stream);

        int nAprobado   = dtos.Count(d => d.JuicioEvaluativo == EstadosJuicio.Aprobado);
        int nPorEvaluar = dtos.Count - nAprobado;
        _log.LogInformation(
            "Fase 1 completada — {a} filas APROBADO + {p} filas POR EVALUAR " +
            "(solo filas vacías o sin juicio descartadas)",
            nAprobado, nPorEvaluar);

        // ── Fase 2: DTOs → BD ─────────────────────────────────────────────────
        return await ImportarNormalizadoAsync(dtos, idUsuarioAdmin, idArchivoRegistro);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  FASE 1 — NORMALIZADOR  (Excel → DTO, sin BD)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Convierte el stream de un .xls de SOFIA Plus en una lista de DTOs normalizados.
    ///
    /// Reglas de descarte (fila completa ignorada):
    ///   • JuicioEvaluativo es nulo o vacío.
    ///   • Documento y competencia ambos vacíos (fila en blanco).
    ///
    /// Las filas POR EVALUAR se mantienen: son necesarias para crear
    /// FichaCompetenciaResultado en BD y garantizar que el recálculo refleje
    /// el conjunto COMPLETO de RAs del Excel.
    ///
    /// Esta función es pura: sin EF Core, sin BD, sin efectos secundarios.
    /// </summary>
    public static List<ReporteJuicioDto> NormalizarReporteSofia(Stream excelStream)
    {
        using var reader = ExcelReaderFactory.CreateReader(excelStream);
        var ds = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
        });

        var tabla = ds.Tables[0];

        // Extraer metadatos del encabezado del Excel (primeras filas)
        var cab = ExtraerCabecera(tabla);

        // Localizar la fila de encabezados de columnas y mapear sus índices
        int headerRow = LocalizarFilaEncabezado(tabla);
        var col       = MapearColumnas(tabla, headerRow);

        var resultado = new List<ReporteJuicioDto>();

        for (int r = headerRow + 1; r < tabla.Rows.Count; r++)
        {
            string doc  = Celda(tabla, r, col["doc"]).Trim();
            string comp = Celda(tabla, r, col["competencia"]).Trim();

            // Omitir filas completamente vacías (frecuentes al final del Excel SOFIA)
            if (string.IsNullOrWhiteSpace(doc) && string.IsNullOrWhiteSpace(comp))
                continue;

            // Leer el juicio antes de parsear el resto → fallo rápido si no hay juicio
            string juicioRaw = Limpiar(Celda(tabla, r, col["juicio"])).ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(juicioRaw))
                continue;

            // Normalizar: cualquier variante → "APROBADO" o "POR EVALUAR"
            string juicioNormalizado = juicioRaw.Contains("POR EVALUAR")
                ? "POR EVALUAR"
                : EstadosJuicio.Aprobado;

            var (codComp,   nomComp)   = SepararCodigoNombre(comp);
            var (codResult, nomResult) = SepararCodigoNombre(Celda(tabla, r, col["resultado"]));

            resultado.Add(new ReporteJuicioDto
            {
                // Metadatos de la ficha (repetidos por fila para que los DTOs sean autónomos)
                NumeroFicha    = cab.NumeroFicha,
                Denominacion   = cab.Denominacion,
                EstadoFicha    = cab.EstadoRaw,
                FechaInicio    = cab.FechaInicio,
                FechaFin       = cab.FechaFin,
                Modalidad      = cab.Modalidad,
                Regional       = cab.Regional,
                Centro         = cab.Centro,

                // Aprendiz
                TipoDocumento  = NormalizarTipoDoc(Celda(tabla, r, col["tipoDoc"])),
                Documento      = doc,
                Nombres        = Celda(tabla, r, col["nombres"]).Trim(),
                Apellidos      = Celda(tabla, r, col["apellidos"]).Trim(),
                EstadoAprendiz = NormalizarEstadoAprendiz(Celda(tabla, r, col["estado"])),

                // Competencia y resultado
                CodigoCompetencia = codComp,
                NombreCompetencia = nomComp,
                CodigoResultado   = codResult,
                NombreResultado   = nomResult,

                // "APROBADO" o "POR EVALUAR" — la Fase 2 decide qué hacer con cada uno
                JuicioEvaluativo = juicioNormalizado,
                FechaJuicio      = ParseDateTime(Celda(tabla, r, col["fechaJuicio"])),
                Funcionario      = Celda(tabla, r, col["funcionario"]).Trim()
            });
        }

        return resultado;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  FASE 2 — IMPORTADOR  (DTO → BD, solo persistencia)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Persiste la lista de DTOs normalizados en la base de datos.
    ///
    /// PRECONDICIONES (rechaza si no se cumplen):
    ///   • La ficha indicada en el Excel DEBE existir en BD.
    ///   • Cada aprendiz DEBE estar registrado; si no existe, se registra el error
    ///     y se continúa con la siguiente fila.
    ///
    /// Responsabilidades:
    ///   • Registrar importación (trazabilidad).
    ///   • Upsert idempotente: Competencia, FichaCompetencia,
    ///     PlanConcertado, ResultadoAprendizaje, FichaCompetenciaResultado.
    ///   • Actualizar estado de FichaAprendiz si el vínculo ya existe.
    ///   • Cada DTO en su propia transacción — un error no detiene los demás.
    ///   • Recálculo de estados de FichaCompetencia al finalizar.
    ///
    /// Restricciones absolutas:
    ///   ❌ Sin parsing de Excel
    ///   ❌ Sin regex
    ///   ❌ Sin creación de Fichas ni Personas/Aprendices
    ///   ✅ Solo persistencia y actualización de estados
    /// </summary>
    public async Task<ResultadoImportacion> ImportarNormalizadoAsync(
        List<ReporteJuicioDto> datos,
        int                    idUsuario,
        int                    idArchivo)
    {
        string numFicha = datos.FirstOrDefault()?.NumeroFicha ?? "?";

        // ── Registrar importación (trazabilidad inicial) ──────────────────────
        var importacion = new ImportacionArchivo
        {
            TipoEntidad         = "Aprendiz",
            IdDocumento         = idArchivo,
            IdUsuario           = idUsuario,
            EstadoProcesamiento = EstadosImportacion.Pendiente,
            TotalFilas          = datos.Count,
            Observacion         = $"Ficha {numFicha} — Juicios Evaluativos SOFIA Plus"
        };
        _db.ImportacionArchivo.Add(importacion);
        await _db.SaveChangesAsync();

        var resultado = new ResultadoImportacion
        {
            IdImportacion = importacion.IdImportacion,
            TotalFilas    = datos.Count
        };

        // ════════════════════════════════════════════════════════════════════
        //  VALIDACIÓN GLOBAL — La ficha debe existir antes de procesar
        //
        //  Dado que TODAS las filas del archivo pertenecen a la misma ficha,
        //  si la ficha no existe se rechaza TODO el archivo inmediatamente.
        //  El importador de juicios no crea fichas: esa responsabilidad es del
        //  módulo de importación de fichas (Importaciones.razor).
        // ════════════════════════════════════════════════════════════════════
        var fichaRow = await _db.Ficha
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.NumeroFicha == numFicha);

        if (fichaRow is null)
        {
            string motivoRechazo =
                $"La ficha '{numFicha}' no existe en el sistema. " +
                "Registre la ficha en el módulo de importación antes de cargar sus juicios evaluativos.";

            _log.LogWarning("Importación rechazada — {motivo}", motivoRechazo);
            resultado.FilasError = datos.Count;
            resultado.Errores.Add(motivoRechazo);

            // Marcar el registro de importación como Error y salir
            await _db.ImportacionArchivo
                .Where(i => i.IdImportacion == importacion.IdImportacion)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.EstadoProcesamiento, EstadosImportacion.Error)
                    .SetProperty(i => i.FilasError,          datos.Count)
                    .SetProperty(i => i.FechaProcesado,      (DateTime?)DateTime.Now));

            return resultado;
        }

        int idFicha = fichaRow.IdFicha;
        resultado.FichasAfectadas.Add(numFicha);

        // ── Catálogo de tipos de documento (carga única al inicio) ────────────
        var tiposDoc = await _db.TipoDocumento
            .AsNoTracking()
            .ToDictionaryAsync(t => t.Nombre.ToUpper(), t => t.IdTipoDocumento);

        // ── Caches en memoria (optimización — NUNCA compuerta de ejecución) ───
        //
        // Evitan SELECT repetidos cuando múltiples filas comparten la misma
        // competencia, resultado o aprendiz dentro del mismo archivo.
        //
        // Un miss de caché SIEMPRE consulta BD antes de operar.
        // En rollback se evictan solo las entradas INSERTADAS (isNew=true);
        // las encontradas (isNew=false) son válidas en BD y no se tocan.
        var cacheCompetencias     = new Dictionary<string, int>(); // codComp  → IdCompetencia
        var cacheFichaCompetencia = new Dictionary<string, int>(); // codComp  → IdFichaCompetencia
        var cachePlanes           = new Dictionary<string, int>(); // codComp  → IdPlan
        var cacheResultados       = new Dictionary<string, int>(); // "comp_res"→ IdResultado
        var cacheAprendices       = new Dictionary<string, (int id, bool found)>(); // numDoc → (IdAprendiz, found)

        // ── Procesar un DTO por transacción ───────────────────────────────────
        foreach (var dto in datos)
        {
            _log.LogDebug(
                "[Fila] Doc={doc} | Comp={comp} | Res={res} | Juicio={j}",
                dto.Documento, dto.CodigoCompetencia, dto.CodigoResultado, dto.JuicioEvaluativo);

            // IDs resueltos en esta iteración (0 = no disponible)
            int idCompetencia      = 0;
            int idFichaCompetencia = 0;
            int idPlan             = 0;
            int idResultado        = 0;
            int idAprendiz         = 0;

            // Claves de entradas NUEVAS en caché (INSERT) → evictar en rollback.
            string? cacheKeyComp  = null;
            string? cacheKeyFC    = null;
            string? cacheKeyPlan  = null;
            string? cacheKeyRes   = null;

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Etapa productiva / práctica: no tiene resultados evaluativos en el flujo normal.
                // Se procesa el aprendiz (actualización de estado) pero se omiten todos los
                // pasos académicos (competencia, fichaCompetencia, plan, resultado, juicio).
                //
                // Detección por código Y por nombre:
                //   • Código "590803"               → caso estándar SOFIA Plus.
                //   • Nombre contiene "ETAPA PRACTICA" → algunos reportes exportan el código
                //     distinto pero el nombre identifica la misma etapa (ej. "Resultados de
                //     aprendizaje etapa practica"). Se normaliza para evitar crear competencias
                //     duplicadas en la BD.
                bool esEtapaPractica =
                    dto.CodigoCompetencia == "590803" ||
                    dto.NombreCompetencia.Contains("ETAPA PRACTICA",   StringComparison.OrdinalIgnoreCase) ||
                    dto.NombreCompetencia.Contains("ETAPA PRODUCTIVA",  StringComparison.OrdinalIgnoreCase);

                // ── PASO 1: Competencia ───────────────────────────────────────
                // La ficha ya está validada fuera del loop; aquí solo se gestionan competencias.
                if (!esEtapaPractica && !string.IsNullOrWhiteSpace(dto.CodigoCompetencia))
                {
                    if (!cacheCompetencias.TryGetValue(dto.CodigoCompetencia, out idCompetencia))
                    {
                        var (id, isNew) = await UpsertCompetenciaAsync(
                            dto.CodigoCompetencia, dto.NombreCompetencia);
                        idCompetencia = id;
                        cacheCompetencias[dto.CodigoCompetencia] = idCompetencia;
                        if (isNew) cacheKeyComp = dto.CodigoCompetencia;
                    }
                }

                // ── PASO 2: FichaCompetencia ──────────────────────────────────
                // Vincula la competencia con la ficha. Estado inicial = Pendiente;
                // se recalcula al final con los juicios ya persistidos.
                if (!esEtapaPractica && idCompetencia > 0)
                {
                    if (!cacheFichaCompetencia.TryGetValue(dto.CodigoCompetencia, out idFichaCompetencia))
                    {
                        var (id, isNew) = await UpsertFichaCompetenciaAsync(idFicha, idCompetencia);
                        idFichaCompetencia = id;
                        cacheFichaCompetencia[dto.CodigoCompetencia] = idFichaCompetencia;
                        if (isNew) cacheKeyFC = dto.CodigoCompetencia;
                    }
                }

                // ── PASO 3: PlanConcertado ────────────────────────────────────
                // Contenedor lógico de los ResultadosAprendizaje de la competencia.
                if (!esEtapaPractica && idCompetencia > 0)
                {
                    if (!cachePlanes.TryGetValue(dto.CodigoCompetencia, out idPlan))
                    {
                        var (id, isNew) = await UpsertPlanConcertadoAsync(idCompetencia, idArchivo);
                        idPlan = id;
                        cachePlanes[dto.CodigoCompetencia] = idPlan;
                        if (isNew) cacheKeyPlan = dto.CodigoCompetencia;
                    }
                }

                // ── PASO 4: ResultadoAprendizaje ──────────────────────────────
                // Clave compuesta "{codComp}_{codRes}" evita colisiones entre
                // competencias con resultados de mismo código numérico.
                string claveRes = $"{dto.CodigoCompetencia}_{dto.CodigoResultado}";
                if (!esEtapaPractica && idPlan > 0 && !string.IsNullOrWhiteSpace(dto.CodigoResultado))
                {
                    if (!cacheResultados.TryGetValue(claveRes, out idResultado))
                    {
                        var (id, isNew) = await UpsertResultadoAprendizajeAsync(
                            idPlan, dto.CodigoResultado, dto.NombreResultado);
                        idResultado = id;
                        cacheResultados[claveRes] = idResultado;
                        if (isNew) cacheKeyRes = claveRes;
                    }
                }

                // ── PASO 5: FichaCompetenciaResultado ─────────────────────────
                // Solo se consideran resultados que ya tienen programación registrada
                // (estado Programado o Completado). Si el FCR no existe o está Pendiente,
                // se omite el juicio de esta fila sin interrumpir la importación.
                bool fcrProgramado = false;
                if (!esEtapaPractica && idFichaCompetencia > 0 && idResultado > 0)
                {
                    var fcrEstado = await _db.FichaCompetenciaResultado
                        .AsNoTracking()
                        .Where(r => r.IdFichaCompetencia == idFichaCompetencia
                                 && r.IdResultado        == idResultado)
                        .Select(r => (string?)r.Estado)
                        .FirstOrDefaultAsync();

                    fcrProgramado = fcrEstado is "Programado" or "Completado";

                    // Si no existe todavía, crearlo en Pendiente para que el catálogo
                    // quede consistente, pero no se procesará el juicio.
                    if (fcrEstado is null)
                        await UpsertFichaCompetenciaResultadoAsync(idFichaCompetencia, idResultado);
                }

                // ── PASO 6: Aprendiz (solo lectura — NO crea registros) ───────
                //
                // El importador de juicios NO crea aprendices.
                // Si el aprendiz no está registrado en el sistema, se registra el error
                // para esta fila y se continúa con las demás.
                //
                // La creación de aprendices es responsabilidad del módulo
                // "Cargar Aprendices" disponible en la vista de la ficha.
                if (!string.IsNullOrWhiteSpace(dto.Documento))
                {
                    if (!cacheAprendices.TryGetValue(dto.Documento, out var cached))
                    {
                        // Buscar el aprendiz por número de documento sin crear nada
                        var aprendizRow = await _db.AprendizPerfil
                            .AsNoTracking()
                            .Where(a => a.Persona.NumeroDocumento == dto.Documento)
                            .Select(a => (int?)a.IdAprendiz)
                            .FirstOrDefaultAsync();

                        bool found = aprendizRow.HasValue;
                        cached = (aprendizRow ?? 0, found);
                        cacheAprendices[dto.Documento] = cached;
                    }

                    if (!cached.found)
                    {
                        // Aprendiz no registrado: error por fila, no se detiene la importación
                        throw new InvalidOperationException(
                            $"El aprendiz con documento '{dto.Documento}' no está registrado " +
                            "en el sistema. Cárguelo primero desde la sección de la ficha.");
                    }

                    idAprendiz = cached.id;
                }

                // ── PASO 7: Actualizar estado del vínculo FichaAprendiz ───────
                //
                // Si el aprendiz ya está vinculado a la ficha, actualiza su estado.
                // Si aún no está vinculado, el vínculo NO se crea aquí: eso es
                // responsabilidad del módulo "Cargar Aprendices".
                if (idAprendiz > 0)
                {
                    var vinculo = await _db.FichaAprendiz
                        .FirstOrDefaultAsync(fa => fa.IdFicha == idFicha && fa.IdAprendiz == idAprendiz);

                    if (vinculo is not null && vinculo.Estado != dto.EstadoAprendiz)
                    {
                        // Aplicar regla de traslado: no revertir si existe ficha más reciente
                        await _aprendizService.VincularAprendizFichaAsync(
                            idFicha, idAprendiz, dto.EstadoAprendiz);
                    }
                }

                // ── PASO 8: JuicioResultado (CRÍTICO — sin skips silenciosos) ─
                //
                // Solo se persiste el juicio para filas APROBADO.
                // Las filas POR EVALUAR garantizaron la existencia del FCR (Paso 5)
                // y actualizaron el estado del aprendiz (Paso 7). Su ausencia en
                // JuicioResultado es el equivalente semántico de "aún sin aprobar".
                //
                // Solo se registra el juicio si el FCR tiene programación activa
                // (Programado o Completado). Si está Pendiente se omite silenciosamente:
                // no es un error, simplemente ese resultado aún no fue programado.
                if (!esEtapaPractica && fcrProgramado
                    && !string.IsNullOrWhiteSpace(dto.CodigoResultado)
                    && dto.JuicioEvaluativo == EstadosJuicio.Aprobado)
                {
                    if (idResultado == 0)
                        throw new InvalidOperationException(
                            $"No se pudo obtener ResultadoAprendizaje para " +
                            $"código='{dto.CodigoResultado}' competencia='{dto.CodigoCompetencia}'. " +
                            "El juicio APROBADO no puede registrarse.");

                    if (idAprendiz == 0)
                        throw new InvalidOperationException(
                            $"No se pudo resolver el aprendiz para documento='{dto.Documento}'. " +
                            "El juicio APROBADO no puede registrarse.");

                    bool esNuevoJuicio = await UpsertJuicioResultadoAsync(
                        idResultado, idAprendiz,
                        dto.JuicioEvaluativo,
                        dto.Funcionario,
                        dto.FechaJuicio);

                    if (esNuevoJuicio) resultado.JuiciosInsertados++;
                    else               resultado.JuiciosActualizados++;

                    _log.LogDebug(
                        "Juicio {accion}: Resultado={r} Aprendiz={a}",
                        esNuevoJuicio ? "INSERTADO" : "ACTUALIZADO",
                        idResultado, idAprendiz);
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                resultado.FilasOk++;
            }
            catch (Exception ex)
            {
                // ── Rollback de esta fila ─────────────────────────────────────
                await tx.RollbackAsync();
                resultado.FilasError++;
                _db.ChangeTracker.Clear();

                // ── Evicción selectiva del caché ──────────────────────────────
                // Solo se evictan las entradas INSERTADAS (isNew=true).
                // Los SELECT son válidos en BD incluso tras rollback.
                // Si la Competencia fue nueva, FichaCompetencia y Plan también lo eran.
                if (cacheKeyComp != null)
                {
                    cacheCompetencias.Remove(cacheKeyComp);
                    cacheFichaCompetencia.Remove(cacheKeyComp);
                    cachePlanes.Remove(cacheKeyComp);
                }
                else
                {
                    if (cacheKeyFC   != null) cacheFichaCompetencia.Remove(cacheKeyFC);
                    if (cacheKeyPlan != null) cachePlanes.Remove(cacheKeyPlan);
                }

                if (cacheKeyRes != null) cacheResultados.Remove(cacheKeyRes);

                // ── Trazar el error en la BD ──────────────────────────────────
                string motivo = ex.InnerException?.Message ?? ex.Message;
                if (motivo.Length > 490) motivo = motivo[..490];

                _log.LogWarning(
                    "Error Doc={doc} Comp={comp} Res={res}: {motivo}",
                    dto.Documento, dto.CodigoCompetencia, dto.CodigoResultado, motivo);

                resultado.Errores.Add(
                    $"Doc={dto.Documento} | Comp={dto.CodigoCompetencia} | " +
                    $"Res={dto.CodigoResultado}: {motivo}");

                // SaveChanges fuera de la tx fallida → siempre persiste el error
                _db.ImportacionError.Add(new ImportacionError
                {
                    IdImportacion = importacion.IdImportacion,
                    NumeroFila    = resultado.FilasOk + resultado.FilasError,
                    DatosFila     = JsonSerializer.Serialize(dto),
                    MotivoError   = motivo
                });
                await _db.SaveChangesAsync();
            }
        }

        // ── Recalcular estados de FichaCompetencia ────────────────────────────
        // Una vez por ficha afectada, con todos los juicios ya persistidos.
        // Determina si cada competencia está en Vista, En Curso o Pendiente.
        await RecalcularEstadoFichaCompetenciaAsync(idFicha);

        // ── Cerrar registro de importación ────────────────────────────────────
        // ExecuteUpdateAsync → SQL directo, funciona aunque `importacion` haya
        // quedado detached por algún ChangeTracker.Clear() en el loop.
        var estadoFinal = resultado.FilasError == 0
            ? EstadosImportacion.Procesado
            : EstadosImportacion.Error;

        await _db.ImportacionArchivo
            .Where(i => i.IdImportacion == importacion.IdImportacion)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.EstadoProcesamiento, estadoFinal)
                .SetProperty(i => i.FilasOk,             resultado.FilasOk)
                .SetProperty(i => i.FilasError,          resultado.FilasError)
                .SetProperty(i => i.FechaProcesado,      (DateTime?)DateTime.Now));

        _log.LogInformation(
            "Importación {id}: OK={ok} ERR={err} JuiciosIns={ji} JuiciosUpd={ju}",
            importacion.IdImportacion,
            resultado.FilasOk, resultado.FilasError,
            resultado.JuiciosInsertados, resultado.JuiciosActualizados);

        return resultado;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UPSERTS — PERSISTENCIA IDEMPOTENTE
    //
    //  Retorno (int id, bool isNew):
    //    id    = PK del registro (existente o recién creado)
    //    isNew = true → INSERT ejecutado; false → SELECT encontró el registro
    //
    //  AsNoTracking en los que NUNCA modifican el objeto existente.
    //  Sin AsNoTracking en los que actualizan propiedades del existente.
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea la competencia si no existe. Si ya existe, la devuelve sin modificar.
    /// El tipo se infiere del código según la tabla de mapeo ADSO.
    /// </summary>
    private async Task<(int id, bool isNew)> UpsertCompetenciaAsync(string codigo, string nombre)
    {
        var existing = await _db.Competencia
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == codigo);
        if (existing is not null) return (existing.IdCompetencia, false);

        // Clasificación de tipo por código (tabla oficial ADSO)
        string tipo = codigo switch
        {
            "590803"                                                         => "Practica",
            "36182"                                                          => "Induccion",
            "37371" or "38560" or "37801"                                   => "Clave",
            "36180" or "37714" or "37799" or "37800" or "37802"
                or "38199" or "38558" or "38561"                            => "Transversal",
            _                                                                => "Tecnica"
        };

        var comp = new CompetenciaModel
        {
            Codigo         = codigo,
            Nombre         = nombre.Length > 200 ? nombre[..200] : nombre,
            HorasAsignadas = 48,   // valor de catálogo; el real se configura por ficha (TotalHoras)
            Tipo           = tipo
        };
        _db.Competencia.Add(comp);
        await _db.SaveChangesAsync();
        return (comp.IdCompetencia, true);
    }

    /// <summary>
    /// Crea el vínculo Ficha-Competencia si no existe.
    /// FechaInicio, FechaFin y TotalHoras se dejan en NULL: se configuran manualmente
    /// desde el módulo de asignación de competencias, no desde la importación.
    /// </summary>
    private async Task<(int id, bool isNew)> UpsertFichaCompetenciaAsync(
        int idFicha, int idCompetencia)
    {
        var existing = await _db.FichaCompetencia
            .AsNoTracking()
            .FirstOrDefaultAsync(fc => fc.IdFicha == idFicha && fc.IdCompetencia == idCompetencia);
        if (existing is not null) return (existing.IdFichaCompetencia, false);

        var fc = new FichaCompetencia
        {
            IdFicha         = idFicha,
            IdCompetencia   = idCompetencia,
            Estado          = EstadosCompetencia.Pendiente,
            HorasEjecutadas = 0
            // FechaInicio, FechaFin y TotalHoras: NULL hasta configuración manual
        };
        _db.FichaCompetencia.Add(fc);
        await _db.SaveChangesAsync();
        return (fc.IdFichaCompetencia, true);
    }

    /// <summary>
    /// Crea el PlanConcertado de la competencia si no existe.
    /// Es el contenedor lógico de los ResultadosAprendizaje asociados.
    /// </summary>
    private async Task<(int id, bool isNew)> UpsertPlanConcertadoAsync(
        int  idCompetencia,
        int? idDocumento)   // nullable: null cuando el plan llega del seeder o no tiene archivo
    {
        // Buscar cualquier plan de la competencia, sin filtrar por instructor ni activo.
        // Un PlanConcertado es único por competencia; no debe haber más de uno.
        var existing = await _db.PlanConcertado
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdCompetencia == idCompetencia);
        if (existing is not null) return (existing.IdPlan, false);

        var plan = new PlanConcertado
        {
            IdCompetencia = idCompetencia,
            IdInstructor  = null,
            IdDocumento   = idDocumento,
            Estado        = "Publicado",
            Activo        = true    // C# default de bool es false; hay que asignarlo explícitamente
        };
        _db.PlanConcertado.Add(plan);
        await _db.SaveChangesAsync();
        return (plan.IdPlan, true);
    }

    /// <summary>
    /// Crea el ResultadoAprendizaje en el plan si no existe.
    /// Clave compuesta (IdPlan + Codigo) para evitar duplicados entre competencias.
    /// </summary>
    private async Task<(int id, bool isNew)> UpsertResultadoAprendizajeAsync(
        int idPlan, string codigo, string nombre)
    {
        var existing = await _db.ResultadoAprendizaje
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdPlan == idPlan && r.Codigo == codigo);
        if (existing is not null) return (existing.IdResultado, false);

        string nombreCompleto = $"{codigo} - {nombre}";
        if (nombreCompleto.Length > 200) nombreCompleto = nombreCompleto[..200];

        var ra = new ResultadoAprendizaje
        {
            IdPlan = idPlan,
            Codigo = codigo,
            Nombre = nombreCompleto
        };
        _db.ResultadoAprendizaje.Add(ra);
        await _db.SaveChangesAsync();
        return (ra.IdResultado, true);
    }

    /// <summary>
    /// Crea el FichaCompetenciaResultado (FCR) si no existe.
    /// El FCR es necesario para que el recálculo de estados incluya TODOS los RAs
    /// del Excel, incluyendo los POR EVALUAR (sin juicio registrado aún).
    /// </summary>
    private async Task UpsertFichaCompetenciaResultadoAsync(int idFichaCompetencia, int idResultado)
    {
        bool existe = await _db.FichaCompetenciaResultado.AnyAsync(
            r => r.IdFichaCompetencia == idFichaCompetencia && r.IdResultado == idResultado);
        if (existe) return;

        _db.FichaCompetenciaResultado.Add(new FichaCompetenciaResultado
        {
            IdFichaCompetencia = idFichaCompetencia,
            IdResultado        = idResultado,
            Estado             = "Pendiente"
        });
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Crea o actualiza el JuicioResultado para un aprendiz y resultado dados.
    /// Solo se llama para juicios APROBADO; la ausencia de registro equivale a POR EVALUAR.
    /// </summary>
    private async Task<bool> UpsertJuicioResultadoAsync(
        int       idResultado,
        int       idAprendiz,
        string    juicio,
        string?   funcionario,
        DateTime? fechaJuicio)
    {
        var existing = await _db.JuicioResultado
            .FirstOrDefaultAsync(j => j.IdResultado == idResultado && j.IdAprendiz == idAprendiz);

        if (existing is not null)
        {
            existing.Juicio        = juicio;
            // Solo actualizar funcionario y fecha si vienen con valor en el Excel
            existing.RegistradoPor = string.IsNullOrWhiteSpace(funcionario)
                ? existing.RegistradoPor : funcionario;
            existing.FechaJuicio   = fechaJuicio ?? existing.FechaJuicio;
            return false;   // existía → actualizado
        }

        _db.JuicioResultado.Add(new JuicioResultado
        {
            IdResultado   = idResultado,
            IdAprendiz    = idAprendiz,
            Juicio        = juicio,
            RegistradoPor = funcionario,
            FechaJuicio   = fechaJuicio
        });
        return true;   // nuevo → insertado
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  RECÁLCULO DE ESTADOS DE FICHACOMPETENCIA
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Recalcula el estado (Vista / En Curso / Pendiente) de cada FichaCompetencia.
    ///
    /// Reglas (CLAUDE.md §7.5):
    ///   VISTA    = 100% aprendicesActivos × 100% resultados tienen JuicioResultado APROBADO.
    ///              Estado máximo — NUNCA se degrada por reimportación.
    ///   EN CURSO = al menos 1 APROBADO pero no todos.
    ///   PENDIENTE= 0 APROBADOS. No se toca si el estado actual es Programada.
    ///
    /// AsNoTracking + ExecuteUpdateAsync → SQL directo, sin interferir
    /// con el change tracker ni la entidad `importacion` del llamador.
    ///
    /// Lógica por niveles:
    ///
    ///   NIVEL 1 — por cada RA (FichaCompetenciaResultado):
    ///     COMPLETADO  = todos los aprendices EN FORMACION tienen JuicioResultado APROBADO.
    ///                   La ausencia de juicio equivale a POR EVALUAR.
    ///     Transición → COMPLETADO: solo cambia el Estado; los campos de programación
    ///     (IdInstructor, fechas, horas) se conservan para mantener la trazabilidad.
    ///
    ///   NIVEL 2 — estado de la FichaCompetencia basado en sus RAs:
    ///     VISTA     = TODOS los RAs están COMPLETADO   (estado máximo, nunca se degrada)
    ///     EN CURSO  = ≥1 RA en COMPLETADO  O  ≥1 RA en PROGRAMADO  (pero no todos COMPLETADO)
    ///     PENDIENTE = 0 RAs COMPLETADO y 0 RAs PROGRAMADO
    /// </summary>
    private async Task RecalcularEstadoFichaCompetenciaAsync(int idFicha)
    {
        // Leer el estado actual de las FichaCompetencias desde BD
        var fichasCompetencias = await _db.FichaCompetencia
            .AsNoTracking()
            .Where(fc => fc.IdFicha == idFicha)
            .ToListAsync();

        // Solo los aprendices EN FORMACION participan en el cálculo de juicios
        var aprendicesActivos = await _db.FichaAprendiz
            .AsNoTracking()
            .Where(fa => fa.IdFicha == idFicha && fa.Estado == EstadosAprendiz.EnFormacion)
            .Select(fa => fa.IdAprendiz)
            .ToListAsync();

        // Sin aprendices activos: no hay juicios que evaluar, no se recalcula
        if (!aprendicesActivos.Any()) return;

        foreach (var fc in fichasCompetencias)
        {
            // VISTA es el estado máximo (logro académico 100% confirmado).
            // Nunca se degrada por reimportación.
            if (fc.Estado == EstadosCompetencia.Vista) continue;

            // Leer los FCRs con su estado actual
            var fcrs = await _db.FichaCompetenciaResultado
                .AsNoTracking()
                .Where(r => r.IdFichaCompetencia == fc.IdFichaCompetencia)
                .ToListAsync();

            if (!fcrs.Any()) continue;

            int totalFcrs   = fcrs.Count;
            int completadas = 0;   // RAs que deben ser COMPLETADO según los juicios
            int programadas = 0;   // RAs en PROGRAMADO que aún no alcanzan COMPLETADO

            // ── NIVEL 1: evaluar cada RA individualmente ───────────────────────
            foreach (var fcr in fcrs)
            {
                // Contar aprobados de este RA para los aprendices activos.
                // La ausencia de JuicioResultado = POR EVALUAR (no registrado).
                int aprobados = await _db.JuicioResultado
                    .AsNoTracking()
                    .CountAsync(j =>
                        j.IdResultado == fcr.IdResultado &&
                        aprendicesActivos.Contains(j.IdAprendiz) &&
                        j.Juicio == EstadosJuicio.Aprobado);

                // Un RA está COMPLETADO cuando todos los activos tienen APROBADO
                bool raDebeCompletarse = aprobados >= aprendicesActivos.Count
                                         && aprendicesActivos.Count > 0;

                if (raDebeCompletarse)
                {
                    completadas++;

                    if (fcr.Estado != "Completado")
                    {
                        // Transición → Completado: solo se cambia el Estado.
                        // Los campos de programación (instructor, fechas, horas) se conservan
                        // para mantener la trazabilidad de cómo se desarrolló la competencia.
                        await _db.FichaCompetenciaResultado
                            .Where(r => r.IdFichaCompetenciaResultado == fcr.IdFichaCompetenciaResultado)
                            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Estado, "Completado"));
                    }
                }
                else
                {
                    if (fcr.Estado == "Programado") programadas++;
                }
            }

            // ── NIVEL 2: determinar el estado de la FichaCompetencia ───────────
            //
            //   VISTA     = todos los RAs son COMPLETADO
            //   EN CURSO  = ≥1 COMPLETADO o ≥1 PROGRAMADO (pero no todos COMPLETADO)
            //   PENDIENTE = 0 COMPLETADO y 0 PROGRAMADO
            string nuevoEstado;

            if (completadas == totalFcrs)
                nuevoEstado = EstadosCompetencia.Vista;
            else if (completadas > 0 || programadas > 0)
                nuevoEstado = EstadosCompetencia.EnCurso;
            else
                nuevoEstado = EstadosCompetencia.Pendiente;

            // Horas ejecutadas = suma de horas_programadas de los FCRs Completado.
            // Se recalcula siempre (Vista, En Curso o Pendiente) para mantener consistencia.
            int horasEjecutadas = await _db.FichaCompetenciaResultado
                .AsNoTracking()
                .Where(r => r.IdFichaCompetencia == fc.IdFichaCompetencia
                         && r.Estado == "Completado"
                         && r.HorasProgramadas != null)
                .SumAsync(r => r.HorasProgramadas ?? 0);

            if (nuevoEstado == EstadosCompetencia.Vista)
            {
                // VISTA: HorasEjecutadas = TotalHoras configurado (100% completado).
                int horasVista = await _db.FichaCompetencia
                    .AsNoTracking()
                    .Where(fc2 => fc2.IdFichaCompetencia == fc.IdFichaCompetencia)
                    .Select(fc2 => fc2.TotalHoras ?? fc2.Competencia.HorasAsignadas)
                    .FirstOrDefaultAsync();

                await _db.FichaCompetencia
                    .Where(fc2 => fc2.IdFichaCompetencia == fc.IdFichaCompetencia)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(fc2 => fc2.Estado,          EstadosCompetencia.Vista)
                        .SetProperty(fc2 => fc2.HorasEjecutadas, horasVista));
            }
            else
            {
                await _db.FichaCompetencia
                    .Where(fc2 => fc2.IdFichaCompetencia == fc.IdFichaCompetencia)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(fc2 => fc2.Estado,          nuevoEstado)
                        .SetProperty(fc2 => fc2.HorasEjecutadas, horasEjecutadas));
            }
        }
        // ExecuteUpdateAsync genera SQL UPDATE directo — no requiere SaveChangesAsync
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  FASE 1 — HELPERS ESTÁTICOS DE PARSEO
    //  Usados exclusivamente por NormalizarReporteSofia.
    //  Sin acceso a BD, sin EF Core, sin efectos secundarios.
    // ═══════════════════════════════════════════════════════════════════════

    private record CabeceraXls(
        string    NumeroFicha,
        string    Denominacion,
        string    EstadoRaw,
        DateOnly? FechaInicio,
        DateOnly? FechaFin,
        string    Modalidad,
        string    Regional,
        string    Centro);

    private static CabeceraXls ExtraerCabecera(DataTable tabla)
    {
        string    numeroFicha = "", denominacion = "", estadoRaw = "EN EJECUCION";
        string    modalidad   = "", regional     = "", centro    = "";
        DateOnly? fechaInicio = null, fechaFin   = null;

        // Posición fija histórica del número de ficha en reportes SOFIA Plus
        numeroFicha = ExtraerDigitos(Celda(tabla, 2, 2));
        fechaFin    = ParseDateOnly(Celda(tabla, 8, 2));

        for (int r = 0; r < Math.Min(15, tabla.Rows.Count); r++)
        {
            for (int c = 0; c < tabla.Columns.Count - 2; c++)
            {
                string clave = Celda(tabla, r, c).ToUpperInvariant();
                string valor = !string.IsNullOrWhiteSpace(Celda(tabla, r, c + 1))
                                ? Celda(tabla, r, c + 1)
                                : Celda(tabla, r, c + 2);

                if ((clave.Contains("COGIGO") || clave.Contains("CÓGIGO") ||
                     clave.Contains("CODIGO") || clave.Contains("FICHA"))
                    && string.IsNullOrWhiteSpace(numeroFicha))
                {
                    string enMisma = ExtraerDigitos(clave);
                    numeroFicha = !string.IsNullOrWhiteSpace(enMisma)
                        ? enMisma : ExtraerDigitos(valor);
                }
                else if ((clave.Contains("DENOMINACI") || clave.Contains("NOMBRE DEL PROGRAMA"))
                         && string.IsNullOrWhiteSpace(denominacion))
                    denominacion = valor;
                else if (clave.Contains("ESTADO DE LA FICHA"))
                    estadoRaw = valor;
                else if (clave.Contains("FECHA INICIO"))
                    fechaInicio = ParseDateOnly(valor);
                else if (clave.Contains("FECHA FIN"))
                    fechaFin = ParseDateOnly(valor);
                else if (clave.Contains("MODALIDAD") && string.IsNullOrWhiteSpace(modalidad))
                    modalidad = valor;
                else if (clave.Contains("REGIONAL") && string.IsNullOrWhiteSpace(regional))
                    regional = valor;
                else if (clave.Contains("CENTRO") && string.IsNullOrWhiteSpace(centro))
                    centro = valor;
            }
        }

        if (string.IsNullOrWhiteSpace(numeroFicha)) numeroFicha = "SIN_NUMERO";
        if (string.IsNullOrWhiteSpace(denominacion)) denominacion = "Análisis y Desarrollo de Software";

        return new CabeceraXls(numeroFicha, denominacion, estadoRaw,
                                fechaInicio, fechaFin, modalidad, regional, centro);
    }

    /// <summary>
    /// Localiza la fila del Excel que contiene los encabezados de columna.
    /// Busca la fila que contenga "TIPO", "DOCUMENTO" y "NOMBRE" simultáneamente.
    /// Si no la encuentra, usa la posición histórica 9 (reportes SOFIA estándar).
    /// </summary>
    private static int LocalizarFilaEncabezado(DataTable tabla)
    {
        for (int r = 0; r < Math.Min(20, tabla.Rows.Count); r++)
        {
            string fila = string.Join(" ", Enumerable
                .Range(0, tabla.Columns.Count)
                .Select(c => tabla.Rows[r][c]?.ToString() ?? ""))
                .ToUpperInvariant();

            if (fila.Contains("TIPO") && fila.Contains("DOCUMENTO") && fila.Contains("NOMBRE"))
                return r;
        }
        return 9;
    }

    /// <summary>
    /// Mapea los nombres de columna del encabezado a sus índices numéricos.
    /// Usa valores por defecto históricos si una columna no se encuentra por nombre.
    /// </summary>
    private static Dictionary<string, int> MapearColumnas(DataTable tabla, int headerRow)
    {
        var idx = new Dictionary<string, int>
        {
            ["tipoDoc"]     = 0,  ["doc"]         = 1,
            ["nombres"]     = 2,  ["apellidos"]   = 3,
            ["estado"]      = 4,  ["competencia"] = 5,
            ["resultado"]   = 6,  ["juicio"]      = 7,
            ["fechaJuicio"] = 9,  ["funcionario"] = 10
        };

        if (headerRow < 0 || headerRow >= tabla.Rows.Count) return idx;

        var reglas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TIPO"]          = "tipoDoc",
            ["NÚMERO DE DOC"] = "doc",
            ["NUMERO DE DOC"] = "doc",
            ["NOMBRE"]        = "nombres",
            ["APELLIDO"]      = "apellidos",
            ["ESTADO"]        = "estado",
            ["COMPETENCIA"]   = "competencia",
            ["RESULTADO"]     = "resultado",
            ["JUICIO"]        = "juicio",
            ["FECHA"]         = "fechaJuicio",
            ["FUNCIONARIO"]   = "funcionario"
        };

        for (int c = 0; c < tabla.Columns.Count; c++)
        {
            string header = (tabla.Rows[headerRow][c]?.ToString() ?? "").ToUpperInvariant();
            foreach (var regla in reglas)
            {
                if (header.Contains(regla.Key) && !idx.ContainsValue(c))
                {
                    idx[regla.Value] = c;
                    break;
                }
            }
        }

        return idx;
    }

    // ── Utilidades de celda y texto ───────────────────────────────────────────

    /// <summary>Devuelve el contenido de una celda como string limpio. Retorna "" si está fuera de rango.</summary>
    private static string Celda(DataTable tabla, int row, int col)
    {
        if (col < 0 || col >= tabla.Columns.Count) return "";
        return tabla.Rows[row][col]?.ToString()?.Trim() ?? "";
    }

    /// <summary>Elimina caracteres de control y espacios no estándar del texto.</summary>
    private static string Limpiar(string raw) =>
        Regex.Replace(raw ?? "", @"[\x00-\x1F\x7F\xA0]", "").Trim();

    /// <summary>Separa "CÓDIGO - Nombre" en sus dos componentes. Si no hay separador, devuelve ("", raw).</summary>
    private static (string codigo, string nombre) SepararCodigoNombre(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return ("", "");
        var m = Regex.Match(raw, @"^(\d+)\s*-\s*(.+)$", RegexOptions.Singleline);
        return m.Success
            ? (m.Groups[1].Value.Trim(), m.Groups[2].Value.Trim())
            : ("", raw.Trim());
    }

    /// <summary>Extrae la primera secuencia de 5+ dígitos del string (para número de ficha).</summary>
    private static string ExtraerDigitos(string raw) =>
        Regex.Match(raw ?? "", @"\d{5,}").Value;

    /// <summary>Parsea una fecha en formato d/MM/yyyy, dd/MM/yyyy o ISO. Retorna null si no es parseable.</summary>
    private static DateOnly? ParseDateOnly(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var solo = raw.Split(' ')[0].Trim();
        if (DateOnly.TryParseExact(solo, "d/MM/yyyy",  null,
            System.Globalization.DateTimeStyles.None, out var d1)) return d1;
        if (DateOnly.TryParseExact(solo, "dd/MM/yyyy", null,
            System.Globalization.DateTimeStyles.None, out var d2)) return d2;
        if (DateOnly.TryParse(solo, out var d3)) return d3;
        return null;
    }

    /// <summary>Parsea una fecha y hora. Retorna null si no es parseable.</summary>
    private static DateTime? ParseDateTime(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return DateTime.TryParse(raw, out var d) ? d : null;
    }

    /// <summary>Normaliza el tipo de documento al código canónico del catálogo (CC, TI, CE, Pasaporte).</summary>
    private static string NormalizarTipoDoc(string raw) => raw.ToUpperInvariant().Trim() switch
    {
        var s when s.StartsWith("CC") || s.Contains("CIUDADAN")  => "CC",
        var s when s.StartsWith("TI") || s.Contains("IDENTIDAD") => "TI",
        var s when s.StartsWith("CE") || s.Contains("EXTRAN")    => "CE",
        var s when s.Contains("PASAPORTE")                        => "Pasaporte",
        _                                                          => "CC"
    };

    /// <summary>Normaliza el estado del aprendiz al valor canónico (CLAUDE.md §7.9).</summary>
    private static string NormalizarEstadoAprendiz(string raw)
    {
        var s = Limpiar(raw).ToUpperInvariant();
        if (s.Contains("TRASLAD"))           return EstadosAprendiz.Trasladado;
        if (s.Contains("RETIRO VOLUNTARIO")) return EstadosAprendiz.RetiroVoluntario;
        if (s.Contains("CANCEL"))            return EstadosAprendiz.Cancelado;
        return EstadosAprendiz.EnFormacion;
    }
}
