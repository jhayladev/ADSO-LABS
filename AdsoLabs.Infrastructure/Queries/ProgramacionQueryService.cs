using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Fichas;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class ProgramacionQueryService(AdsoDbContext context) : IProgramacionQueryService
{
    public async Task<List<ResultadoFichaDTO>> ObtenerResultadosPorFichaYCompetenciaAsync(
        string numeroFicha, int idCompetencia)
    {
        // Todos los ResultadoAprendizaje para la competencia.
        // La deduplicación por Código se hace en memoria para evitar
        // problemas de traducción de EF Core con GroupBy + First().
        var resultadosRaw = await context.ResultadoAprendizaje
            .Where(ra => ra.Plan.IdCompetencia == idCompetencia)
            .Select(ra => new { ra.IdResultado, ra.Codigo, ra.Nombre })
            .ToListAsync();

        var resultadosBd = resultadosRaw
            .GroupBy(r => r.Codigo)
            .Select(g => g.OrderBy(r => r.IdResultado).First())
            .OrderBy(r => ExtraerNumeroOrden(r.Nombre))
            .ToList();

        // Programaciones ya existentes para (NumeroFicha + Competencia)
        var programados = await context.FichaCompetenciaResultado
            .Where(fcr =>
                fcr.FichaCompetencia.Ficha.NumeroFicha == numeroFicha &&
                fcr.FichaCompetencia.IdCompetencia     == idCompetencia)
            .Select(fcr => new
            {
                fcr.IdFichaCompetenciaResultado,
                fcr.IdResultado,
                fcr.IdInstructor,
                NombreInstructor = fcr.Instructor != null
                    ? fcr.Instructor.Usuario.Persona.Nombres + " " + fcr.Instructor.Usuario.Persona.Apellidos
                    : "",
                fcr.FechaInicio,
                fcr.FechaFin,
                fcr.HoraInicio,
                fcr.HoraFin,
                fcr.HorasProgramadas,
                fcr.Estado
            })
            .ToListAsync();

        // ── Juicios evaluativos ───────────────────────────────────────────────
        // Aprendices EN FORMACION en esta ficha (son los que participan en juicios).
        var aprendicesActivos = await context.FichaAprendiz
            .Where(fa => fa.Ficha.NumeroFicha == numeroFicha
                      && fa.Estado            == EstadosAprendiz.EnFormacion)
            .Select(fa => new
            {
                fa.IdAprendiz,
                Nombre = fa.Aprendiz.Persona.Nombres + " " + fa.Aprendiz.Persona.Apellidos
            })
            .OrderBy(a => a.Nombre)
            .ToListAsync();

        // Todos los juicios APROBADO para los RAs de esta competencia.
        // La ausencia de registro equivale a "POR EVALUAR".
        var idResultados  = resultadosBd.Select(r => r.IdResultado).ToList();
        var juiciosEnBd   = await context.JuicioResultado
            .Where(j => idResultados.Contains(j.IdResultado))
            .Select(j => new { j.IdResultado, j.IdAprendiz, j.Juicio })
            .ToListAsync();

        // ── Construcción del resultado ────────────────────────────────────────
        return resultadosBd.Select(r =>
        {
            var p = programados.FirstOrDefault(x => x.IdResultado == r.IdResultado);

            // Mapa IdAprendiz → Juicio para este RA (registros reales en JuicioResultado).
            var mapaJuicios = juiciosEnBd
                .Where(j => j.IdResultado == r.IdResultado)
                .ToDictionary(j => j.IdAprendiz, j => j.Juicio);

            // Lista completa de aprendices activos; los sin registro muestran "Por Evaluar" como default.
            var listaJuicios = aprendicesActivos
                .Select(a => new JuicioAprendizDTO
                {
                    IdAprendiz     = a.IdAprendiz,
                    NombreCompleto = a.Nombre,
                    Juicio         = mapaJuicios.TryGetValue(a.IdAprendiz, out var j)
                                        ? j : EstadosJuicio.PorEvaluar
                })
                .ToList();

            // Contamos solo sobre aprendices EN FORMACION para evitar que cancelados/retirados
            // inflen el numerador (sus juicios siguen en BD pero ya no forman parte de la ficha).
            int aprobados = listaJuicios.Count(j => j.Juicio == EstadosJuicio.Aprobado);
            // JuiciosCargados = cuántos activos tienen registro real en BD para este resultado.
            // El pill solo se muestra si > 0 (la evaluación ya comenzó para al menos uno).
            int cargados = aprendicesActivos.Count(a => mapaJuicios.ContainsKey(a.IdAprendiz));

            return new ResultadoFichaDTO
            {
                IdFichaCompetenciaResultado = p?.IdFichaCompetenciaResultado ?? 0,
                IdResultado                 = r.IdResultado,
                Codigo                      = r.Codigo,
                Nombre                      = r.Nombre,
                IdInstructor                = p?.IdInstructor,
                NombreInstructor            = p?.NombreInstructor ?? "",
                FechaInicio                 = p?.FechaInicio,
                FechaFin                    = p?.FechaFin,
                HoraInicio                  = p?.HoraInicio,
                HoraFin                     = p?.HoraFin,
                HorasProgramadas            = p?.HorasProgramadas,
                Estado                      = p?.Estado ?? "Pendiente",
                Juicios                     = listaJuicios,
                JuiciosAprobados            = aprobados,
                TotalJuicios                = listaJuicios.Count,  // siempre el total de activos (18)
                JuiciosCargados             = cargados
            };
        }).ToList();
    }

    /// <summary>
    /// Extrae el número de orden desde el Nombre del resultado de aprendizaje.
    /// Formato real SENA: "593162 - 01 IDENTIFICAR LOS PRINCIPIOS..."
    ///                               ^^^ este es el número de orden
    /// Busca " - " y toma los dígitos contiguos que le siguen.
    /// Si no puede parsear devuelve 0 (el resultado queda al final).
    /// </summary>
    private static int ExtraerNumeroOrden(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return 0;

        int idx = nombre.IndexOf(" - ", StringComparison.Ordinal);
        if (idx < 0) return 0;

        // Tomar solo los dígitos contiguos después de " - "
        // "01 IDENTIFICAR..." → "01" → 1
        var resto  = nombre[(idx + 3)..].TrimStart();
        var numStr = new string(resto.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(numStr, out int n) ? n : 0;
    }

    public async Task<List<InstructorSelectDTO>> ObtenerInstructoresActivosAsync()
        => await context.InstructorPerfil
            .Where(i => i.Activo)
            .Select(i => new InstructorSelectDTO
            {
                IdInstructor = i.IdInstructor,
                Nombres      = i.Usuario.Persona.Nombres + " " + i.Usuario.Persona.Apellidos
            })
            .OrderBy(i => i.Nombres)
            .ToListAsync();
}
