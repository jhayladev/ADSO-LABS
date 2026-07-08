using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Asistencia;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class AsistenciaQueryService(AdsoDbContext context) : IAsistenciaQueryService
{
    // ── Resultados programados de la ficha ────────────────────────────────────
    public async Task<List<ResultadoProgramadoDTO>> ObtenerResultadosProgramadosAsync(
        string numeroFicha)
    {
        return await context.FichaCompetenciaResultado
            .Where(fcr =>
                fcr.FichaCompetencia.Ficha.NumeroFicha == numeroFicha &&
                fcr.Estado != "Pendiente"              &&
                fcr.FechaInicio != null                &&
                fcr.FechaFin    != null                &&
                fcr.HoraInicio  != null                &&
                fcr.HoraFin     != null                &&
                fcr.IdInstructor != null)
            .Select(fcr => new ResultadoProgramadoDTO
            {
                IdFichaCompetenciaResultado = fcr.IdFichaCompetenciaResultado,
                IdFichaCompetencia          = fcr.IdFichaCompetencia,
                NombreCompetencia           = fcr.FichaCompetencia.Competencia.Nombre,
                NombreResultado             = fcr.Resultado.Nombre,
                FechaInicio                 = fcr.FechaInicio!.Value,
                FechaFin                    = fcr.FechaFin!.Value,
                HoraInicio                  = fcr.HoraInicio!.Value,
                HoraFin                     = fcr.HoraFin!.Value,
                IdInstructor                = fcr.IdInstructor!.Value,
                NombreInstructor            = fcr.Instructor!.Usuario.Persona.Nombres + " "
                                            + fcr.Instructor.Usuario.Persona.Apellidos,
                EstadoResultado             = fcr.Estado
            })
            .OrderBy(r => r.NombreCompetencia)
            .ThenBy(r => r.FechaInicio)
            .ToListAsync();
    }

    // ── Resultado específico por ID ───────────────────────────────────────────
    public async Task<ResultadoProgramadoDTO?> ObtenerResultadoProgramadoAsync(
        int idFichaCompetenciaResultado)
    {
        var dto = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdFichaCompetenciaResultado == idFichaCompetenciaResultado
                       && fcr.FechaInicio != null
                       && fcr.FechaFin    != null
                       && fcr.HoraInicio  != null
                       && fcr.HoraFin     != null
                       && fcr.IdInstructor != null)
            .Select(fcr => new ResultadoProgramadoDTO
            {
                IdFichaCompetenciaResultado = fcr.IdFichaCompetenciaResultado,
                IdFichaCompetencia          = fcr.IdFichaCompetencia,
                NombreCompetencia           = fcr.FichaCompetencia.Competencia.Nombre,
                NombreResultado             = fcr.Resultado.Nombre,
                FechaInicio                 = fcr.FechaInicio!.Value,
                FechaFin                    = fcr.FechaFin!.Value,
                HoraInicio                  = fcr.HoraInicio!.Value,
                HoraFin                     = fcr.HoraFin!.Value,
                IdInstructor                = fcr.IdInstructor!.Value,
                NombreInstructor            = fcr.Instructor!.Usuario.Persona.Nombres + " "
                                            + fcr.Instructor.Usuario.Persona.Apellidos,
                EstadoResultado             = fcr.Estado,
                HorasProgramadas            = fcr.HorasProgramadas
            })
            .FirstOrDefaultAsync();

        if (dto is not null)
        {
            // HorasEjecutadas: suma de duración de sesiones no canceladas (en memoria para evitar
            // problemas de traducción de TimeOnly en SQL Server)
            var sesiones = await context.Sesion
                .Where(s => s.IdFichaCompetenciaResultado == idFichaCompetenciaResultado
                         && s.Estado != EstadosSesion.Cancelada)
                .Select(s => new { s.HoraInicio, s.HoraFin })
                .ToListAsync();

            dto.HorasEjecutadas = sesiones
                .Sum(s => (s.HoraFin.ToTimeSpan() - s.HoraInicio.ToTimeSpan()).TotalHours);
        }

        return dto;
    }

    // ── Sesiones de un resultado ──────────────────────────────────────────────
    public async Task<List<SesionDTO>> ObtenerSesionesPorResultadoAsync(
        int idFichaCompetenciaResultado)
    {
        // Necesitamos IdFichaCompetencia para buscar sesiones
        var idFC = await context.FichaCompetenciaResultado
            .Where(r => r.IdFichaCompetenciaResultado == idFichaCompetenciaResultado)
            .Select(r => r.IdFichaCompetencia)
            .FirstOrDefaultAsync();

        if (idFC == 0) return [];

        // Total de aprendices EN FORMACION en la ficha (para mostrar % asistencia)
        var idFicha = await context.FichaCompetencia
            .Where(fc => fc.IdFichaCompetencia == idFC)
            .Select(fc => fc.IdFicha)
            .FirstOrDefaultAsync();

        var totalAprendices = await context.FichaAprendiz
            .CountAsync(fa => fa.IdFicha == idFicha && fa.Estado == EstadosAprendiz.EnFormacion);

        var sesiones = await context.Sesion
            .Where(s => s.IdFichaCompetenciaResultado == idFichaCompetenciaResultado)
            .OrderByDescending(s => s.Fecha)
            .Select(s => new
            {
                s.IdSesion,
                s.Fecha,
                s.HoraInicio,
                s.HoraFin,
                s.Estado,
                Presentes    = s.Asistencias.Count(a => a.Estado == "Presente"),
                Tardes       = s.Asistencias.Count(a => a.Estado == "Tarde"),
                Ausentes     = s.Asistencias.Count(a => a.Estado == "Ausente"),
                Justificados = s.Asistencias.Count(a => a.Estado == "Justificado")
            })
            .ToListAsync();

        return sesiones.Select(s => new SesionDTO
        {
            IdSesion        = s.IdSesion,
            Fecha           = s.Fecha,
            HoraInicio      = s.HoraInicio,
            HoraFin         = s.HoraFin,
            Estado          = s.Estado,
            TotalAprendices = totalAprendices,
            Presentes       = s.Presentes,
            Tardes          = s.Tardes,
            Ausentes        = s.Ausentes,
            Justificados    = s.Justificados
        }).ToList();
    }

    // ── Cabecera de sesión ────────────────────────────────────────────────────
    public async Task<SesionDTO?> ObtenerInfoSesionAsync(int idSesion)
    {
        return await context.Sesion
            .Where(s => s.IdSesion == idSesion)
            .Select(s => new SesionDTO
            {
                IdSesion        = s.IdSesion,
                Fecha           = s.Fecha,
                HoraInicio      = s.HoraInicio,
                HoraFin         = s.HoraFin,
                Estado          = s.Estado,
                TotalAprendices = 0
            })
            .FirstOrDefaultAsync();
    }

    // ── Aprendices con asistencia para una sesión ─────────────────────────────
    public async Task<List<AprendizAsistenciaDTO>> ObtenerAprendicesSesionAsync(int idSesion)
    {
        // Resolver ficha desde la sesión
        var sesionInfo = await context.Sesion
            .Where(s => s.IdSesion == idSesion)
            .Select(s => new { s.IdFichaCompetencia })
            .FirstOrDefaultAsync();

        if (sesionInfo is null) return [];

        var idFicha = await context.FichaCompetencia
            .Where(fc => fc.IdFichaCompetencia == sesionInfo.IdFichaCompetencia)
            .Select(fc => fc.IdFicha)
            .FirstOrDefaultAsync();

        // Aprendices EN FORMACION
        var aprendices = await context.FichaAprendiz
            .Where(fa => fa.IdFicha == idFicha && fa.Estado == EstadosAprendiz.EnFormacion)
            .Select(fa => new
            {
                fa.IdAprendiz,
                NombreCompleto  = fa.Aprendiz.Persona.Nombres + " " + fa.Aprendiz.Persona.Apellidos,
                NumeroDocumento = fa.Aprendiz.Persona.NumeroDocumento
            })
            .OrderBy(a => a.NombreCompleto)
            .ToListAsync();

        // Asistencias existentes para esta sesión
        var mapaAsistencia = await context.Asistencia
            .Where(a => a.IdSesion == idSesion)
            .ToDictionaryAsync(
                a => a.IdAprendiz,
                a => (a.Estado, a.MotivoInasistencia));

        return aprendices.Select(a =>
        {
            mapaAsistencia.TryGetValue(a.IdAprendiz, out var reg);
            return new AprendizAsistenciaDTO
            {
                IdAprendiz         = a.IdAprendiz,
                NombreCompleto     = a.NombreCompleto,
                NumeroDocumento    = a.NumeroDocumento,
                Estado             = reg.Estado,
                MotivoInasistencia = reg.MotivoInasistencia
            };
        }).ToList();
    }
}
