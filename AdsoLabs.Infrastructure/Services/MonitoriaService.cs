using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Monitoria;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Services;

public class MonitoriaService(AdsoDbContext db) : IMonitoriaService
{
    private static readonly string[] JornadasValidas = ["Mañana", "Tarde"];
    private static readonly string[] ModalidadesValidas = ["Presencial", "Virtual"];
    private static readonly string[] EstadosAsistenciaValidos = ["Presente", "Justificado", "Ausente"];
    private static readonly string[] EstadosNoElegiblesMonitor = [EstadosAprendiz.Cancelado, EstadosAprendiz.RetiroVoluntario];

    public async Task<int> AsignarMonitorAsync(AsignarMonitorCommand cmd)
    {
        var existeAprendiz = await db.AprendizPerfil.AnyAsync(a => a.IdAprendiz == cmd.IdAprendiz);
        if (!existeAprendiz)
            throw new KeyNotFoundException($"Aprendiz {cmd.IdAprendiz} no encontrado.");

        // RN-MONITOR-01: no se asigna la condición de Monitor a aprendices Cancelados o en Retiro Voluntario.
        var estadoActual = await db.FichaAprendiz
            .Where(fa => fa.IdAprendiz == cmd.IdAprendiz)
            .OrderByDescending(fa => fa.Ficha.FechaInicio)
            .Select(fa => fa.Estado)
            .FirstOrDefaultAsync();

        if (estadoActual is not null && EstadosNoElegiblesMonitor.Contains(estadoActual))
            throw new InvalidOperationException(
                $"No se puede asignar la condición de Monitor a un aprendiz en estado '{estadoActual}'.");

        var yaEsMonitor = await db.MonitorPerfil
            .AnyAsync(m => m.IdAprendiz == cmd.IdAprendiz && m.Activo);
        if (yaEsMonitor)
            throw new InvalidOperationException("El aprendiz ya tiene la condición de Monitor activa.");

        var tienePatrocinioActivo = await db.Patrocinio
            .AnyAsync(p => p.IdAprendiz == cmd.IdAprendiz && p.Activo);
        if (tienePatrocinioActivo)
            throw new InvalidOperationException(
                "El aprendiz tiene un patrocinio activo. Un aprendiz no puede ser Patrocinado y Monitor a la vez.");

        var monitor = new MonitorPerfil
        {
            IdAprendiz      = cmd.IdAprendiz,
            Activo          = true,
            FechaAsignacion = DateTime.UtcNow
        };

        db.MonitorPerfil.Add(monitor);
        await db.SaveChangesAsync();
        return monitor.IdMonitor;
    }

    public async Task DesactivarMonitorAsync(int idAprendiz)
    {
        var monitor = await db.MonitorPerfil
            .FirstOrDefaultAsync(m => m.IdAprendiz == idAprendiz && m.Activo)
            ?? throw new KeyNotFoundException($"El aprendiz {idAprendiz} no tiene condición de Monitor activa.");

        monitor.Activo = false;

        // Al desactivar la condición de Monitor, sus sesiones vigentes ya no tienen quién las administre.
        var sesionesActivas = await db.SesionMonitoria
            .Where(s => s.IdMonitor == monitor.IdMonitor && s.Estado == "Activa")
            .ToListAsync();

        foreach (var sesion in sesionesActivas)
            sesion.Estado = "Cancelada";

        await db.SaveChangesAsync();
    }

    public async Task<int> CrearSesionAsync(CrearSesionMonitoriaCommand cmd)
    {
        var monitor = await ResolverMonitorActivoAsync(cmd.IdUsuario);

        if (!JornadasValidas.Contains(cmd.Jornada))
            throw new ArgumentException("Jornada inválida. Debe ser 'Mañana' o 'Tarde'.");
        if (!ModalidadesValidas.Contains(cmd.Modalidad))
            throw new ArgumentException("Modalidad inválida. Debe ser 'Presencial' o 'Virtual'.");
        if (cmd.HoraInicio >= cmd.HoraFin)
            throw new ArgumentException("La hora de inicio debe ser menor a la hora de fin.");
        if (cmd.FechaFin.HasValue && cmd.FechaFin.Value < cmd.FechaInicio)
            throw new ArgumentException("La fecha de fin no puede ser anterior a la fecha de inicio.");
        if (string.IsNullOrWhiteSpace(cmd.Nombre))
            throw new ArgumentException("El nombre de la monitoría es obligatorio.");

        var sesion = new SesionMonitoria
        {
            IdMonitor     = monitor.IdMonitor,
            Nombre        = cmd.Nombre,
            Descripcion   = cmd.Descripcion,
            Jornada       = cmd.Jornada,
            Modalidad     = cmd.Modalidad,
            HoraInicio    = cmd.HoraInicio,
            HoraFin       = cmd.HoraFin,
            FechaInicio   = cmd.FechaInicio,
            FechaFin      = cmd.FechaFin,
            Estado        = "Activa",
            FechaCreacion = DateTime.UtcNow
        };

        db.SesionMonitoria.Add(sesion);
        await db.SaveChangesAsync();
        return sesion.IdSesionMonitoria;
    }

    public async Task CancelarSesionAsync(int idSesionMonitoria, int idUsuarioMonitor)
    {
        var monitor = await ResolverMonitorActivoAsync(idUsuarioMonitor);

        var sesion = await db.SesionMonitoria
            .FirstOrDefaultAsync(s => s.IdSesionMonitoria == idSesionMonitoria)
            ?? throw new KeyNotFoundException($"Sesión de monitoría {idSesionMonitoria} no encontrada.");

        if (sesion.IdMonitor != monitor.IdMonitor)
            throw new InvalidOperationException("No puedes cancelar una sesión de monitoría que no es tuya.");

        sesion.Estado = "Cancelada";
        await db.SaveChangesAsync();
    }

    public async Task<int> InscribirseAsync(InscribirseMonitoriaCommand cmd)
    {
        var aprendiz = await db.AprendizPerfil
            .FirstOrDefaultAsync(a => a.IdUsuario == cmd.IdUsuario)
            ?? throw new KeyNotFoundException("Aprendiz no encontrado para el usuario autenticado.");

        var sesion = await db.SesionMonitoria
            .Include(s => s.Monitor)
            .FirstOrDefaultAsync(s => s.IdSesionMonitoria == cmd.IdSesionMonitoria)
            ?? throw new KeyNotFoundException($"Sesión de monitoría {cmd.IdSesionMonitoria} no encontrada.");

        if (sesion.Estado != "Activa")
            throw new InvalidOperationException("Esta monitoría ya no está activa.");

        if (sesion.Monitor.IdAprendiz == aprendiz.IdAprendiz)
            throw new InvalidOperationException("No puedes inscribirte a tu propia monitoría.");

        var yaInscrito = await db.InscripcionMonitoria
            .AnyAsync(i => i.IdSesionMonitoria == cmd.IdSesionMonitoria && i.IdAprendiz == aprendiz.IdAprendiz);
        if (yaInscrito)
            throw new InvalidOperationException("Ya estás inscrito en esta monitoría.");

        var inscripcion = new InscripcionMonitoria
        {
            IdSesionMonitoria = cmd.IdSesionMonitoria,
            IdAprendiz        = aprendiz.IdAprendiz,
            FechaInscripcion  = DateTime.UtcNow
        };

        db.InscripcionMonitoria.Add(inscripcion);
        await db.SaveChangesAsync();
        return inscripcion.IdInscripcion;
    }

    public async Task DesinscribirseAsync(int idInscripcion, int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario)
            ?? throw new KeyNotFoundException("Aprendiz no encontrado para el usuario autenticado.");

        var inscripcion = await db.InscripcionMonitoria
            .FirstOrDefaultAsync(i => i.IdInscripcion == idInscripcion)
            ?? throw new KeyNotFoundException($"Inscripción {idInscripcion} no encontrada.");

        if (inscripcion.IdAprendiz != aprendiz.IdAprendiz)
            throw new InvalidOperationException("No puedes desinscribir a otro aprendiz.");

        db.InscripcionMonitoria.Remove(inscripcion);
        await db.SaveChangesAsync();
    }

    public async Task RegistrarAsistenciaAsync(RegistrarAsistenciaMonitoriaCommand cmd)
    {
        var monitor = await ResolverMonitorActivoAsync(cmd.IdUsuarioMonitor);
        var sesion = await ResolverSesionDelMonitorAsync(cmd.IdSesionMonitoria, monitor.IdMonitor);

        var instructorActivo = await db.InstructorPerfil
            .AnyAsync(i => i.IdInstructor == cmd.IdInstructorDestinatario && i.Activo);
        if (!instructorActivo)
            throw new ArgumentException("El instructor destinatario no existe o no está activo.");

        foreach (var item in cmd.Asistencias)
        {
            if (!EstadosAsistenciaValidos.Contains(item.Estado))
                throw new ArgumentException($"Estado de asistencia inválido: '{item.Estado}'.");

            var existente = await db.AsistenciaMonitoria.FirstOrDefaultAsync(a =>
                a.IdSesionMonitoria == cmd.IdSesionMonitoria
                && a.IdAprendiz == item.IdAprendiz
                && a.Fecha == cmd.Fecha);

            if (existente is not null)
            {
                existente.Estado = item.Estado;
                existente.IdInstructorDestinatario = cmd.IdInstructorDestinatario;
            }
            else
            {
                db.AsistenciaMonitoria.Add(new AsistenciaMonitoria
                {
                    IdSesionMonitoria        = cmd.IdSesionMonitoria,
                    IdAprendiz               = item.IdAprendiz,
                    Fecha                    = cmd.Fecha,
                    Estado                   = item.Estado,
                    IdInstructorDestinatario = cmd.IdInstructorDestinatario,
                    FechaRegistro            = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task RegistrarInformeAsync(RegistrarInformeMonitoriaCommand cmd)
    {
        var monitor = await ResolverMonitorActivoAsync(cmd.IdUsuarioMonitor);
        await ResolverSesionDelMonitorAsync(cmd.IdSesionMonitoria, monitor.IdMonitor);

        if (string.IsNullOrWhiteSpace(cmd.Texto))
            throw new ArgumentException("El texto del informe es obligatorio.");
        if (cmd.IdsAprendices.Count == 0)
            throw new ArgumentException("Debes seleccionar al menos un aprendiz para el informe.");

        var instructorActivo = await db.InstructorPerfil
            .AnyAsync(i => i.IdInstructor == cmd.IdInstructorDestinatario && i.Activo);
        if (!instructorActivo)
            throw new ArgumentException("El instructor destinatario no existe o no está activo.");

        var inscritos = await db.InscripcionMonitoria
            .Where(i => i.IdSesionMonitoria == cmd.IdSesionMonitoria)
            .Select(i => i.IdAprendiz)
            .ToListAsync();

        if (cmd.IdsAprendices.Except(inscritos).Any())
            throw new ArgumentException("Solo puedes reportar sobre aprendices inscritos en esta sesión.");

        var informe = new InformeMonitoria
        {
            IdSesionMonitoria        = cmd.IdSesionMonitoria,
            IdInstructorDestinatario = cmd.IdInstructorDestinatario,
            Texto                    = cmd.Texto,
            FechaRegistro            = DateTime.UtcNow
        };

        db.InformeMonitoria.Add(informe);
        await db.SaveChangesAsync();

        foreach (var idAprendiz in cmd.IdsAprendices)
        {
            db.InformeMonitoriaAprendiz.Add(new InformeMonitoriaAprendiz
            {
                IdInforme  = informe.IdInforme,
                IdAprendiz = idAprendiz
            });
        }

        await db.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<MonitorPerfil> ResolverMonitorActivoAsync(int idUsuario)
    {
        var aprendiz = await db.AprendizPerfil
            .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario)
            ?? throw new KeyNotFoundException("Aprendiz no encontrado para el usuario autenticado.");

        return await db.MonitorPerfil
            .FirstOrDefaultAsync(m => m.IdAprendiz == aprendiz.IdAprendiz && m.Activo)
            ?? throw new InvalidOperationException("No tienes la condición de Monitor activa.");
    }

    private async Task<SesionMonitoria> ResolverSesionDelMonitorAsync(int idSesionMonitoria, int idMonitor)
    {
        var sesion = await db.SesionMonitoria
            .FirstOrDefaultAsync(s => s.IdSesionMonitoria == idSesionMonitoria)
            ?? throw new KeyNotFoundException($"Sesión de monitoría {idSesionMonitoria} no encontrada.");

        if (sesion.IdMonitor != idMonitor)
            throw new InvalidOperationException("Esta sesión de monitoría no te pertenece.");

        return sesion;
    }
}
