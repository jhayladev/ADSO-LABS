using AdsoLabs.Application.Commands.Repositorio;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class RepositorioRepository(AdsoDbContext context) : IRepositorioRepository
{
    // ─── ARCHIVO BASE ────────────────────────────────────────────────────────

    public async Task<int> AgregarArchivoAsync(AgregarArchivoCommand cmd, CancellationToken ct = default)
    {
        var tipo = await context.TipoArchivo
            .FirstOrDefaultAsync(t => t.Nombre == cmd.NombreTipoArchivo, ct)
            ?? await context.TipoArchivo.FirstAsync(ct);   // fallback al primero si no existe

        var archivo = new Archivo
        {
            NombreOriginal   = cmd.NombreOriginal,
            NombreAlmacenado = cmd.NombreAlmacenado,
            RutaStorage      = cmd.RutaStorage,
            ExtensionArchivo = cmd.Extension,
            MimeType         = cmd.MimeType,
            IdTipoArchivo    = tipo.IdTipoArchivo,
            IdUsuarioSubio   = cmd.IdUsuarioSubio,
            TamanioBytes     = cmd.TamanioBytes,
            Version          = 1,
            FechaSubida      = DateTime.Now,
            Activo           = true
        };
        context.Archivo.Add(archivo);
        await context.SaveChangesAsync(ct);
        return archivo.IdArchivo;
    }

    // ─── DOCUMENTOS ACADÉMICOS ───────────────────────────────────────────────

    public async Task<int> AgregarGuiaAsync(AgregarGuiaCommand cmd, CancellationToken ct = default)
    {
        var guia = new GuiaAprendizaje
        {
            IdFichaCompetencia = cmd.IdFichaCompetencia,
            IdInstructor       = cmd.IdInstructor,
            IdDocumento        = cmd.IdArchivoCreado,
            Titulo             = cmd.Titulo,
            Descripcion        = cmd.Descripcion,
            Version            = 1,
            Estado             = "Borrador",
            FechaCreacion      = DateTime.Now,
            Activo             = true
        };
        context.GuiaAprendizaje.Add(guia);
        await context.SaveChangesAsync(ct);
        return guia.IdGuia;
    }

    public async Task<int> AgregarInstrumentoAsync(AgregarInstrumentoCommand cmd, CancellationToken ct = default)
    {
        var inst = new InstrumentoEvaluacion
        {
            IdCompetencia = cmd.IdCompetencia,
            IdInstructor  = cmd.IdInstructor,
            IdDocumento   = cmd.IdArchivoCreado,
            Nombre        = cmd.Nombre,
            Descripcion   = cmd.Descripcion,
            Version       = 1,
            Estado        = "Borrador",
            FechaCreacion = DateTime.Now,
            Activo        = true
        };
        context.InstrumentoEvaluacion.Add(inst);
        await context.SaveChangesAsync(ct);
        return inst.IdInstrumento;
    }

    public async Task<int> AgregarPlaneacionAsync(AgregarPlaneacionCommand cmd, CancellationToken ct = default)
    {
        var plan = new PlaneacionPedagogica
        {
            IdFichaCompetencia = cmd.IdFichaCompetencia,
            IdInstructor       = cmd.IdInstructor,
            IdDocumento        = cmd.IdArchivoCreado,
            Descripcion        = cmd.Descripcion,
            Version            = 1,
            Estado             = "Borrador",
            FechaCreacion      = DateTime.Now,
            Activo             = true
        };
        context.PlaneacionPedagogica.Add(plan);
        await context.SaveChangesAsync(ct);
        return plan.IdPlaneacion;
    }

    public async Task<int> AgregarProyectoAsync(AgregarProyectoCommand cmd, CancellationToken ct = default)
    {
        var proy = new ProyectoFormativo
        {
            IdFicha       = cmd.IdFicha,
            IdInstructor  = cmd.IdInstructor,
            IdDocumento   = cmd.IdArchivoCreado,
            Titulo        = cmd.Titulo,
            Descripcion   = cmd.Descripcion,
            Version       = 1,
            Estado        = "Borrador",
            FechaCreacion = DateTime.Now,
            Activo        = true
        };
        context.ProyectoFormativo.Add(proy);
        await context.SaveChangesAsync(ct);
        return proy.IdProyecto;
    }

    public async Task<int> AgregarDesarrolloAsync(AgregarDesarrolloCommand cmd, CancellationToken ct = default)
    {
        var des = new DesarrolloCurricular
        {
            IdCompetencia = cmd.IdCompetencia,
            IdInstructor  = cmd.IdInstructor,
            IdDocumento   = cmd.IdArchivoCreado,
            Descripcion   = cmd.Descripcion,
            Version       = 1,
            Estado        = "Borrador",
            FechaCreacion = DateTime.Now,
            Activo        = true
        };
        context.DesarrolloCurricular.Add(des);
        await context.SaveChangesAsync(ct);
        return des.IdDesarrollo;
    }

    public async Task VincularArchivoAPlanAsync(int idPlan, int idArchivo, CancellationToken ct = default)
    {
        var plan = await context.PlanConcertado.FindAsync([idPlan], ct);
        if (plan is null) return;
        plan.IdDocumento = idArchivo;
        await context.SaveChangesAsync(ct);
    }

    // ─── OPERACIONES GENÉRICAS ───────────────────────────────────────────────

    public async Task EliminarDocumentoAsync(string tipoDoc, int idDocumento, CancellationToken ct = default)
    {
        int? idArchivo = null;

        switch (tipoDoc)
        {
            case "Guia":
                var g = await context.GuiaAprendizaje.FindAsync([idDocumento], ct);
                if (g is { Activo: true }) { idArchivo = g.IdDocumento; g.Activo = false; await context.SaveChangesAsync(ct); }
                break;
            case "Instrumento":
                var ins = await context.InstrumentoEvaluacion.FindAsync([idDocumento], ct);
                if (ins is { Activo: true }) { idArchivo = ins.IdDocumento; ins.Activo = false; await context.SaveChangesAsync(ct); }
                break;
            case "Planeacion":
                var pl = await context.PlaneacionPedagogica.FindAsync([idDocumento], ct);
                if (pl is { Activo: true }) { idArchivo = pl.IdDocumento; pl.Activo = false; await context.SaveChangesAsync(ct); }
                break;
            case "Proyecto":
                var pr = await context.ProyectoFormativo.FindAsync([idDocumento], ct);
                if (pr is { Activo: true }) { idArchivo = pr.IdDocumento; pr.Activo = false; await context.SaveChangesAsync(ct); }
                break;
            case "Desarrollo":
                var d = await context.DesarrolloCurricular.FindAsync([idDocumento], ct);
                if (d is { Activo: true }) { idArchivo = d.IdDocumento; d.Activo = false; await context.SaveChangesAsync(ct); }
                break;
            case "Plan":
                idArchivo = await DesvincularPlanAsync(idDocumento, ct);
                break;
        }

        if (idArchivo.HasValue)
        {
            var archivo = await context.Archivo.FindAsync([idArchivo.Value], ct);
            if (archivo is not null) { archivo.Activo = false; await context.SaveChangesAsync(ct); }
        }
    }

    public async Task CambiarEstadoAsync(string tipoDoc, int idDocumento, string nuevoEstado, CancellationToken ct = default)
    {
        switch (tipoDoc)
        {
            case "Guia":
                var g = await context.GuiaAprendizaje.FindAsync([idDocumento], ct);
                if (g is not null) { g.Estado = nuevoEstado; break; }
                return;
            case "Instrumento":
                var i = await context.InstrumentoEvaluacion.FindAsync([idDocumento], ct);
                if (i is not null) { i.Estado = nuevoEstado; break; }
                return;
            case "Planeacion":
                var pl = await context.PlaneacionPedagogica.FindAsync([idDocumento], ct);
                if (pl is not null) { pl.Estado = nuevoEstado; break; }
                return;
            case "Proyecto":
                var pr = await context.ProyectoFormativo.FindAsync([idDocumento], ct);
                if (pr is not null) { pr.Estado = nuevoEstado; break; }
                return;
            case "Desarrollo":
                var d = await context.DesarrolloCurricular.FindAsync([idDocumento], ct);
                if (d is not null) { d.Estado = nuevoEstado; break; }
                return;
            case "Plan":
                var p = await context.PlanConcertado.FindAsync([idDocumento], ct);
                if (p is not null) { p.Estado = nuevoEstado; break; }
                return;
        }
        await context.SaveChangesAsync(ct);
    }

    public async Task<(int IdInstructor, int IdArchivo, string RutaStorage)?> ObtenerInfoDocumentoAsync(
        string tipoDoc, int idDocumento, CancellationToken ct = default)
    {
        // Los expression trees de EF Core no permiten literales de tupla.
        // Se proyecta a tipo anónimo en SQL y se convierte a tupla en memoria.
        switch (tipoDoc)
        {
            case "Guia":
            {
                var r = await (from g in context.GuiaAprendizaje
                               join a in context.Archivo on g.IdDocumento equals a.IdArchivo
                               where g.IdGuia == idDocumento && g.Activo
                               select new { g.IdInstructor, a.IdArchivo, a.RutaStorage })
                              .FirstOrDefaultAsync(ct);
                return r is null ? null : (r.IdInstructor, r.IdArchivo, r.RutaStorage);
            }
            case "Instrumento":
            {
                var r = await (from x in context.InstrumentoEvaluacion
                               join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                               where x.IdInstrumento == idDocumento && x.Activo
                               select new { x.IdInstructor, a.IdArchivo, a.RutaStorage })
                              .FirstOrDefaultAsync(ct);
                return r is null ? null : (r.IdInstructor, r.IdArchivo, r.RutaStorage);
            }
            case "Planeacion":
            {
                var r = await (from x in context.PlaneacionPedagogica
                               join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                               where x.IdPlaneacion == idDocumento && x.Activo
                               select new { x.IdInstructor, a.IdArchivo, a.RutaStorage })
                              .FirstOrDefaultAsync(ct);
                return r is null ? null : (r.IdInstructor, r.IdArchivo, r.RutaStorage);
            }
            case "Proyecto":
            {
                var r = await (from x in context.ProyectoFormativo
                               join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                               where x.IdProyecto == idDocumento && x.Activo
                               select new { x.IdInstructor, a.IdArchivo, a.RutaStorage })
                              .FirstOrDefaultAsync(ct);
                return r is null ? null : (r.IdInstructor, r.IdArchivo, r.RutaStorage);
            }
            case "Desarrollo":
            {
                var r = await (from x in context.DesarrolloCurricular
                               join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                               where x.IdDesarrollo == idDocumento && x.Activo
                               select new { x.IdInstructor, a.IdArchivo, a.RutaStorage })
                              .FirstOrDefaultAsync(ct);
                return r is null ? null : (r.IdInstructor, r.IdArchivo, r.RutaStorage);
            }
            case "Plan":
            {
                var r = await (from x in context.PlanConcertado
                               join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                               where x.IdPlan == idDocumento && x.Activo && x.IdDocumento != null
                               select new { IdInstructor = x.IdInstructor ?? 0, a.IdArchivo, a.RutaStorage })
                              .FirstOrDefaultAsync(ct);
                return r is null ? null : (r.IdInstructor, r.IdArchivo, r.RutaStorage);
            }
            default:
                return null;
        }
    }

    public async Task RegistrarDescargaAsync(int idUsuario, int idArchivo, string nombreArchivo, string ip,
        CancellationToken ct = default)
    {
        context.Auditoria.Add(new Auditoria
        {
            IdUsuario          = idUsuario,
            Accion             = "DESCARGA",
            TablaAfectada      = "Archivo",
            IdRegistroAfectado = idArchivo.ToString(),
            DatosNuevos        = nombreArchivo,
            Fecha              = DateTime.Now,
            DireccionIp        = ip
        });
        await context.SaveChangesAsync(ct);
    }

    private async Task<int?> DesvincularPlanAsync(int idPlan, CancellationToken ct)
    {
        var plan = await context.PlanConcertado.FindAsync([idPlan], ct);
        if (plan?.IdDocumento is null) return null;
        var idArchivo = plan.IdDocumento;
        plan.IdDocumento = null;
        await context.SaveChangesAsync(ct);
        return idArchivo;
    }
}
