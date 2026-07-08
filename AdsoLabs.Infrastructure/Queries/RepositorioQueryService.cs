using AdsoLabs.Application.DTOs.Repositorio;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class RepositorioQueryService(AdsoDbContext context) : IRepositorioQueryService
{
    // ─── GUÍAS ───────────────────────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerGuiasAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = from g in context.GuiaAprendizaje
                join a in context.Archivo on g.IdDocumento equals a.IdArchivo
                join fc in context.FichaCompetencia on g.IdFichaCompetencia equals fc.IdFichaCompetencia
                join fi in context.Ficha on fc.IdFicha equals fi.IdFicha
                join co in context.Competencia on fc.IdCompetencia equals co.IdCompetencia
                join ip in context.InstructorPerfil on g.IdInstructor equals ip.IdInstructor
                join us in context.Usuario on ip.IdUsuario equals us.IdUsuario
                join pe in context.Persona on us.IdPersona equals pe.IdPersona
                where g.Activo && a.Activo
                      && (string.IsNullOrEmpty(filtro)
                          || g.Titulo.Contains(filtro)
                          || fi.NumeroFicha.Contains(filtro)
                          || co.Nombre.Contains(filtro)
                          || pe.Nombres.Contains(filtro)
                          || pe.Apellidos.Contains(filtro))
                select new DocumentoListadoDTO
                {
                    IdDocumento        = g.IdGuia,
                    TipoDocumento      = "Guia",
                    Nombre             = g.Titulo,
                    Descripcion        = g.Descripcion,
                    Estado             = g.Estado,
                    Version            = g.Version,
                    FechaCreacion      = g.FechaCreacion,
                    IdInstructor       = g.IdInstructor,
                    InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                    EntidadRelacionada = fi.NumeroFicha + " – " + co.Nombre,
                    IdArchivo          = a.IdArchivo,
                    NombreOriginal     = a.NombreOriginal,
                    ExtensionArchivo   = a.ExtensionArchivo,
                    TamanioBytes       = a.TamanioBytes
                };

        return await q.OrderByDescending(x => x.FechaCreacion).ToListAsync(ct);
    }

    // ─── INSTRUMENTOS ────────────────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerInstrumentosAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = from x in context.InstrumentoEvaluacion
                join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                join co in context.Competencia on x.IdCompetencia equals co.IdCompetencia
                join ip in context.InstructorPerfil on x.IdInstructor equals ip.IdInstructor
                join us in context.Usuario on ip.IdUsuario equals us.IdUsuario
                join pe in context.Persona on us.IdPersona equals pe.IdPersona
                where x.Activo && a.Activo
                select new DocumentoListadoDTO
                {
                    IdDocumento        = x.IdInstrumento,
                    TipoDocumento      = "Instrumento",
                    Nombre             = x.Nombre,
                    Descripcion        = x.Descripcion,
                    Estado             = x.Estado,
                    Version            = x.Version,
                    FechaCreacion      = x.FechaCreacion,
                    IdInstructor       = x.IdInstructor,
                    InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                    EntidadRelacionada = co.Nombre,
                    IdArchivo          = a.IdArchivo,
                    NombreOriginal     = a.NombreOriginal,
                    ExtensionArchivo   = a.ExtensionArchivo,
                    TamanioBytes       = a.TamanioBytes
                };

        if (!string.IsNullOrWhiteSpace(filtro))
            q = q.Where(x => x.Nombre.Contains(filtro) || x.InstructorNombre.Contains(filtro));

        return await q.OrderByDescending(x => x.FechaCreacion).ToListAsync(ct);
    }

    // ─── PLANEACIONES ────────────────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerPlaneacionesAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = from x in context.PlaneacionPedagogica
                join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                join fc in context.FichaCompetencia on x.IdFichaCompetencia equals fc.IdFichaCompetencia
                join fi in context.Ficha on fc.IdFicha equals fi.IdFicha
                join co in context.Competencia on fc.IdCompetencia equals co.IdCompetencia
                join ip in context.InstructorPerfil on x.IdInstructor equals ip.IdInstructor
                join us in context.Usuario on ip.IdUsuario equals us.IdUsuario
                join pe in context.Persona on us.IdPersona equals pe.IdPersona
                where x.Activo && a.Activo
                select new DocumentoListadoDTO
                {
                    IdDocumento        = x.IdPlaneacion,
                    TipoDocumento      = "Planeacion",
                    Nombre             = "Planeación – " + fi.NumeroFicha + " / " + co.Nombre,
                    Descripcion        = x.Descripcion,
                    Estado             = x.Estado,
                    Version            = x.Version,
                    FechaCreacion      = x.FechaCreacion,
                    IdInstructor       = x.IdInstructor,
                    InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                    EntidadRelacionada = fi.NumeroFicha + " – " + co.Nombre,
                    IdArchivo          = a.IdArchivo,
                    NombreOriginal     = a.NombreOriginal,
                    ExtensionArchivo   = a.ExtensionArchivo,
                    TamanioBytes       = a.TamanioBytes
                };

        if (!string.IsNullOrWhiteSpace(filtro))
            q = q.Where(x => x.Nombre.Contains(filtro) || x.InstructorNombre.Contains(filtro));

        return await q.OrderByDescending(x => x.FechaCreacion).ToListAsync(ct);
    }

    // ─── PROYECTOS FORMATIVOS ────────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerProyectosAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = from x in context.ProyectoFormativo
                join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                join fi in context.Ficha on x.IdFicha equals fi.IdFicha
                join ip in context.InstructorPerfil on x.IdInstructor equals ip.IdInstructor
                join us in context.Usuario on ip.IdUsuario equals us.IdUsuario
                join pe in context.Persona on us.IdPersona equals pe.IdPersona
                where x.Activo && a.Activo
                select new DocumentoListadoDTO
                {
                    IdDocumento        = x.IdProyecto,
                    TipoDocumento      = "Proyecto",
                    Nombre             = x.Titulo,
                    Descripcion        = x.Descripcion,
                    Estado             = x.Estado,
                    Version            = x.Version,
                    FechaCreacion      = x.FechaCreacion,
                    IdInstructor       = x.IdInstructor,
                    InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                    EntidadRelacionada = "Ficha " + fi.NumeroFicha,
                    IdArchivo          = a.IdArchivo,
                    NombreOriginal     = a.NombreOriginal,
                    ExtensionArchivo   = a.ExtensionArchivo,
                    TamanioBytes       = a.TamanioBytes
                };

        if (!string.IsNullOrWhiteSpace(filtro))
            q = q.Where(x => x.Nombre.Contains(filtro) || x.InstructorNombre.Contains(filtro));

        return await q.OrderByDescending(x => x.FechaCreacion).ToListAsync(ct);
    }

    // ─── DESARROLLO CURRICULAR ───────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerDesarrollosAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = from x in context.DesarrolloCurricular
                join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                join co in context.Competencia on x.IdCompetencia equals co.IdCompetencia
                join ip in context.InstructorPerfil on x.IdInstructor equals ip.IdInstructor
                join us in context.Usuario on ip.IdUsuario equals us.IdUsuario
                join pe in context.Persona on us.IdPersona equals pe.IdPersona
                where x.Activo && a.Activo
                select new DocumentoListadoDTO
                {
                    IdDocumento        = x.IdDesarrollo,
                    TipoDocumento      = "Desarrollo",
                    Nombre             = "Desarrollo Curricular – " + co.Nombre,
                    Descripcion        = x.Descripcion,
                    Estado             = x.Estado,
                    Version            = x.Version,
                    FechaCreacion      = x.FechaCreacion,
                    IdInstructor       = x.IdInstructor,
                    InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                    EntidadRelacionada = co.Nombre,
                    IdArchivo          = a.IdArchivo,
                    NombreOriginal     = a.NombreOriginal,
                    ExtensionArchivo   = a.ExtensionArchivo,
                    TamanioBytes       = a.TamanioBytes
                };

        if (!string.IsNullOrWhiteSpace(filtro))
            q = q.Where(x => x.Nombre.Contains(filtro) || x.InstructorNombre.Contains(filtro));

        return await q.OrderByDescending(x => x.FechaCreacion).ToListAsync(ct);
    }

    // ─── PLANES CONCERTADOS ──────────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerPlanesConcertadosAsync(string? filtro = null, CancellationToken ct = default)
    {
        var q = from x in context.PlanConcertado
                join a in context.Archivo on x.IdDocumento equals a.IdArchivo
                join co in context.Competencia on x.IdCompetencia equals co.IdCompetencia
                join ip in context.InstructorPerfil on x.IdInstructor equals ip.IdInstructor into ip_join
                from ip in ip_join.DefaultIfEmpty()
                join us in context.Usuario on ip.IdUsuario equals us.IdUsuario into us_join
                from us in us_join.DefaultIfEmpty()
                join pe in context.Persona on us.IdPersona equals pe.IdPersona into pe_join
                from pe in pe_join.DefaultIfEmpty()
                where x.Activo && a.Activo
                select new DocumentoListadoDTO
                {
                    IdDocumento        = x.IdPlan,
                    TipoDocumento      = "Plan",
                    Nombre             = "Plan Concertado – " + co.Nombre,
                    Descripcion        = x.DescripcionGeneral,
                    Estado             = x.Estado,
                    Version            = 1,
                    FechaCreacion      = x.FechaCreacion,
                    IdInstructor       = ip != null ? ip.IdInstructor : 0,
                    InstructorNombre   = pe != null ? pe.Nombres + " " + pe.Apellidos : "Sin instructor",
                    EntidadRelacionada = co.Nombre,
                    IdArchivo          = a.IdArchivo,
                    NombreOriginal     = a.NombreOriginal,
                    ExtensionArchivo   = a.ExtensionArchivo,
                    TamanioBytes       = a.TamanioBytes
                };

        if (!string.IsNullOrWhiteSpace(filtro))
            q = q.Where(x => x.Nombre.Contains(filtro) || x.InstructorNombre.Contains(filtro));

        return await q.OrderByDescending(x => x.FechaCreacion).ToListAsync(ct);
    }

    // ─── DESCARGAS (Auditoría) ───────────────────────────────────────────────

    public async Task<List<DescargaDTO>> ObtenerMisDescargasAsync(int idUsuario, CancellationToken ct = default)
    {
        // int.Parse no es traducible a SQL — se obtienen las auditorías primero
        // y luego se hace el join con Archivo en memoria.
        var auditorias = await context.Auditoria
            .Where(a => a.IdUsuario == idUsuario && a.Accion == "DESCARGA")
            .OrderByDescending(a => a.Fecha)
            .Take(100)
            .ToListAsync(ct);

        if (auditorias.Count == 0) return [];

        var idArchivos = auditorias
            .Select(a => int.TryParse(a.IdRegistroAfectado, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Solo archivos activos — si fue eliminado no aparece en el historial
        var archivos = await context.Archivo
            .Where(a => idArchivos.Contains(a.IdArchivo) && a.Activo)
            .ToDictionaryAsync(a => a.IdArchivo, ct);

        return auditorias
            .Select(au =>
            {
                int.TryParse(au.IdRegistroAfectado, out var idArchivo);
                archivos.TryGetValue(idArchivo, out var ar);
                return (au, ar, idArchivo);
            })
            .Where(x => x.ar is not null)   // excluir si el archivo fue eliminado
            .Select(x => new DescargaDTO
            {
                IdAuditoria      = x.au.IdAuditoria,
                NombreOriginal   = x.au.DatosNuevos ?? x.ar!.NombreOriginal,
                ExtensionArchivo = x.ar!.ExtensionArchivo,
                FechaDescarga    = x.au.Fecha,
                IdArchivo        = x.idArchivo
            })
            .ToList();
    }

    // ─── ARCHIVO INFO ────────────────────────────────────────────────────────

    public async Task<ArchivoInfoDTO?> ObtenerArchivoAsync(int idArchivo, CancellationToken ct = default)
    {
        return await context.Archivo
            .Where(a => a.IdArchivo == idArchivo && a.Activo)
            .Select(a => new ArchivoInfoDTO
            {
                IdArchivo      = a.IdArchivo,
                NombreOriginal = a.NombreOriginal,
                RutaStorage    = a.RutaStorage,
                MimeType       = a.MimeType,
                ExtensionArchivo = a.ExtensionArchivo,
                IdUsuarioSubio = a.IdUsuarioSubio
            })
            .FirstOrDefaultAsync(ct);
    }

    // ─── SELECTORES PARA MODAL DE SUBIDA ────────────────────────────────────

    public async Task<List<PlanParaSubirDTO>> ObtenerPlanesSinDocumentoAsync(CancellationToken ct = default)
    {
        return await (
            from p in context.PlanConcertado
            join co in context.Competencia on p.IdCompetencia equals co.IdCompetencia
            where p.Activo
            orderby co.Nombre
            select new PlanParaSubirDTO
            {
                IdPlan            = p.IdPlan,
                CompetenciaNombre = co.Nombre,
                Estado            = p.Estado
            }
        ).ToListAsync(ct);
    }

    public async Task<List<FichaCompetenciaSelectDTO>> ObtenerFichasCompetenciasAsync(CancellationToken ct = default)
    {
        return await (
            from fc in context.FichaCompetencia
            join fi in context.Ficha on fc.IdFicha equals fi.IdFicha
            join co in context.Competencia on fc.IdCompetencia equals co.IdCompetencia
            where fi.Estado != "Cancelada"
            orderby fi.NumeroFicha, co.Nombre
            select new FichaCompetenciaSelectDTO
            {
                IdFichaCompetencia = fc.IdFichaCompetencia,
                NumeroFicha        = fi.NumeroFicha,
                CompetenciaNombre  = co.Nombre
            }
        ).ToListAsync(ct);
    }

    public async Task<List<CompetenciaSelectDTO>> ObtenerCompetenciasAsync(CancellationToken ct = default)
    {
        return await context.Competencia
            .Where(c => c.Estado != "Clausurada")
            .OrderBy(c => c.Nombre)
            .Select(c => new CompetenciaSelectDTO { IdCompetencia = c.IdCompetencia, Nombre = c.Nombre })
            .ToListAsync(ct);
    }

    public async Task<List<FichaSelectDTO>> ObtenerFichasAsync(CancellationToken ct = default)
    {
        return await context.Ficha
            .Where(f => f.Estado != "Cancelada")
            .OrderBy(f => f.NumeroFicha)
            .Select(f => new FichaSelectDTO { IdFicha = f.IdFicha, NumeroFicha = f.NumeroFicha })
            .ToListAsync(ct);
    }

    public async Task<int?> ObtenerIdInstructorPorUsuarioAsync(int idUsuario, CancellationToken ct = default)
    {
        return await context.InstructorPerfil
            .Where(ip => ip.IdUsuario == idUsuario)
            .Select(ip => (int?)ip.IdInstructor)
            .FirstOrDefaultAsync(ct);
    }

    // ─── POR FICHA ───────────────────────────────────────────────────────────

    public async Task<List<DocumentoListadoDTO>> ObtenerGuiasPorFichaAsync(string numeroFicha, CancellationToken ct = default)
    {
        return await (
            from g  in context.GuiaAprendizaje
            join a  in context.Archivo         on g.IdDocumento        equals a.IdArchivo
            join fc in context.FichaCompetencia on g.IdFichaCompetencia equals fc.IdFichaCompetencia
            join fi in context.Ficha           on fc.IdFicha            equals fi.IdFicha
            join co in context.Competencia     on fc.IdCompetencia      equals co.IdCompetencia
            join ip in context.InstructorPerfil on g.IdInstructor       equals ip.IdInstructor
            join us in context.Usuario         on ip.IdUsuario          equals us.IdUsuario
            join pe in context.Persona         on us.IdPersona          equals pe.IdPersona
            where g.Activo && a.Activo && fi.NumeroFicha == numeroFicha
            orderby g.FechaCreacion descending
            select new DocumentoListadoDTO
            {
                IdDocumento        = g.IdGuia,
                TipoDocumento      = "Guia",
                Nombre             = g.Titulo,
                Descripcion        = g.Descripcion,
                Estado             = g.Estado,
                Version            = g.Version,
                FechaCreacion      = g.FechaCreacion,
                IdInstructor       = g.IdInstructor,
                InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                EntidadRelacionada = co.Nombre,
                IdArchivo          = a.IdArchivo,
                NombreOriginal     = a.NombreOriginal,
                ExtensionArchivo   = a.ExtensionArchivo,
                TamanioBytes       = a.TamanioBytes
            }
        ).ToListAsync(ct);
    }

    public async Task<List<DocumentoListadoDTO>> ObtenerPlaneacionesPorFichaAsync(string numeroFicha, CancellationToken ct = default)
    {
        return await (
            from x  in context.PlaneacionPedagogica
            join a  in context.Archivo          on x.IdDocumento        equals a.IdArchivo
            join fc in context.FichaCompetencia  on x.IdFichaCompetencia equals fc.IdFichaCompetencia
            join fi in context.Ficha            on fc.IdFicha            equals fi.IdFicha
            join co in context.Competencia      on fc.IdCompetencia      equals co.IdCompetencia
            join ip in context.InstructorPerfil  on x.IdInstructor       equals ip.IdInstructor
            join us in context.Usuario          on ip.IdUsuario          equals us.IdUsuario
            join pe in context.Persona          on us.IdPersona          equals pe.IdPersona
            where x.Activo && a.Activo && fi.NumeroFicha == numeroFicha
            orderby x.FechaCreacion descending
            select new DocumentoListadoDTO
            {
                IdDocumento        = x.IdPlaneacion,
                TipoDocumento      = "Planeacion",
                Nombre             = "Planeación – " + co.Nombre,
                Descripcion        = x.Descripcion,
                Estado             = x.Estado,
                Version            = x.Version,
                FechaCreacion      = x.FechaCreacion,
                IdInstructor       = x.IdInstructor,
                InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                EntidadRelacionada = co.Nombre,
                IdArchivo          = a.IdArchivo,
                NombreOriginal     = a.NombreOriginal,
                ExtensionArchivo   = a.ExtensionArchivo,
                TamanioBytes       = a.TamanioBytes
            }
        ).ToListAsync(ct);
    }

    public async Task<List<DocumentoListadoDTO>> ObtenerProyectosPorFichaAsync(string numeroFicha, CancellationToken ct = default)
    {
        return await (
            from x  in context.ProyectoFormativo
            join a  in context.Archivo          on x.IdDocumento  equals a.IdArchivo
            join fi in context.Ficha            on x.IdFicha      equals fi.IdFicha
            join ip in context.InstructorPerfil  on x.IdInstructor equals ip.IdInstructor
            join us in context.Usuario          on ip.IdUsuario   equals us.IdUsuario
            join pe in context.Persona          on us.IdPersona   equals pe.IdPersona
            where x.Activo && a.Activo && fi.NumeroFicha == numeroFicha
            orderby x.FechaCreacion descending
            select new DocumentoListadoDTO
            {
                IdDocumento        = x.IdProyecto,
                TipoDocumento      = "Proyecto",
                Nombre             = x.Titulo,
                Descripcion        = x.Descripcion,
                Estado             = x.Estado,
                Version            = x.Version,
                FechaCreacion      = x.FechaCreacion,
                IdInstructor       = x.IdInstructor,
                InstructorNombre   = pe.Nombres + " " + pe.Apellidos,
                EntidadRelacionada = "Ficha " + fi.NumeroFicha,
                IdArchivo          = a.IdArchivo,
                NombreOriginal     = a.NombreOriginal,
                ExtensionArchivo   = a.ExtensionArchivo,
                TamanioBytes       = a.TamanioBytes
            }
        ).ToListAsync(ct);
    }
}
