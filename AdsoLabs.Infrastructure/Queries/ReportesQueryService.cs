using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Reportes;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class ReportesQueryService(AdsoDbContext context) : IReportesQueryService
{
    // ─────────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private Task<int?> ResolverIdInstructorAsync(int idUsuario) =>
        context.InstructorPerfil
            .Where(ip => ip.IdUsuario == idUsuario)
            .Select(ip => (int?)ip.IdInstructor)
            .FirstOrDefaultAsync();

    private async Task<bool> EsAdministradorAsync(int idUsuario) =>
        await context.Usuario
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => u.Rol.Nombre == "Administrador")
            .FirstOrDefaultAsync();

    private async Task<List<int>> ObtenerIdFichasParaUsuarioAsync(int idUsuario)
    {
        // Administradores siempre ven todas las fichas, incluso si tienen InstructorPerfil.
        if (await EsAdministradorAsync(idUsuario))
            return await context.Ficha
                .Select(f => f.IdFicha)
                .ToListAsync();

        var idInstructor = await ResolverIdInstructorAsync(idUsuario);

        return idInstructor.HasValue
            ? await context.FichaCompetenciaResultado
                .Where(fcr => fcr.IdInstructor == idInstructor.Value)
                .Select(fcr => fcr.FichaCompetencia.IdFicha)
                .Distinct()
                .ToListAsync()
            : await context.Ficha
                .Select(f => f.IdFicha)
                .ToListAsync();
    }

    private async Task<bool> TieneAccesoFichaAsync(string numeroFicha, int idUsuario)
    {
        // Administradores tienen acceso a cualquier ficha.
        if (await EsAdministradorAsync(idUsuario)) return true;

        var idInstructor = await ResolverIdInstructorAsync(idUsuario);
        if (!idInstructor.HasValue) return true; // sin InstructorPerfil → acceso completo

        return await context.FichaCompetenciaResultado
            .AnyAsync(fcr => fcr.IdInstructor == idInstructor.Value
                          && fcr.FichaCompetencia.Ficha.NumeroFicha == numeroFicha);
    }

    private static string NombreInstructor(AdsoLabs.Infrastructure.Models.InstructorPerfil ip) =>
        $"{ip.Usuario.Persona.Nombres} {ip.Usuario.Persona.Apellidos}".Trim();

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte E — Resumen global de fichas
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<List<ReporteGlobalFichaDTO>> ObtenerResumenGlobalAsync(int idUsuario)
    {
        var idFichas = await ObtenerIdFichasParaUsuarioAsync(idUsuario);

        var fichas = await context.Ficha
            .Include(f => f.FichasCompetencias)
                .ThenInclude(fc => fc.Competencia)
            .Where(f => idFichas.Contains(f.IdFicha))
            .ToListAsync();

        var resultado = new List<ReporteGlobalFichaDTO>();

        foreach (var ficha in fichas)
        {
            var aprendicesFicha = await context.FichaAprendiz
                .Where(fa => fa.IdFicha == ficha.IdFicha)
                .ToListAsync();

            int totalAprendices   = aprendicesFicha.Count;
            int aprendicesActivos = aprendicesFicha.Count(fa => fa.Estado == "EN FORMACION");

            var asistencias = await context.Asistencia
                .Include(a => a.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia)
                .Where(a => a.Sesion.FichaCompetencia.IdFicha == ficha.IdFicha)
                .ToListAsync();

            int totalAsist = asistencias.Count;
            int presentes  = asistencias.Count(a => a.Estado == EstadosAsistencia.Presente
                                                  || a.Estado == EstadosAsistencia.Justificado);
            double pctAsist = totalAsist > 0 ? Math.Round((double)presentes / totalAsist * 100, 1) : 0;

            int enRiesgo = asistencias
                .GroupBy(a => a.IdAprendiz)
                .Count(g => AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(g));

            int totalHoras = ficha.FichasCompetencias.Sum(fc => fc.TotalHoras ?? fc.Competencia.HorasAsignadas);
            int horasEjec  = ficha.FichasCompetencias.Sum(fc => fc.HorasEjecutadas);
            int progreso   = totalHoras > 0 ? (int)Math.Round((double)horasEjec / totalHoras * 100) : 0;

            resultado.Add(new ReporteGlobalFichaDTO
            {
                NumeroFicha            = ficha.NumeroFicha,
                Estado                 = ficha.Estado,
                TotalAprendices        = totalAprendices,
                AprendicesActivos      = aprendicesActivos,
                AprendicesEnRiesgo     = enRiesgo,
                PorcentajeAsistencia   = pctAsist,
                ProgresoGeneral        = progreso,
                CompetenciasVista      = ficha.FichasCompetencias.Count(fc => fc.Estado == EstadosCompetencia.Vista),
                CompetenciasEnCurso    = ficha.FichasCompetencias.Count(fc => fc.Estado == EstadosCompetencia.EnCurso),
                CompetenciasPendientes = ficha.FichasCompetencias.Count(fc => fc.Estado == EstadosCompetencia.Pendiente)
            });
        }

        return resultado.OrderBy(r => r.NumeroFicha).ToList();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte B — Asistencia por ficha
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<ReporteAsistenciaFichaDTO?> ObtenerAsistenciaFichaAsync(string numeroFicha, int idUsuario)
    {
        if (!await TieneAccesoFichaAsync(numeroFicha, idUsuario)) return null;

        var ficha = await context.Ficha.FirstOrDefaultAsync(f => f.NumeroFicha == numeroFicha);
        if (ficha is null) return null;

        var aprendices = await context.FichaAprendiz
            .Include(fa => fa.Aprendiz)
                .ThenInclude(a => a.Persona)
            .Where(fa => fa.IdFicha == ficha.IdFicha && fa.Estado == "EN FORMACION")
            .ToListAsync();

        var idAprendices = aprendices.Select(fa => fa.IdAprendiz).ToList();

        var asistencias = await context.Asistencia
            .Include(a => a.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia)
            .Where(a => a.Sesion.FichaCompetencia.IdFicha == ficha.IdFicha
                     && idAprendices.Contains(a.IdAprendiz))
            .ToListAsync();

        var filas = aprendices.Select(fa =>
        {
            var asis     = asistencias.Where(a => a.IdAprendiz == fa.IdAprendiz).ToList();
            int total    = asis.Count;
            int present  = asis.Count(a => a.Estado == EstadosAsistencia.Presente);
            int justif   = asis.Count(a => a.Estado == EstadosAsistencia.Justificado);
            int ausent   = asis.Count(a => a.Estado == EstadosAsistencia.Ausente);
            double pct   = total > 0 ? Math.Round((double)(present + justif) / total * 100, 1) : 0;

            return new ReporteAsistenciaAprendizFila
            {
                NombreCompleto       = $"{fa.Aprendiz.Persona.Nombres} {fa.Aprendiz.Persona.Apellidos}".Trim(),
                NumeroDocumento      = fa.Aprendiz.Persona.NumeroDocumento,
                Estado               = fa.Estado,
                TotalSesiones        = total,
                Presentes            = present,
                Justificados         = justif,
                Ausentes             = ausent,
                PorcentajeAsistencia = pct,
                EnRiesgo             = AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(asis)
            };
        }).OrderBy(f => f.NombreCompleto).ToList();

        return new ReporteAsistenciaFichaDTO { NumeroFicha = numeroFicha, Aprendices = filas };
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte C — Progreso de competencias por ficha
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<ReporteProgresoCompetenciasDTO?> ObtenerProgresoCompetenciasAsync(string numeroFicha, int idUsuario)
    {
        if (!await TieneAccesoFichaAsync(numeroFicha, idUsuario)) return null;

        var fcs = await context.FichaCompetencia
            .Include(fc => fc.Competencia)
            .Include(fc => fc.Ficha)
            .Include(fc => fc.ResultadosProgramados)
                .ThenInclude(fcr => fcr.Instructor)
                    .ThenInclude(ip => ip!.Usuario)
                        .ThenInclude(u => u.Persona)
            .Where(fc => fc.Ficha.NumeroFicha == numeroFicha)
            .OrderBy(fc => fc.Competencia.Nombre)
            .ToListAsync();

        if (!fcs.Any()) return null;

        var filas = fcs.Select(fc =>
        {
            int horasPlaneadas = fc.TotalHoras ?? fc.Competencia.HorasAsignadas;
            int progreso = horasPlaneadas > 0
                ? (int)Math.Round((double)fc.HorasEjecutadas / horasPlaneadas * 100)
                : 0;

            var instructor = fc.ResultadosProgramados
                .FirstOrDefault(fcr => fcr.Instructor is not null)?.Instructor;

            return new ReporteCompetenciaFichaFila
            {
                CodigoCompetencia  = fc.Competencia.Codigo,
                NombreCompetencia  = fc.Competencia.Nombre,
                Estado             = fc.Estado,
                NombreInstructor   = instructor is not null ? NombreInstructor(instructor) : null,
                HorasPlaneadas     = horasPlaneadas,
                HorasEjecutadas    = fc.HorasEjecutadas,
                PorcentajeProgreso = progreso
            };
        }).ToList();

        return new ReporteProgresoCompetenciasDTO { NumeroFicha = numeroFicha, Competencias = filas };
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte D — Juicios evaluativos por ficha
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<ReporteJuiciosDTO?> ObtenerJuiciosAsync(string numeroFicha, int idUsuario)
    {
        if (!await TieneAccesoFichaAsync(numeroFicha, idUsuario)) return null;

        var ficha = await context.Ficha.FirstOrDefaultAsync(f => f.NumeroFicha == numeroFicha);
        if (ficha is null) return null;

        var resultadosFicha = await context.FichaCompetenciaResultado
            .Include(fcr => fcr.Resultado)
                .ThenInclude(r => r.Plan)
                    .ThenInclude(p => p.Competencia)
            .Where(fcr => fcr.FichaCompetencia.IdFicha == ficha.IdFicha)
            .Select(fcr => fcr.Resultado)
            .Distinct()
            .ToListAsync();

        if (!resultadosFicha.Any())
            return new ReporteJuiciosDTO { NumeroFicha = numeroFicha };

        var idResultados = resultadosFicha.Select(r => r.IdResultado).ToList();

        var aprendices = await context.FichaAprendiz
            .Include(fa => fa.Aprendiz)
                .ThenInclude(a => a.Persona)
            .Where(fa => fa.IdFicha == ficha.IdFicha && fa.Estado == "EN FORMACION")
            .ToListAsync();

        var idAprendices = aprendices.Select(fa => fa.IdAprendiz).ToList();

        var juicios = await context.JuicioResultado
            .Where(j => idResultados.Contains(j.IdResultado) && idAprendices.Contains(j.IdAprendiz))
            .ToListAsync();

        var filas = new List<ReporteJuicioFila>();

        foreach (var fa in aprendices.OrderBy(a => $"{a.Aprendiz.Persona.Nombres} {a.Aprendiz.Persona.Apellidos}"))
        {
            foreach (var r in resultadosFicha.OrderBy(r => r.Plan.Competencia.Nombre).ThenBy(r => r.Codigo))
            {
                var juicio = juicios.FirstOrDefault(j => j.IdResultado == r.IdResultado
                                                      && j.IdAprendiz  == fa.IdAprendiz);
                filas.Add(new ReporteJuicioFila
                {
                    NombreCompleto    = $"{fa.Aprendiz.Persona.Nombres} {fa.Aprendiz.Persona.Apellidos}".Trim(),
                    NumeroDocumento   = fa.Aprendiz.Persona.NumeroDocumento,
                    CodigoCompetencia = r.Plan.Competencia.Codigo,
                    NombreCompetencia = r.Plan.Competencia.Nombre,
                    CodigoResultado   = r.Codigo,
                    NombreResultado   = r.Nombre,
                    Juicio            = juicio?.Juicio,
                    FechaJuicio       = juicio?.FechaJuicio
                });
            }
        }

        return new ReporteJuiciosDTO { NumeroFicha = numeroFicha, Filas = filas };
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte F — Por instructor
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<ReporteInstructorDTO?> ObtenerReporteInstructorAsync(int idInstructor, int idUsuario)
    {
        // Administrador usa el parámetro idInstructor para ver cualquier perfil.
        // Instructor autenticado queda restringido a sus propios datos (ignora el parámetro).
        int idTarget;
        if (await EsAdministradorAsync(idUsuario))
        {
            idTarget = idInstructor;
        }
        else
        {
            var idInstructorPropio = await ResolverIdInstructorAsync(idUsuario);
            idTarget = idInstructorPropio ?? idInstructor;
        }

        var instructor = await context.InstructorPerfil
            .Include(ip => ip.Usuario)
                .ThenInclude(u => u.Persona)
            .FirstOrDefaultAsync(ip => ip.IdInstructor == idTarget);

        if (instructor is null) return null;

        var fcrs = await context.FichaCompetenciaResultado
            .Include(fcr => fcr.FichaCompetencia)
                .ThenInclude(fc => fc.Ficha)
            .Include(fcr => fcr.FichaCompetencia)
                .ThenInclude(fc => fc.Competencia)
            .Where(fcr => fcr.IdInstructor == idTarget)
            .ToListAsync();

        var fichasAgrupadas = fcrs
            .GroupBy(fcr => fcr.FichaCompetencia.Ficha.NumeroFicha)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var competencias = g
                    .GroupBy(fcr => fcr.FichaCompetencia.IdFichaCompetencia)
                    .Select(cg =>
                    {
                        var fc = cg.First().FichaCompetencia;
                        return new ReporteInstructorCompetenciaFila
                        {
                            CodigoCompetencia = fc.Competencia.Codigo,
                            NombreCompetencia = fc.Competencia.Nombre,
                            Estado            = fc.Estado,
                            HorasPlaneadas    = fc.TotalHoras ?? fc.Competencia.HorasAsignadas,
                            HorasEjecutadas   = fc.HorasEjecutadas
                        };
                    }).OrderBy(c => c.NombreCompetencia).ToList();

                return new ReporteInstructorFichaFila
                {
                    NumeroFicha  = g.Key,
                    Competencias = competencias
                };
            }).ToList();

        return new ReporteInstructorDTO
        {
            IdInstructor         = idTarget,
            NombreCompleto       = NombreInstructor(instructor),
            Correo               = instructor.Usuario.Correo,
            TotalFichas          = fichasAgrupadas.Count,
            TotalCompetencias    = fichasAgrupadas.Sum(f => f.Competencias.Count),
            TotalHorasPlaneadas  = fichasAgrupadas.Sum(f => f.Competencias.Sum(c => c.HorasPlaneadas)),
            TotalHorasEjecutadas = fichasAgrupadas.Sum(f => f.Competencias.Sum(c => c.HorasEjecutadas)),
            Fichas               = fichasAgrupadas
        };
    }

    public async Task<List<InstructorSelectorDTO>> ObtenerSelectorInstructoresAsync() =>
        await context.InstructorPerfil
            .Include(ip => ip.Usuario)
                .ThenInclude(u => u.Persona)
            .OrderBy(ip => ip.Usuario.Persona.Apellidos)
            .Select(ip => new InstructorSelectorDTO
            {
                IdInstructor   = ip.IdInstructor,
                NombreCompleto = (ip.Usuario.Persona.Nombres + " " + ip.Usuario.Persona.Apellidos).Trim()
            })
            .ToListAsync();

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte G — Por competencia
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<ReporteCompetenciaDTO?> ObtenerReporteCompetenciaAsync(int idCompetencia, int idUsuario)
    {
        var competencia = await context.Competencia.FirstOrDefaultAsync(c => c.IdCompetencia == idCompetencia);
        if (competencia is null) return null;

        var idFichasAccesibles = await ObtenerIdFichasParaUsuarioAsync(idUsuario);

        var fcs = await context.FichaCompetencia
            .Include(fc => fc.Ficha)
            .Include(fc => fc.ResultadosProgramados)
                .ThenInclude(fcr => fcr.Instructor)
                    .ThenInclude(ip => ip!.Usuario)
                        .ThenInclude(u => u.Persona)
            .Where(fc => fc.IdCompetencia == idCompetencia
                      && idFichasAccesibles.Contains(fc.IdFicha))
            .OrderBy(fc => fc.Ficha.NumeroFicha)
            .ToListAsync();

        var filas = new List<ReporteCompetenciaEnFichaFila>();

        foreach (var fc in fcs)
        {
            var instructor = fc.ResultadosProgramados
                .FirstOrDefault(fcr => fcr.Instructor is not null)?.Instructor;

            int totalAprendices = await context.FichaAprendiz
                .CountAsync(fa => fa.IdFicha == fc.IdFicha && fa.Estado == "EN FORMACION");

            var idResultadosFC = fc.ResultadosProgramados
                .Select(fcr => fcr.IdResultado)
                .Distinct()
                .ToList();

            int aprobados = 0;
            if (idResultadosFC.Any())
            {
                var aprendicesFicha = await context.FichaAprendiz
                    .Where(fa => fa.IdFicha == fc.IdFicha && fa.Estado == "EN FORMACION")
                    .Select(fa => fa.IdAprendiz)
                    .ToListAsync();

                foreach (var idApr in aprendicesFicha)
                {
                    int cantJuicios = await context.JuicioResultado
                        .CountAsync(j => idResultadosFC.Contains(j.IdResultado) && j.IdAprendiz == idApr);

                    if (cantJuicios != idResultadosFC.Count) continue;

                    bool todoAprobado = await context.JuicioResultado
                        .Where(j => idResultadosFC.Contains(j.IdResultado) && j.IdAprendiz == idApr)
                        .AllAsync(j => j.Juicio == EstadosJuicio.Aprobado);

                    if (todoAprobado) aprobados++;
                }
            }

            filas.Add(new ReporteCompetenciaEnFichaFila
            {
                NumeroFicha         = fc.Ficha.NumeroFicha,
                Estado              = fc.Estado,
                NombreInstructor    = instructor is not null ? NombreInstructor(instructor) : null,
                HorasPlaneadas      = fc.TotalHoras ?? competencia.HorasAsignadas,
                HorasEjecutadas     = fc.HorasEjecutadas,
                AprendicesAprobados = aprobados,
                TotalAprendices     = totalAprendices
            });
        }

        return new ReporteCompetenciaDTO
        {
            IdCompetencia  = competencia.IdCompetencia,
            Codigo         = competencia.Codigo,
            Nombre         = competencia.Nombre,
            HorasAsignadas = competencia.HorasAsignadas,
            TotalFichas    = filas.Count,
            Fichas         = filas
        };
    }

    // ─────────────────────────────────────────────────────────────────────────────
    //  Reporte H — Patrocinio
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<List<ReportePatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario)
    {
        var idFichas = await ObtenerIdFichasParaUsuarioAsync(idUsuario);

        var patrocinios = await context.Patrocinio
            .Include(p => p.Aprendiz)
                .ThenInclude(a => a.Persona)
            .Include(p => p.Ficha)
            .Where(p => p.Activo && idFichas.Contains(p.IdFicha))
            .OrderBy(p => p.Aprendiz.Persona.Apellidos)
            .ToListAsync();

        var resultado = new List<ReportePatrocinioDTO>();

        foreach (var pat in patrocinios)
        {
            var asis = await context.Asistencia
                .Where(a => a.IdAprendiz == pat.IdAprendiz
                         && a.Sesion.FichaCompetencia.IdFicha == pat.IdFicha)
                .ToListAsync();

            int total     = asis.Count;
            int presentes = asis.Count(a => a.Estado == EstadosAsistencia.Presente
                                          || a.Estado == EstadosAsistencia.Justificado);
            double pct    = total > 0 ? Math.Round((double)presentes / total * 100, 1) : 0;

            resultado.Add(new ReportePatrocinioDTO
            {
                NombreCompleto       = $"{pat.Aprendiz.Persona.Nombres} {pat.Aprendiz.Persona.Apellidos}".Trim(),
                NumeroDocumento      = pat.Aprendiz.Persona.NumeroDocumento,
                NumeroFicha          = pat.Ficha.NumeroFicha,
                NombreEmpresa        = pat.NombreEmpresa ?? string.Empty,
                ContactoEmpresa      = pat.ContactoEmpresa,
                Etapa                = pat.Etapa,
                FechaInicioEtapa     = pat.FechaInicioEtapa,
                FechaFinEtapa        = pat.FechaFinEtapa,
                PorcentajeAsistencia = pct
            });
        }

        return resultado;
    }
}
