using AdsoLabs.Application.DTOs.Monitoria;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class MonitoriaQueryService(AdsoDbContext db) : IMonitoriaQueryService
{
    public async Task<EstadoMonitorDTO> ObtenerEstadoMonitorAsync(int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

        if (aprendiz is null) return new EstadoMonitorDTO { EsMonitor = false };

        var monitor = await db.MonitorPerfil
            .FirstOrDefaultAsync(m => m.IdAprendiz == aprendiz.IdAprendiz && m.Activo);

        return new EstadoMonitorDTO
        {
            EsMonitor  = monitor is not null,
            IdAprendiz = aprendiz.IdAprendiz,
            IdMonitor  = monitor?.IdMonitor
        };
    }

    public async Task<List<MonitorDTO>> ObtenerMonitoresAsync()
        => await db.MonitorPerfil
            .Include(m => m.Aprendiz).ThenInclude(a => a.Persona)
            .OrderByDescending(m => m.FechaAsignacion)
            .Select(m => new MonitorDTO
            {
                IdMonitor       = m.IdMonitor,
                IdAprendiz      = m.IdAprendiz,
                NombreAprendiz  = m.Aprendiz.Persona.Nombres + " " + m.Aprendiz.Persona.Apellidos,
                NumeroDocumento = m.Aprendiz.Persona.NumeroDocumento,
                Activo          = m.Activo,
                FechaAsignacion = m.FechaAsignacion
            })
            .ToListAsync();

    public async Task<List<SesionMonitoriaDTO>> ObtenerMonitoriasVigentesAsync(int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil.FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

        var sesiones = await db.SesionMonitoria
            .Include(s => s.Monitor).ThenInclude(m => m.Aprendiz).ThenInclude(a => a.Persona)
            .Include(s => s.Inscripciones)
            .Where(s => s.Estado == "Activa")
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

        return sesiones.Select(s => MapearSesion(s, aprendiz?.IdAprendiz)).ToList();
    }

    public async Task<List<SesionMonitoriaDTO>> ObtenerMisSesionesAsync(int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil.FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);
        if (aprendiz is null) return [];

        var monitor = await db.MonitorPerfil.FirstOrDefaultAsync(m => m.IdAprendiz == aprendiz.IdAprendiz);
        if (monitor is null) return [];

        var sesiones = await db.SesionMonitoria
            .Include(s => s.Monitor).ThenInclude(m => m.Aprendiz).ThenInclude(a => a.Persona)
            .Include(s => s.Inscripciones)
            .Where(s => s.IdMonitor == monitor.IdMonitor)
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

        return sesiones.Select(s => MapearSesion(s, aprendiz.IdAprendiz)).ToList();
    }

    public async Task<List<InscripcionMonitoriaDTO>> ObtenerInscritosAsync(int idSesionMonitoria)
        => await db.InscripcionMonitoria
            .Include(i => i.Aprendiz).ThenInclude(a => a.Persona)
            .Where(i => i.IdSesionMonitoria == idSesionMonitoria)
            .OrderBy(i => i.Aprendiz.Persona.Apellidos)
            .Select(i => new InscripcionMonitoriaDTO
            {
                IdInscripcion    = i.IdInscripcion,
                IdAprendiz       = i.IdAprendiz,
                NombreAprendiz   = i.Aprendiz.Persona.Nombres + " " + i.Aprendiz.Persona.Apellidos,
                NumeroDocumento  = i.Aprendiz.Persona.NumeroDocumento,
                FechaInscripcion = i.FechaInscripcion
            })
            .ToListAsync();

    public async Task<List<AsistenciaMonitoriaDTO>> ObtenerAsistenciasRecibidasAsync(int? idInstructor)
    {
        IQueryable<Models.AsistenciaMonitoria> query = db.AsistenciaMonitoria
            .Include(a => a.SesionMonitoria)
            .Include(a => a.Aprendiz).ThenInclude(ap => ap.Persona)
            .Include(a => a.InstructorDestinatario).ThenInclude(i => i.Usuario).ThenInclude(u => u.Persona);

        if (idInstructor.HasValue)
            query = query.Where(a => a.IdInstructorDestinatario == idInstructor.Value);

        return await query
            .OrderByDescending(a => a.FechaRegistro)
            .Select(a => new AsistenciaMonitoriaDTO
            {
                IdAsistenciaMonitoria       = a.IdAsistenciaMonitoria,
                IdSesionMonitoria           = a.IdSesionMonitoria,
                NombreSesion                = a.SesionMonitoria.Nombre,
                IdAprendiz                  = a.IdAprendiz,
                NombreAprendiz              = a.Aprendiz.Persona.Nombres + " " + a.Aprendiz.Persona.Apellidos,
                Fecha                       = a.Fecha,
                Estado                      = a.Estado,
                IdInstructorDestinatario    = a.IdInstructorDestinatario,
                NombreInstructorDestinatario = a.InstructorDestinatario.Usuario.Persona.Nombres + " " + a.InstructorDestinatario.Usuario.Persona.Apellidos,
                FechaRegistro               = a.FechaRegistro
            })
            .ToListAsync();
    }

    public async Task<List<InformeMonitoriaDTO>> ObtenerInformesRecibidosAsync(int? idInstructor)
    {
        IQueryable<Models.InformeMonitoria> query = db.InformeMonitoria
            .Include(i => i.SesionMonitoria)
            .Include(i => i.InstructorDestinatario).ThenInclude(ins => ins.Usuario).ThenInclude(u => u.Persona)
            .Include(i => i.Aprendices).ThenInclude(ia => ia.Aprendiz).ThenInclude(a => a.Persona);

        if (idInstructor.HasValue)
            query = query.Where(i => i.IdInstructorDestinatario == idInstructor.Value);

        var informes = await query
            .OrderByDescending(i => i.FechaRegistro)
            .ToListAsync();

        return informes.Select(i => new InformeMonitoriaDTO
        {
            IdInforme                    = i.IdInforme,
            IdSesionMonitoria            = i.IdSesionMonitoria,
            NombreSesion                 = i.SesionMonitoria.Nombre,
            IdInstructorDestinatario     = i.IdInstructorDestinatario,
            NombreInstructorDestinatario = i.InstructorDestinatario.Usuario.Persona.Nombres + " " + i.InstructorDestinatario.Usuario.Persona.Apellidos,
            Texto                        = i.Texto,
            FechaRegistro                = i.FechaRegistro,
            Aprendices                   = i.Aprendices
                .Select(ia => ia.Aprendiz.Persona.Nombres + " " + ia.Aprendiz.Persona.Apellidos)
                .ToList()
        }).ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SesionMonitoriaDTO MapearSesion(Models.SesionMonitoria s, int? idAprendizActual)
    {
        var inscripcionActual = idAprendizActual.HasValue
            ? s.Inscripciones.FirstOrDefault(i => i.IdAprendiz == idAprendizActual.Value)
            : null;

        return new SesionMonitoriaDTO
        {
            IdSesionMonitoria         = s.IdSesionMonitoria,
            IdMonitor                 = s.IdMonitor,
            IdAprendizMonitor         = s.Monitor.IdAprendiz,
            NombreMonitor             = s.Monitor.Aprendiz.Persona.Nombres + " " + s.Monitor.Aprendiz.Persona.Apellidos,
            Nombre                    = s.Nombre,
            Descripcion               = s.Descripcion,
            Jornada                   = s.Jornada,
            Modalidad                 = s.Modalidad,
            HoraInicio                = s.HoraInicio,
            HoraFin                   = s.HoraFin,
            FechaInicio               = s.FechaInicio,
            FechaFin                  = s.FechaFin,
            Estado                    = s.Estado,
            TotalInscritos            = s.Inscripciones.Count,
            InscritoUsuarioActual     = inscripcionActual is not null,
            IdInscripcionUsuarioActual = inscripcionActual?.IdInscripcion
        };
    }
}
