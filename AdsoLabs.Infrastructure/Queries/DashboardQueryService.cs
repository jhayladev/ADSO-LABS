using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Dashboard;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class DashboardQueryService(AdsoDbContext context) : IDashboardQueryService
{
    // ── Umbrales de negocio (§7.8, definidos 2026-05-29) ──────────────────
    private const int UmbralCritico           = 30;   // ProgresoGeneral < 30 → Crítico
    private const int UmbralExcelencia        = 60;   // ProgresoGeneral > 60 → Excelencia
    private const double UmbralProximaCerrar  = 0.80; // ≥ 80 % horas ejecutadas
    private const double UmbralBajaAsistencia = 0.70; // < 70 % presencia
    private const int MargenRetrasoHoras      = 10;   // puntos de tolerancia ProgresoTemporal vs ProgresoGeneral

    // ── Helpers privados ──────────────────────────────────────────────────

    /// <summary>
    /// Devuelve el IdInstructor vinculado al IdUsuario, o null si no tiene InstructorPerfil.
    /// </summary>
    private Task<int?> ResolverIdInstructorAsync(int idUsuario) =>
        context.InstructorPerfil
            .Where(ip => ip.IdUsuario == idUsuario)
            .Select(ip => (int?)ip.IdInstructor)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Devuelve los IdFicha relevantes para el dashboard:
    /// – Administrador: TODAS las fichas activas (aunque tenga InstructorPerfil).
    /// – Instructor con FCRs asignados: solo las fichas donde tiene resultados.
    /// – Instructor sin FCRs (o sin InstructorPerfil): todas las fichas activas.
    /// </summary>
    private async Task<List<int>> ObtenerIdFichasParaDashboardAsync(int idUsuario)
    {
        // Administradores siempre ven todas las fichas, independientemente
        // de si tienen InstructorPerfil asignado.
        bool esAdmin = await context.Usuario
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => u.Rol.Nombre == "Administrador")
            .FirstOrDefaultAsync();

        if (esAdmin)
            return await context.Ficha
                .Where(f => f.Estado == EstadosFicha.Activa)
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
                .Where(f => f.Estado == EstadosFicha.Activa)
                .Select(f => f.IdFicha)
                .ToListAsync();
    }

    private static int CalcularProgresoTemporal(DateOnly fechaInicio, DateOnly? fechaFin)
    {
        if (!fechaFin.HasValue) return 0;
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (hoy <= fechaInicio) return 0;
        if (hoy >= fechaFin.Value) return 100;
        var total        = fechaFin.Value.DayNumber - fechaInicio.DayNumber;
        var transcurrido = hoy.DayNumber            - fechaInicio.DayNumber;
        return total > 0 ? (int)Math.Round((double)transcurrido / total * 100) : 0;
    }

    private static string ClasificarEstado(int progresoGeneral) => progresoGeneral switch
    {
        < UmbralCritico    => "Crítico",
        <= UmbralExcelencia => "Riesgo",
        _                  => "Excelencia"
    };

    // ── Endpoints públicos ────────────────────────────────────────────────

    public async Task<ResumenDashboardDTO?> ObtenerResumenAsync(int idUsuario)
    {
        // Datos del usuario autenticado (nombre/rol) — válido para cualquier rol
        var usuario = await context.Usuario
            .Include(u => u.Persona)
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario is null) return null;

        var idFichas = await ObtenerIdFichasParaDashboardAsync(idUsuario);

        var fichas = await context.Ficha
            .Include(f => f.FichasCompetencias)
                .ThenInclude(fc => fc.Competencia)
            .Where(f => idFichas.Contains(f.IdFicha) && f.Estado == EstadosFicha.Activa)
            .ToListAsync();

        var instructoresTecnicos = await context.FichaCompetenciaResultado
            .Where(fcr => idFichas.Contains(fcr.FichaCompetencia.IdFicha)
                       && fcr.IdInstructor != null)
            .Select(fcr => fcr.IdInstructor)
            .Distinct()
            .CountAsync();

        var fichasResumen = fichas.Select(f =>
        {
            int totalHoras = f.FichasCompetencias.Sum(fc => fc.TotalHoras ?? fc.Competencia.HorasAsignadas);
            int horasEjec  = f.FichasCompetencias.Sum(fc => fc.HorasEjecutadas);
            int progreso   = totalHoras > 0 ? (int)Math.Round((double)horasEjec / totalHoras * 100) : 0;

            return new EstadoFichaResumenDTO
            {
                NumeroFicha         = f.NumeroFicha,
                ProgresoGeneral     = progreso,
                ClasificacionEstado = ClasificarEstado(progreso)
            };
        }).ToList();

        return new ResumenDashboardDTO
        {
            Nombres              = $"{usuario.Persona.Nombres} {usuario.Persona.Apellidos}".Trim(),
            Rol                  = usuario.Rol.Nombre,
            UltimaActividad      = usuario.FechaInicioSesion,
            FichasActivas        = fichas.Count,
            TotalHorasPlaneadas  = fichas.Sum(f => f.FichasCompetencias.Sum(fc => fc.TotalHoras ?? fc.Competencia.HorasAsignadas)),
            ProgresoPromedio     = fichasResumen.Count > 0
                ? (int)Math.Round(fichasResumen.Average(f => (double)f.ProgresoGeneral))
                : 0,
            InstructoresTecnicos = instructoresTecnicos,
            Fichas               = fichasResumen
        };
    }

    public async Task<EstadoCompetenciasDashboardDTO?> ObtenerEstadoCompetenciasAsync(int idUsuario)
    {
        var idFichas = await ObtenerIdFichasParaDashboardAsync(idUsuario);

        var fichaCompetencias = await context.FichaCompetencia
            .Include(fc => fc.Competencia)
            .Include(fc => fc.ResultadosProgramados)
            .Where(fc => idFichas.Contains(fc.IdFicha))
            .ToListAsync();

        int finalizadas = fichaCompetencias.Count(fc => fc.Estado == EstadosCompetencia.Vista);

        int enEjecucion = fichaCompetencias.Count(fc =>
            fc.Estado != EstadosCompetencia.Vista &&
            (fc.Estado == EstadosCompetencia.EnCurso ||
             fc.ResultadosProgramados.Any(r => r.Estado == "Programado")));

        int pendientes = fichaCompetencias.Count(fc =>
            fc.Estado == EstadosCompetencia.Pendiente &&
            !fc.ResultadosProgramados.Any(r => r.Estado == "Programado"));

        int proximasACerrar = fichaCompetencias.Count(fc =>
        {
            if (fc.Estado == EstadosCompetencia.Vista) return false;
            int totalHoras = fc.TotalHoras ?? fc.Competencia.HorasAsignadas;
            return totalHoras > 0 && (double)fc.HorasEjecutadas / totalHoras >= UmbralProximaCerrar;
        });

        return new EstadoCompetenciasDashboardDTO
        {
            Total           = fichaCompetencias.Count,
            Finalizadas     = finalizadas,
            EnEjecucion     = enEjecucion,
            ProximasACerrar = proximasACerrar,
            Pendientes      = pendientes
        };
    }

    public async Task<List<ProgresoFichaDashboardDTO>> ObtenerProgresoFichasAsync(int idUsuario)
    {
        var idFichas = await ObtenerIdFichasParaDashboardAsync(idUsuario);

        var fichas = await context.Ficha
            .Include(f => f.FichasCompetencias)
                .ThenInclude(fc => fc.Competencia)
            .Where(f => idFichas.Contains(f.IdFicha) && f.Estado == EstadosFicha.Activa)
            .ToListAsync();

        return fichas.Select(f => new ProgresoFichaDashboardDTO
        {
            NumeroFicha     = f.NumeroFicha,
            HorasPlaneadas  = f.FichasCompetencias.Sum(fc => fc.TotalHoras ?? fc.Competencia.HorasAsignadas),
            HorasEjecutadas = f.FichasCompetencias.Sum(fc => fc.HorasEjecutadas)
        }).ToList();
    }

    public async Task<AlertasDashboardDTO> ObtenerAlertasAsync(int idUsuario)
    {
        var idFichas = await ObtenerIdFichasParaDashboardAsync(idUsuario);

        var fichas = await context.Ficha
            .Include(f => f.FichasCompetencias)
                .ThenInclude(fc => fc.Competencia)
            .Where(f => idFichas.Contains(f.IdFicha) && f.Estado == EstadosFicha.Activa)
            .ToListAsync();

        var metricas = fichas.Select(f =>
        {
            int totalHoras      = f.FichasCompetencias.Sum(fc => fc.TotalHoras ?? fc.Competencia.HorasAsignadas);
            int horasEjec       = f.FichasCompetencias.Sum(fc => fc.HorasEjecutadas);
            int progresoGeneral = totalHoras > 0 ? (int)Math.Round((double)horasEjec / totalHoras * 100) : 0;

            return new
            {
                f.IdFicha,
                f.NumeroFicha,
                f.FechaFin,
                ProgresoGeneral  = progresoGeneral,
                ProgresoTemporal = CalcularProgresoTemporal(f.FechaInicio, f.FechaFin)
            };
        }).ToList();

        // Baja asistencia: ratio (Presente + Justificado) / total < 70 % por ficha
        var asistenciasPorFicha = await context.Asistencia
            .Where(a => idFichas.Contains(a.Sesion.FichaCompetencia.IdFicha))
            .GroupBy(a => a.Sesion.FichaCompetencia.IdFicha)
            .Select(g => new
            {
                IdFicha   = g.Key,
                Total     = g.Count(),
                Presentes = g.Count(a => a.Estado == EstadosAsistencia.Presente
                                      || a.Estado == EstadosAsistencia.Justificado)
            })
            .ToListAsync();

        var fichasConBajaAsistencia = asistenciasPorFicha
            .Where(a => a.Total > 0 && (double)a.Presentes / a.Total < UmbralBajaAsistencia)
            .Select(a => a.IdFicha)
            .ToHashSet();

        var hoy         = DateOnly.FromDateTime(DateTime.Today);
        var finDeSemana = hoy.AddDays(7);
        var resultado   = new AlertasDashboardDTO();

        foreach (var f in metricas)
        {
            var item = new AlertaFichaItemDTO
            {
                NumeroFicha      = f.NumeroFicha,
                ProgresoGeneral  = f.ProgresoGeneral,
                ProgresoTemporal = f.ProgresoTemporal
            };

            if (fichasConBajaAsistencia.Contains(f.IdFicha))
                resultado.BajaAsistencia.Add(item);

            if (f.ProgresoTemporal > 0 && f.ProgresoGeneral < f.ProgresoTemporal - MargenRetrasoHoras)
                resultado.RetrasoHoras.Add(item);
            else
                resultado.AlDia.Add(item);

            if (f.FechaFin.HasValue && f.FechaFin.Value >= hoy && f.FechaFin.Value <= finDeSemana)
                resultado.CulminanEstaSemana.Add(item);
        }

        return resultado;
    }
}
