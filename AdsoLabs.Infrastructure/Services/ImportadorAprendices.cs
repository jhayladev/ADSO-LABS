using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Importacion;
using AdsoLabs.Application.AppService.Importacion;
using AdsoLabs.Application.Importacion;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AdsoLabs.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
//
//  ImportadorAprendices  —  Importador masivo de aprendices desde plantilla Excel
//
//  RESPONSABILIDADES (lo que SÍ hace):
//    ✅ Leer el archivo .xlsx usando la plantilla oficial (PlantillaAprendizGenerator).
//    ✅ Leer el archivo .xls exportado por SOFIA Plus (Reporte de Juicios Evaluativos),
//       extrayendo automáticamente los aprendices únicos del reporte.
//    ✅ Crear Persona si el número de documento no existe aún en el sistema.
//    ✅ Actualizar Nombres y Apellidos de Persona si ya existe.
//    ✅ Crear AprendizPerfil si la persona no tiene uno todavía.
//    ✅ Vincular el aprendiz a la ficha con el estado indicado en el Excel.
//    ✅ Aplicar regla de traslado automático (vía IAprendizService, CLAUDE.md §7.9).
//    ✅ Normalizar el estado del Excel (acepta "EN FORMACION" o "En Formacion", etc.).
//    ✅ Asumir "EN FORMACION" cuando la celda de Estado está vacía.
//    ✅ Devolver un resumen detallado: AprendicesCreados, AprendicesActualizados,
//       VinculosCreados, VinculosActualizados, FilasOk, FilasError y lista de errores.
//
//  RESTRICCIONES (lo que NO hace):
//    ❌ NO crea la ficha: el número de ficha debe existir previamente en la BD.
//    ❌ NO crea cuentas de usuario (Usuario): solo Persona + AprendizPerfil.
//    ❌ NO registra trazabilidad en ImportacionArchivo (sin requisito de historial aún).
//    ❌ NO elimina vínculos existentes: importar el mismo archivo dos veces es inocuo.
//
//  DETECCIÓN AUTOMÁTICA DE FORMATO:
//    • Extensión .xlsx → Plantilla oficial (PlantillaAprendizGenerator)
//    • Extensión .xls  → Reporte de Juicios Evaluativos de SOFIA Plus
//
//  FORMATO DE LA PLANTILLA (PlantillaAprendizGenerator):
//    Fila 1:  Encabezados (se salta)
//    Fila 2+: Un aprendiz por fila
//    Columna A: TipoDocumento  → abreviatura (CC / TI / CE / PPT / PA / RC)
//    Columna B: NumeroDocumento → número único de identificación
//    Columna C: Nombres         → nombres completos
//    Columna D: Apellidos       → apellidos completos
//    Columna E: Estado          → EN FORMACION / CANCELADO / RETIRO VOLUNTARIO /
//                                 TRASLADADO (vacío = EN FORMACION)
//
//  FORMATO SOFIA PLUS (.xls):
//    El importer usa ImportadorReporteJuicios.NormalizarReporteSofia para extraer
//    aprendices únicos del reporte. Se deduplican por número de documento (último
//    estado encontrado gana). El número de ficha del Excel se ignora: el binding
//    a la ficha viene siempre del parámetro de ruta de la pantalla.
//
//  IDEMPOTENCIA:
//    El importador es idempotente: importar el mismo archivo dos veces no duplica
//    datos. Persona y AprendizPerfil se actualizan (upsert) y FichaAprendiz
//    también se actualiza si el estado cambió.
//
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Importador masivo de aprendices desde el archivo Excel de plantilla oficial.
/// Orquesta Persona, AprendizPerfil y FichaAprendiz usando <see cref="IAprendizService"/>.
/// </summary>
public class ImportadorAprendices(
    AdsoDbContext    db,
    IAprendizService aprendizService,
    ILogger<ImportadorAprendices> log)
{
    // ── Mapa de normalización de estados ────────────────────────────────────
    // El Excel puede venir con casing variado ("EN FORMACION", "En Formacion", etc.).
    // Se normaliza siempre a la forma canónica almacenada en BD (EstadosAprendiz).
    private static readonly Dictionary<string, string> _estadoMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["EN FORMACION"]       = EstadosAprendiz.EnFormacion,
            ["CANCELADO"]          = EstadosAprendiz.Cancelado,
            ["RETIRO VOLUNTARIO"]  = EstadosAprendiz.RetiroVoluntario,
            ["TRASLADADO"]         = EstadosAprendiz.Trasladado,
        };

    // ── DTO interno de fila parseada ─────────────────────────────────────────

    /// <summary>
    /// Representación normalizada de una fila del Excel, antes de persistir.
    /// Incluye el número de fila original para mensajes de error trazables.
    /// </summary>
    private sealed record FilaAprendizDto(
        int    NumeroFila,
        string TipoDocumentoAbrev,
        string NumeroDocumento,
        string Nombres,
        string Apellidos,
        string Estado);          // ya normalizado al valor canónico

    // ══════════════════════════════════════════════════════════════════════════
    //  ORQUESTADOR
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Punto de entrada principal. Lee el archivo Excel, valida cada fila y persiste
    /// los aprendices y sus vínculos con la ficha indicada.
    /// </summary>
    /// <param name="rutaExcel">Ruta al archivo .xlsx en disco temporal.</param>
    /// <param name="numeroFicha">
    ///   Número de ficha al que se vincularán los aprendices.
    ///   Debe existir en la BD; si no se encuentra, se devuelve error global.
    ///   Viene del parámetro de ruta de la pantalla, no del Excel.
    /// </param>
    /// <returns>Resumen de la operación con contadores y lista de errores.</returns>
    public async Task<ResultadoImportacionAprendizDto> ImportarAsync(
        string rutaExcel,
        string numeroFicha)
    {
        // ExcelDataReader requiere este proveedor para archivos legados .xls;
        // para .xlsx no es estrictamente necesario pero no causa daño.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Detección de formato una sola vez — se reutiliza en el log y en Fase 1
        bool esSofia = rutaExcel.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
                       && !rutaExcel.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);

        log.LogInformation(
            "ImportadorAprendices: iniciando — ficha '{ficha}', formato '{fmt}', archivo '{ruta}'",
            numeroFicha,
            esSofia ? "SOFIA Plus (.xls)" : "Plantilla oficial (.xlsx)",
            rutaExcel);

        var res = new ResultadoImportacionAprendizDto();

        // ── Resolver la ficha destino ────────────────────────────────────────
        var ficha = await db.Ficha
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.NumeroFicha == numeroFicha);

        if (ficha is null)
        {
            // Error crítico: sin ficha no hay nada que hacer
            res.Errores.Add(
                $"La ficha con número '{numeroFicha}' no existe en el sistema. " +
                "Verifique que haya sido importada antes de cargar aprendices.");
            log.LogWarning(
                "ImportadorAprendices: ficha '{ficha}' no encontrada. Abortando.",
                numeroFicha);
            return res;
        }

        // ── Cargar catálogo TipoDocumento en memoria (evita N+1 en Fase 2) ──
        var tiposDocumento = await db.TipoDocumento
            .AsNoTracking()
            .ToDictionaryAsync(t => t.Nombre, t => t.IdTipoDocumento,
                StringComparer.OrdinalIgnoreCase);

        // ── Fase 1: Excel → DTOs (sin BD, sin efectos secundarios) ─────────
        // esSofia se calcula al inicio del método (ver arriba).
        //   .xls  → Reporte de Juicios Evaluativos SOFIA Plus
        //   .xlsx → Plantilla oficial de aprendices
        var (filasValidas, filasConError) = esSofia
            ? LeerFilasSofia(rutaExcel)
            : LeerFilas(rutaExcel);

        res.TotalFilas = filasValidas.Count + filasConError.Count;

        log.LogInformation(
            "ImportadorAprendices Fase 1 — {ok} filas válidas, {err} con error de formato",
            filasValidas.Count, filasConError.Count);

        // Registrar errores de formato detectados en Fase 1
        foreach (var (fila, error) in filasConError)
        {
            res.FilasError++;
            res.Errores.Add($"Fila {fila.NumeroFila}: {error}");
        }

        // ── Fase 2: DTOs → BD ────────────────────────────────────────────────
        await PersistirAsync(filasValidas, ficha.IdFicha, tiposDocumento, res);

        log.LogInformation(
            "ImportadorAprendices: finalizado — {c} creados, {u} actualizados, " +
            "{vc} vínculos creados, {vu} vínculos actualizados, {e} errores.",
            res.AprendicesCreados, res.AprendicesActualizados,
            res.VinculosCreados,   res.VinculosActualizados,
            res.FilasError);

        return res;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  FASE 1 — LECTURA Y NORMALIZACIÓN (sin BD)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lee todas las filas de datos del Excel (saltando encabezados) y las separa
    /// en válidas y con error de formato.
    ///
    /// Esta función es pura: no accede a la BD ni tiene efectos secundarios.
    /// Solo valida presencia de campos obligatorios y normalización del estado.
    /// La validación de existencia en catálogos (TipoDocumento) ocurre en Fase 2.
    /// </summary>
    private (List<FilaAprendizDto> Validas, List<(FilaAprendizDto Fila, string Error)> ConError)
        LeerFilas(string rutaExcel)
    {
        var validas  = new List<FilaAprendizDto>();
        var conError = new List<(FilaAprendizDto, string)>();

        using var stream = File.OpenRead(rutaExcel);

        // CreateReader auto-detecta el formato: .xls (BIFF) o .xlsx (OOXML)
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var ds = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
        });

        var tabla = ds.Tables[0];

        // Fila 0 = encabezados → empezar desde fila 1 (índice 0-based)
        for (int r = 1; r < tabla.Rows.Count; r++)
        {
            int    numFila = r + 1; // número de fila en Excel (1-based, con encabezado en 1)
            string col(int c) => (tabla.Rows[r][c]?.ToString() ?? "").Trim();

            var tipoDoc = col(0);
            var numDoc  = col(1);
            var nombres = col(2);
            var apells  = col(3);
            var estadoRaw = col(4);

            // Ignorar filas completamente vacías (comunes al final del Excel)
            if (string.IsNullOrEmpty(tipoDoc) &&
                string.IsNullOrEmpty(numDoc)  &&
                string.IsNullOrEmpty(nombres)) continue;

            // Crear DTO provisional para incluirlo en mensajes de error con contexto
            var dto = new FilaAprendizDto(numFila, tipoDoc, numDoc, nombres, apells,
                estadoRaw); // estado se reemplazará si se normaliza correctamente

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(tipoDoc))
            { conError.Add((dto, "TipoDocumento es obligatorio (ej. CC, TI, CE).")); continue; }

            if (string.IsNullOrWhiteSpace(numDoc))
            { conError.Add((dto, "NumeroDocumento es obligatorio.")); continue; }

            if (string.IsNullOrWhiteSpace(nombres))
            { conError.Add((dto, "Nombres es obligatorio.")); continue; }

            if (string.IsNullOrWhiteSpace(apells))
            { conError.Add((dto, "Apellidos es obligatorio.")); continue; }

            // Normalizar Estado: vacío → EN FORMACION; valor inválido → error
            string estadoNormalizado;
            if (string.IsNullOrWhiteSpace(estadoRaw))
            {
                estadoNormalizado = EstadosAprendiz.EnFormacion;
            }
            else if (_estadoMap.TryGetValue(estadoRaw, out var estadoMapeado))
            {
                estadoNormalizado = estadoMapeado;
            }
            else
            {
                conError.Add((dto,
                    $"Estado '{estadoRaw}' no es válido. " +
                    "Use: EN FORMACION, CANCELADO, RETIRO VOLUNTARIO o TRASLADADO."));
                continue;
            }

            validas.Add(dto with { Estado = estadoNormalizado });
        }

        return (validas, conError);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  FASE 2 — PERSISTENCIA EN BD
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Persiste aprendices y vínculos de ficha en la base de datos, fila por fila.
    ///
    /// Por cada fila válida:
    ///   1. Resolver IdTipoDocumento desde el catálogo en memoria.
    ///   2. UpsertPersonaAsync → crea o actualiza Persona.
    ///   3. UpsertAprendizPerfilAsync → crea AprendizPerfil si no existe.
    ///   4. VincularAprendizFichaAsync → crea o actualiza FichaAprendiz,
    ///      aplicando la regla de traslado automático.
    ///
    /// Los errores individuales (ej. TipoDocumento inexistente) no abortan el
    /// procesamiento del resto de las filas.
    /// </summary>
    private async Task PersistirAsync(
        List<FilaAprendizDto>       filasValidas,
        int                         idFicha,
        Dictionary<string, int>     tiposDocumento,
        ResultadoImportacionAprendizDto res)
    {
        foreach (var fila in filasValidas)
        {
            try
            {
                // ── Resolver TipoDocumento ───────────────────────────────────
                if (!tiposDocumento.TryGetValue(fila.TipoDocumentoAbrev, out int idTipoDoc))
                {
                    res.FilasError++;
                    res.Errores.Add(
                        $"Fila {fila.NumeroFila}: El tipo de documento " +
                        $"'{fila.TipoDocumentoAbrev}' no existe en el catálogo. " +
                        "Valores válidos: CC, TI, CE, PPT, PA, RC.");
                    continue;
                }

                // ── Upsert Persona ───────────────────────────────────────────
                int idPersona = await aprendizService.UpsertPersonaAsync(
                    fila.NumeroDocumento,
                    fila.Nombres,
                    fila.Apellidos,
                    idTipoDoc);

                // ── Upsert AprendizPerfil ────────────────────────────────────
                var (idAprendiz, isNew) = await aprendizService.UpsertAprendizPerfilAsync(idPersona);

                if (isNew)
                    res.AprendicesCreados++;
                else
                    res.AprendicesActualizados++;

                // ── Vincular a la ficha (con regla de traslado automático) ───
                // Detectar si el vínculo ya existía para contabilizarlo correctamente
                bool vinculoExistia = await db.FichaAprendiz
                    .AsNoTracking()
                    .AnyAsync(fa => fa.IdFicha == idFicha && fa.IdAprendiz == idAprendiz);

                await aprendizService.VincularAprendizFichaAsync(idFicha, idAprendiz, fila.Estado);

                if (vinculoExistia)
                    res.VinculosActualizados++;
                else
                    res.VinculosCreados++;

                res.FilasOk++;
            }
            catch (Exception ex)
            {
                // Error inesperado en una fila: registrar y continuar con las demás
                res.FilasError++;
                res.Errores.Add(
                    $"Fila {fila.NumeroFila} (documento {fila.NumeroDocumento}): " +
                    $"Error inesperado — {ex.Message}");

                log.LogError(ex,
                    "ImportadorAprendices: error en fila {n}, documento '{doc}'",
                    fila.NumeroFila, fila.NumeroDocumento);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  FASE 1 (VARIANTE SOFIA PLUS) — Lectura de .xls de Juicios Evaluativos
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Extrae aprendices únicos de un Reporte de Juicios Evaluativos de SOFIA Plus (.xls).
    ///
    /// Delega el parseo a <see cref="ImportadorReporteJuicios.NormalizarReporteSofia"/>,
    /// que ya replica con precisión la lógica del importer de juicios.
    ///
    /// Deduplicación: si un aprendiz aparece en múltiples filas (una por resultado),
    /// se toma su ÚLTIMO estado encontrado en el reporte.
    ///
    /// El número de ficha del Excel se ignora: el binding a la ficha destino
    /// proviene siempre del parámetro de ruta (NumeroFicha), no del archivo.
    /// </summary>
    private (List<FilaAprendizDto> Validas, List<(FilaAprendizDto Fila, string Error)> ConError)
        LeerFilasSofia(string rutaExcel)
    {
        var validas  = new List<FilaAprendizDto>();
        var conError = new List<(FilaAprendizDto, string)>();

        // NormalizarReporteSofia es pura: lee el .xls y devuelve DTOs normalizados
        List<ReporteJuicioDto> dtos;
        using (var stream = File.OpenRead(rutaExcel))
            dtos = ImportadorReporteJuicios.NormalizarReporteSofia(stream);

        if (dtos.Count == 0)
            return (validas, conError);  // archivo vacío o sin filas con juicio

        // Deduplicar por documento: un aprendiz aparece una vez por resultado × juicio.
        // Tomamos el último registro encontrado para cada documento (estado más reciente).
        var aprendicesUnicos = dtos
            .Where(d => !string.IsNullOrWhiteSpace(d.Documento))
            .GroupBy(d => d.Documento)
            .Select(g => g.Last())
            .ToList();

        for (int i = 0; i < aprendicesUnicos.Count; i++)
        {
            var dto    = aprendicesUnicos[i];
            int numFila = i + 2;   // pseudo row-number legible para mensajes de error

            // NormalizarReporteSofia ya devuelve TipoDocumento y EstadoAprendiz
            // en el formato canónico (EstadosAprendiz.* / "CC", "TI", etc.).
            var fila = new FilaAprendizDto(
                numFila,
                dto.TipoDocumento,
                dto.Documento,
                dto.Nombres,
                dto.Apellidos,
                dto.EstadoAprendiz);

            if (string.IsNullOrWhiteSpace(dto.Nombres))
            { conError.Add((fila, "Nombres vacíos en el reporte SOFIA.")); continue; }

            if (string.IsNullOrWhiteSpace(dto.Apellidos))
            { conError.Add((fila, "Apellidos vacíos en el reporte SOFIA.")); continue; }

            validas.Add(fila);
        }

        return (validas, conError);
    }
}
