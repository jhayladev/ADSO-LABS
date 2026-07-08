using AdsoLabs.Application.DTOs.Importacion;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace AdsoLabs.Infrastructure.Services;

// ═══════════════════════════════════════════════════════════════════════════════
//
//  ImportadorFichas  —  Importador masivo de Fichas desde plantilla Excel
//
//  RESPONSABILIDADES (lo que SÍ hace):
//    ✅ Leer el archivo .xlsx usando la plantilla oficial (PlantillaFichasGenerator).
//    ✅ Crear fichas nuevas si NumeroFicha no existe aún en el sistema.
//    ✅ Actualizar Nombre, FechaInicio y FechaFin de fichas ya existentes.
//    ✅ Vincular automáticamente TODAS las competencias no clausuradas del catálogo
//       a cada ficha nueva creada (idempotente: no duplica vínculos existentes).
//    ✅ Devolver un resumen detallado con filas OK, errores y mensajes de diagnóstico.
//
//  RESTRICCIONES (lo que NO hace):
//    ❌ NO requiere que el Excel incluya códigos de competencia: todas las fichas
//       del programa ADSO comparten el mismo catálogo de competencias.
//    ❌ NO elimina competencias ya vinculadas a una ficha existente.
//    ❌ NO modifica FichaCompetencias que ya están en estado Vista o Programada.
//    ❌ NO crea aprendices ni los vincula a la ficha.
//    ❌ NO registra trazabilidad en ImportacionArchivo (sin requisito de historial aún).
//
//  FORMATO DE LA PLANTILLA:
//    Fila 1:  Encabezados (se salta)
//    Fila 2+: Una ficha por fila
//    Columna A: NumeroFicha  (string, obligatorio)
//    Columna B: Jornada      (Mañana | Tarde, obligatorio)
//    Columna C: FechaInicio  (DD/MM/AAAA, obligatorio)
//    Columna D: FechaFin     (DD/MM/AAAA, opcional — vacío = En Curso)
//
//  El campo Nombre se completa automáticamente con "ANALISIS Y DESARROLLO DE SOFTWARE"
//  para todas las fichas: no es necesario que el usuario lo escriba en el Excel.
//
//  IDEMPOTENCIA:
//    El importador es idempotente: importar el mismo archivo dos veces no duplica
//    datos. Las fichas existentes se actualizan y los vínculos ya existentes se omiten.
//
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Importador masivo de fichas desde el archivo Excel de plantilla oficial.
/// Soporta creación de fichas nuevas y actualización de fichas existentes.
/// </summary>
public class ImportadorFichas(AdsoDbContext db, ILogger<ImportadorFichas> log)
{
    // ── DTO interno de fila parseada ─────────────────────────────────────────

    // Nombre fijo del programa para todas las fichas importadas.
    private const string NombrePrograma = "ANALISIS Y DESARROLLO DE SOFTWARE";

    // Valores válidos de jornada (comparación case-insensitive en Fase 1).
    private static readonly HashSet<string> _jornadasValidas =
        new(StringComparer.OrdinalIgnoreCase) { "Mañana", "Tarde" };

    // Valores válidos de modalidad (comparación case-insensitive en Fase 1).
    private static readonly HashSet<string> _modalidadesValidas =
        new(StringComparer.OrdinalIgnoreCase) { "Presencial", "Virtual" };

    /// <summary>
    /// Representación normalizada de una fila del Excel, antes de persistir.
    /// Incluye el número de fila original para mensajes de error trazables.
    /// </summary>
    private sealed record FilaFichaDto(
        int     NumeroFila,
        string  NumeroFicha,
        string  Jornada,        // ya normalizado a "Mañana" o "Tarde"
        string  Modalidad,      // ya normalizado a "Presencial" o "Virtual"
        string  FechaInicioRaw,
        string  FechaFinRaw);

    // ══════════════════════════════════════════════════════════════════════════
    //  ORQUESTADOR
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Punto de entrada principal. Lee el archivo Excel, valida cada fila y persiste
    /// las fichas y competencias en la base de datos.
    /// </summary>
    /// <param name="rutaExcel">Ruta al archivo .xlsx en disco temporal.</param>
    /// <returns>Resumen de la operación con contadores y lista de errores.</returns>
    public async Task<ResultadoImportacionFichaDto> ImportarAsync(string rutaExcel)
    {
        // ExcelDataReader requiere este proveedor para archivos legados .xls;
        // para .xlsx no es estrictamente necesario pero no causa daño.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        log.LogInformation("ImportadorFichas: iniciando lectura de '{ruta}'", rutaExcel);

        // ── Fase 1: Excel → DTOs (sin BD, sin efectos secundarios) ─────────
        var (filasValidas, filasConError) = LeerFilas(rutaExcel);

        log.LogInformation(
            "ImportadorFichas Fase 1 — {ok} filas válidas, {err} con error de formato",
            filasValidas.Count, filasConError.Count);

        // ── Fase 2: DTOs → BD (con validación de catálogos) ─────────────────
        return await PersistirAsync(filasValidas, filasConError);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  FASE 1 — LECTURA Y NORMALIZACIÓN (sin BD)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Lee todas las filas de datos del Excel (saltando encabezados) y las separa
    /// en válidas y con error de formato.
    ///
    /// Esta función es pura: no accede a la BD ni tiene efectos secundarios.
    /// Solo valida que los campos obligatorios (A, B, C) estén presentes.
    /// </summary>
    private (List<FilaFichaDto> Validas, List<(FilaFichaDto Fila, string Error)> ConError)
        LeerFilas(string rutaExcel)
    {
        var validas   = new List<FilaFichaDto>();
        var conError  = new List<(FilaFichaDto, string)>();

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

            var numFicha  = col(0);
            var jornada   = col(1);
            var fechaIni  = col(2);
            var fechaFin  = col(3);
            var modalidad = col(4);

            // Ignorar filas completamente vacías (comunes al final del Excel)
            if (string.IsNullOrEmpty(numFicha) && string.IsNullOrEmpty(jornada)) continue;

            var dto = new FilaFichaDto(numFila, numFicha, jornada, modalidad, fechaIni, fechaFin);

            // Validar presencia de campos obligatorios
            if (string.IsNullOrWhiteSpace(numFicha))
            { conError.Add((dto, "NumeroFicha es obligatorio.")); continue; }

            if (string.IsNullOrWhiteSpace(jornada))
            { conError.Add((dto, "Jornada es obligatoria (Mañana / Tarde).")); continue; }

            if (!_jornadasValidas.Contains(jornada))
            { conError.Add((dto, $"Jornada '{jornada}' no es válida. Use: Mañana o Tarde.")); continue; }

            if (string.IsNullOrWhiteSpace(fechaIni))
            { conError.Add((dto, "FechaInicio es obligatoria.")); continue; }

            if (string.IsNullOrWhiteSpace(modalidad))
            { conError.Add((dto, "Modalidad es obligatoria (Presencial / Virtual).")); continue; }

            if (!_modalidadesValidas.Contains(modalidad))
            { conError.Add((dto, $"Modalidad '{modalidad}' no es válida. Use: Presencial o Virtual.")); continue; }

            // Normalizar capitalización
            var jornadaNorm   = char.ToUpperInvariant(jornada[0])   + jornada[1..].ToLowerInvariant();
            var modalidadNorm = char.ToUpperInvariant(modalidad[0]) + modalidad[1..].ToLowerInvariant();
            validas.Add(dto with { Jornada = jornadaNorm, Modalidad = modalidadNorm });
        }

        return (validas, conError);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  FASE 2 — PERSISTENCIA EN BD
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Persiste fichas en la base de datos y vincula automáticamente todas las
    /// competencias no clausuradas del catálogo.
    ///
    /// Estrategia por fila:
    ///   1. Parsear FechaInicio (y FechaFin opcional).
    ///   2. Upsert de la ficha (crear o actualizar Nombre/Fechas).
    ///   3. Para cada competencia activa del catálogo, crear FichaCompetencia
    ///      si el vínculo no existe (idempotente).
    ///
    /// El Excel ya no incluye columnas de competencia: todas las fichas del
    /// programa ADSO comparten el mismo catálogo.
    /// </summary>
    private async Task<ResultadoImportacionFichaDto> PersistirAsync(
        List<FilaFichaDto>                         filasValidas,
        List<(FilaFichaDto Fila, string Error)>    filasConError)
    {
        var res = new ResultadoImportacionFichaDto
        {
            TotalFilas = filasValidas.Count + filasConError.Count
        };

        // Registrar errores de formato ya detectados en Fase 1
        foreach (var (fila, error) in filasConError)
        {
            res.FilasError++;
            res.Errores.Add($"Fila {fila.NumeroFila}: {error}");
        }

        // Cargar catálogo de competencias activas una sola vez para todas las filas.
        // Se excluye únicamente Estado = "Clausurada" (retiradas del plan, CLAUDE.md §7.3).
        // Practica e Induccion son competencias válidas de la ficha y se vinculan normalmente.
        var competenciasActivas = await db.Competencia
            .AsNoTracking()
            .Where(c => c.Estado != "Clausurada")
            .ToListAsync();

        foreach (var fila in filasValidas)
        {
            // ── Parsear FechaInicio ──────────────────────────────────────────
            if (!DateTime.TryParse(fila.FechaInicioRaw, out var fechaInicioDt))
            {
                res.FilasError++;
                res.Errores.Add(
                    $"Fila {fila.NumeroFila}: FechaInicio '{fila.FechaInicioRaw}' inválida.");
                continue;
            }

            DateOnly fechaInicio = DateOnly.FromDateTime(fechaInicioDt);

            // ── Parsear FechaFin (opcional) ──────────────────────────────────
            DateOnly? fechaFin = null;

            if (!string.IsNullOrWhiteSpace(fila.FechaFinRaw))
            {
                if (!DateTime.TryParse(fila.FechaFinRaw, out var fechaFinDt))
                {
                    res.FilasError++;
                    res.Errores.Add(
                        $"Fila {fila.NumeroFila}: FechaFin '{fila.FechaFinRaw}' inválida.");
                    continue;
                }

                fechaFin = DateOnly.FromDateTime(fechaFinDt);
            }

            // ── Upsert de Ficha ──────────────────────────────────────────────
            var fichaExistente = await db.Ficha
                .FirstOrDefaultAsync(f => f.NumeroFicha == fila.NumeroFicha);

            if (fichaExistente is null)
            {
                fichaExistente = new Ficha
                {
                    NumeroFicha   = fila.NumeroFicha,
                    Nombre        = NombrePrograma,
                    Jornada       = fila.Jornada,
                    Modalidad     = fila.Modalidad,
                    FechaInicio   = fechaInicio,
                    FechaFin      = fechaFin,
                    Estado        = "Activa",
                    FechaCreacion = DateTime.UtcNow
                };
                db.Ficha.Add(fichaExistente);
                await db.SaveChangesAsync();

                res.FichasCreadas++;
                log.LogInformation("ImportadorFichas: ficha '{n}' ({j}) creada.",
                    fila.NumeroFicha, fila.Jornada);
            }
            else
            {
                // Ficha existente: actualizar Jornada, Modalidad y Fechas.
                // Nombre siempre es el del programa; Estado y FechaCreacion no se tocan.
                fichaExistente.Jornada     = fila.Jornada;
                fichaExistente.Modalidad   = fila.Modalidad;
                fichaExistente.FechaInicio = fechaInicio;
                fichaExistente.FechaFin    = fechaFin;
                await db.SaveChangesAsync();

                res.FichasActualizadas++;
                log.LogInformation("ImportadorFichas: ficha '{n}' ({j}) actualizada.",
                    fila.NumeroFicha, fila.Jornada);
            }

            if (!res.FichasAfectadas.Contains(fila.NumeroFicha))
                res.FichasAfectadas.Add(fila.NumeroFicha);

            // ── Auto-vincular todas las competencias activas del catálogo ────
            // Obtener los IdCompetencia ya vinculados a esta ficha para evitar N
            // consultas AnyAsync dentro del bucle.
            var yaVinculadas = await db.FichaCompetencia
                .Where(fc => fc.IdFicha == fichaExistente.IdFicha)
                .Select(fc => fc.IdCompetencia)
                .ToHashSetAsync();

            foreach (var comp in competenciasActivas)
            {
                if (yaVinculadas.Contains(comp.IdCompetencia)) continue;

                db.FichaCompetencia.Add(new FichaCompetencia
                {
                    IdFicha         = fichaExistente.IdFicha,
                    IdCompetencia   = comp.IdCompetencia,
                    TotalHoras      = null, // usa HorasAsignadas del catálogo (CLAUDE.md §7.3)
                    Estado          = "Pendiente",
                    HorasEjecutadas = 0
                });
                res.CompetenciasVinculadas++;
            }

            await db.SaveChangesAsync();

            res.FilasOk++;
        }

        log.LogInformation(
            "ImportadorFichas: finalizado — {c} creadas, {u} actualizadas, " +
            "{v} competencias vinculadas, {e} errores.",
            res.FichasCreadas, res.FichasActualizadas,
            res.CompetenciasVinculadas, res.FilasError);

        return res;
    }
}
