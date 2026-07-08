using AdsoLabs.Application.DTOs.Patrocinio;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class PatrocinioQueryService(AdsoDbContext context) : IPatrocinioQueryService
{
    public async Task<List<PatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario)
    {
        var esAdmin = await context.Usuario
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => u.Rol.Nombre == "Administrador")
            .FirstOrDefaultAsync();

        IQueryable<Models.Patrocinio> query = context.Patrocinio
            .Include(p => p.Aprendiz).ThenInclude(a => a.Persona)
            .Include(p => p.Ficha);

        if (!esAdmin)
        {
            var idInstructor = await context.InstructorPerfil
                .Where(ip => ip.IdUsuario == idUsuario)
                .Select(ip => (int?)ip.IdInstructor)
                .FirstOrDefaultAsync();

            if (idInstructor.HasValue)
            {
                var idFichas = await context.FichaCompetenciaResultado
                    .Where(fcr => fcr.IdInstructor == idInstructor.Value)
                    .Select(fcr => fcr.FichaCompetencia.IdFicha)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(p => idFichas.Contains(p.IdFicha));
            }
        }

        var patrocinios = await query
            .OrderBy(p => p.Aprendiz.Persona.Apellidos)
            .ThenBy(p => p.Aprendiz.Persona.Nombres)
            .ToListAsync();

        if (patrocinios.Count == 0) return [];

        // ── Estadísticas de asistencia real desde Sesion/Asistencia ──────────
        var fichaIds    = patrocinios.Select(p => p.IdFicha).Distinct().ToList();
        var aprendizIds = patrocinios.Select(p => p.IdAprendiz).Distinct().ToList();

        var registros = await context.Asistencia
            .Where(a => aprendizIds.Contains(a.IdAprendiz)
                     && fichaIds.Contains(a.Sesion.FichaCompetenciaResultado.FichaCompetencia.IdFicha))
            .Select(a => new
            {
                a.IdAprendiz,
                IdFicha = a.Sesion.FichaCompetenciaResultado.FichaCompetencia.IdFicha,
                a.Estado
            })
            .ToListAsync();

        var paresValidos = new HashSet<(int, int)>(patrocinios.Select(p => (p.IdAprendiz, p.IdFicha)));

        var statsPorPar = registros
            .Where(r => paresValidos.Contains((r.IdAprendiz, r.IdFicha)))
            .GroupBy(r => (r.IdAprendiz, r.IdFicha))
            .ToDictionary(
                g => g.Key,
                g => (
                    Total:        g.Count(),
                    Presentes:    g.Count(r => r.Estado == "Presente"),
                    Justificados: g.Count(r => r.Estado == "Justificado")
                )
            );

        return patrocinios.Select(p =>
        {
            var key   = (p.IdAprendiz, p.IdFicha);
            var stats = statsPorPar.TryGetValue(key, out var s)
                ? s : (Total: 0, Presentes: 0, Justificados: 0);
            return MapearPatrocinio(p, stats.Total, stats.Presentes, stats.Justificados);
        }).ToList();
    }

    public async Task<PatrocinioDTO?> ObtenerPatrocinioAsync(int idPatrocinio)
    {
        var p = await context.Patrocinio
            .Include(p => p.Aprendiz).ThenInclude(a => a.Persona)
            .Include(p => p.Ficha)
            .FirstOrDefaultAsync(p => p.IdPatrocinio == idPatrocinio);

        if (p is null) return null;

        var registros = await context.Asistencia
            .Where(a => a.IdAprendiz == p.IdAprendiz
                     && a.Sesion.FichaCompetenciaResultado.FichaCompetencia.IdFicha == p.IdFicha)
            .Select(a => a.Estado)
            .ToListAsync();

        int total        = registros.Count;
        int presentes    = registros.Count(e => e == "Presente");
        int justificados = registros.Count(e => e == "Justificado");

        return MapearPatrocinio(p, total, presentes, justificados);
    }

    public async Task<List<AsistenciaPatrocinioDTO>> ObtenerAsistenciasAsync(int idPatrocinio)
        => await context.AsistenciaPatrocinio
            .Where(a => a.IdPatrocinio == idPatrocinio)
            .OrderByDescending(a => a.Fecha)
            .Select(a => new AsistenciaPatrocinioDTO
            {
                IdAsistenciaPatrocinio = a.IdAsistenciaPatrocinio,
                IdPatrocinio           = a.IdPatrocinio,
                Fecha                  = a.Fecha,
                Estado                 = a.Estado,
                Observacion            = a.Observacion
            })
            .ToListAsync();

    public async Task<ResumenAsistenciaDTO> ObtenerResumenAsistenciaAsync(int idAprendiz, int idFicha)
    {
        var estados = await context.Asistencia
            .Where(a => a.IdAprendiz == idAprendiz
                     && a.Sesion.FichaCompetenciaResultado.FichaCompetencia.IdFicha == idFicha)
            .Select(a => a.Estado)
            .ToListAsync();

        int total        = estados.Count;
        int presentes    = estados.Count(e => e == "Presente");
        int justificados = estados.Count(e => e == "Justificado");
        int ausentes     = estados.Count(e => e == "Ausente");
        double pct       = total > 0
            ? Math.Round((double)(presentes + justificados) / total * 100, 1)
            : 0;

        return new ResumenAsistenciaDTO
        {
            TotalSesiones        = total,
            Presentes            = presentes,
            Justificados         = justificados,
            Ausentes             = ausentes,
            PorcentajeAsistencia = pct
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static PatrocinioDTO MapearPatrocinio(
        Models.Patrocinio p, int total, int presentes, int justificados)
    {
        double pct = total > 0
            ? Math.Round((double)(presentes + justificados) / total * 100, 1)
            : 0;

        return new PatrocinioDTO
        {
            IdPatrocinio         = p.IdPatrocinio,
            IdFicha              = p.IdFicha,
            IdAprendiz           = p.IdAprendiz,
            NombreAprendiz       = $"{p.Aprendiz.Persona.Nombres} {p.Aprendiz.Persona.Apellidos}".Trim(),
            NumeroDocumento      = p.Aprendiz.Persona.NumeroDocumento,
            NumeroFicha          = p.Ficha.NumeroFicha,
            Activo               = p.Activo,
            Etapa                = p.Etapa,
            FechaInicioEtapa     = p.FechaInicioEtapa,
            FechaFinEtapa        = p.FechaFinEtapa,
            HoraInicio           = p.HoraInicio,
            HoraFin              = p.HoraFin,
            NombreEmpresa        = p.NombreEmpresa,
            ContactoEmpresa      = p.ContactoEmpresa,
            FechaRegistro        = p.FechaRegistro,
            PorcentajeAsistencia = pct
        };
    }
}
