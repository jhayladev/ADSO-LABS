using AdsoLabs.Application.DTOs.PrediccionIA;
using AdsoLabs.Application.Interfaces.Services;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using AdsoLabs.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AdsoLabs.Infrastructure.Services;

public class PrediccionIAService(
    AdsoDbContext context,
    IAiAnalysisService ai,
    IMemoryCache cache) : IPrediccionIAService
{
    private Task<int?> ResolverIdInstructorAsync(int idUsuario) =>
        context.InstructorPerfil
            .Where(ip => ip.IdUsuario == idUsuario)
            .Select(ip => (int?)ip.IdInstructor)
            .FirstOrDefaultAsync();

    public async Task<CompatibilidadResponseDTO> ObtenerCompatibilidadAsync(int idUsuario, CancellationToken ct = default)
    {
        var cacheKey = $"pia_compat_{idUsuario}";
        if (cache.TryGetValue(cacheKey, out CompatibilidadResponseDTO? cached) && cached != null)
            return cached;

        var idInstructor = await ResolverIdInstructorAsync(idUsuario);
        if (!idInstructor.HasValue)
            throw new InvalidOperationException("El usuario no tiene perfil de instructor.");

        var instructor = await context.InstructorPerfil
            .Where(ip => ip.IdInstructor == idInstructor.Value)
            .Select(ip => new
            {
                NombreCompleto = ip.Usuario.Persona.Nombres + " " + ip.Usuario.Persona.Apellidos,
                ip.Especialidad
            })
            .FirstAsync(ct);

        // Historial: competencias que el instructor ha impartido antes
        // Excluir inducción y etapa práctica/productiva (código 590803 o nombre clave)
        var historicas = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdInstructor == idInstructor.Value
                          && fcr.FichaCompetencia.Competencia.Codigo != "590803"
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("ETAPA PRACTICA")
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("ETAPA PRODUCTIVA")
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("INDUCCION")
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("INDUCCIÓN"))
            .GroupBy(fcr => new
            {
                fcr.FichaCompetencia.Competencia.Codigo,
                fcr.FichaCompetencia.Competencia.Nombre
            })
            .Select(g => new CompetenciaContextItem(
                g.Key.Nombre,
                g.Key.Codigo,
                g.Select(x => x.IdFichaCompetencia).Distinct().Count(),
                0))
            .ToListAsync(ct);

        // Sugerencias: solo ficha-competencias sin ninguna asignación existente (Estado == "Pendiente").
        // Excluir inducción y etapa práctica/productiva
        // Cargamos sin límite y luego muestreamos en memoria (máx 4 por ficha, máx 40 total)
        // para evitar que una ficha con muchas competencias domine el contexto de la IA
        var fichasCtxAll = await context.FichaCompetencia
            .Where(fc => fc.Ficha.Estado == "Activa"
                         && fc.Estado == "Pendiente"
                         && fc.Competencia.Estado != "Clausurada"
                         && fc.Competencia.Codigo != "590803"
                         && !fc.Competencia.Nombre.Contains("ETAPA PRACTICA")
                         && !fc.Competencia.Nombre.Contains("ETAPA PRODUCTIVA")
                         && !fc.Competencia.Nombre.Contains("INDUCCION")
                         && !fc.Competencia.Nombre.Contains("INDUCCIÓN"))
            .Select(fc => new FichaCompetenciaContextItem(
                fc.IdFichaCompetencia,
                fc.Competencia.Nombre,
                fc.Competencia.Codigo,
                fc.Ficha.NumeroFicha,
                fc.Ficha.Jornada ?? "Presencial",
                fc.Competencia.FaseFormativa,
                0))
            .ToListAsync(ct);

        // Fase actual de cada ficha, calculada sobre TODAS sus ficha-competencias
        // (no solo las candidatas Pendientes) para saber en qué punto real va.
        var numerosFicha = fichasCtxAll.Select(fc => fc.NumeroFicha).Distinct().ToList();
        var todasFcPorFicha = await context.FichaCompetencia
            .Where(fc => numerosFicha.Contains(fc.Ficha.NumeroFicha))
            .Select(fc => new { fc.Ficha.NumeroFicha, fc.Competencia.FaseFormativa, fc.Estado })
            .ToListAsync(ct);

        var faseActualPorFicha = todasFcPorFicha
            .GroupBy(x => x.NumeroFicha)
            .ToDictionary(
                g => g.Key,
                g => FaseFormativaHelper.CalcularFaseActualFicha(
                    g.Select(x => (x.FaseFormativa, x.Estado))));

        var fichasCtx = fichasCtxAll
            .Where(fc => FaseFormativaHelper.EsSugerible(fc.FaseFormativa, faseActualPorFicha[fc.NumeroFicha]))
            .GroupBy(fc => fc.NumeroFicha)
            .SelectMany(g => g.Take(4))
            .Take(40)
            .ToList();

        var iaCtx = new InstructorIAContext(
            instructor.NombreCompleto,
            instructor.Especialidad ?? "General",
            historicas,
            fichasCtx);

        var aiResult = await ai.AnalizarCompatibilidadAsync(iaCtx, ct);

        var solicitudesPendientesIds = (await context.SolicitudAsignacion
            .Where(s => s.IdInstructor == idInstructor.Value && s.Estado == "Pendiente")
            .Select(s => s.IdFichaCompetencia)
            .ToListAsync(ct)).ToHashSet();

        var fichasDic = fichasCtx.ToDictionary(f => f.IdFichaCompetencia);
        var sugerencias = aiResult.Sugerencias
            .Where(s => fichasDic.ContainsKey(s.IdFichaCompetencia))
            .Select(s =>
            {
                var f = fichasDic[s.IdFichaCompetencia];
                return new SugerenciaDTO
                {
                    IdFichaCompetencia = s.IdFichaCompetencia,
                    NombreCompetencia = f.NombreCompetencia,
                    NumeroFicha = f.NumeroFicha,
                    Fase = f.Jornada,
                    FaseFormativa = f.FaseFormativa,
                    FaseActualFicha = FaseFormativaHelper.NombreFase(faseActualPorFicha[f.NumeroFicha]),
                    ProgresoFicha = f.ProgresoFicha,
                    Compatibilidad = s.Compatibilidad,
                    Razonamiento = s.Razonamiento,
                    YaTieneSolicitudPendiente = solicitudesPendientesIds.Contains(s.IdFichaCompetencia)
                };
            })
            .OrderByDescending(s => s.Compatibilidad)
            .ToList();

        var result = new CompatibilidadResponseDTO
        {
            NombreInstructor = instructor.NombreCompleto,
            Especialidad = instructor.Especialidad ?? "General",
            PorcentajeGeneral = aiResult.PorcentajeGeneral,
            ResumenIA = aiResult.ResumenIA,
            FortalezasDetectadas = aiResult.Fortalezas.ToList(),
            Sugerencias = sugerencias
        };

        cache.Set(cacheKey, result, TimeSpan.FromMinutes(60));
        return result;
    }

    public async Task<List<CompetenciaAsignadaDTO>> ObtenerCompetenciasAsignadasAsync(int idUsuario, CancellationToken ct = default)
    {
        var idInstructor = await ResolverIdInstructorAsync(idUsuario);
        if (!idInstructor.HasValue)
            throw new InvalidOperationException("El usuario no tiene perfil de instructor.");

        // Proyecto primero a objetos planos; excluir inducción y etapa práctica/productiva
        var fcrs = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdInstructor == idInstructor.Value
                          && fcr.FichaCompetencia.Competencia.Codigo != "590803"
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("ETAPA PRACTICA")
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("ETAPA PRODUCTIVA")
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("INDUCCION")
                          && !fcr.FichaCompetencia.Competencia.Nombre.Contains("INDUCCIÓN"))
            .Select(fcr => new
            {
                fcr.IdFichaCompetencia,
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre,
                CodigoCompetencia = fcr.FichaCompetencia.Competencia.Codigo,
                NumeroFicha = fcr.FichaCompetencia.Ficha.NumeroFicha,
                NombreResultado = fcr.Resultado.Nombre
            })
            .ToListAsync(ct);

        // Agrupar en memoria por FichaCompetencia
        return fcrs
            .GroupBy(x => x.IdFichaCompetencia)
            .Select(g => new CompetenciaAsignadaDTO
            {
                IdFichaCompetencia = g.Key,
                NombreCompetencia = g.First().NombreCompetencia,
                CodigoCompetencia = g.First().CodigoCompetencia,
                NumeroFicha = g.First().NumeroFicha,
                Resultados = g.Select(x => x.NombreResultado).Distinct().ToList()
            })
            .ToList();
    }

    public async Task<PrediccionAprendizajeDTO> ObtenerPrediccionAprendizajeAsync(int idUsuario, int idFichaCompetencia, bool forzarActualizacion = false, CancellationToken ct = default)
    {
        var cacheKey = $"pia_pred_{idUsuario}_{idFichaCompetencia}";
        if (!forzarActualizacion && cache.TryGetValue(cacheKey, out PrediccionAprendizajeDTO? cached) && cached != null)
            return cached;

        var idInstructor = await ResolverIdInstructorAsync(idUsuario);
        if (!idInstructor.HasValue)
            throw new InvalidOperationException("El usuario no tiene perfil de instructor.");

        var instructor = await context.InstructorPerfil
            .Where(ip => ip.IdInstructor == idInstructor.Value)
            .Select(ip => new
            {
                NombreCompleto = ip.Usuario.Persona.Nombres + " " + ip.Usuario.Persona.Apellidos,
                ip.Especialidad
            })
            .FirstAsync(ct);

        // Obtener la competencia seleccionada y sus resultados asignados al instructor
        var fcrs = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdFichaCompetencia == idFichaCompetencia
                          && fcr.IdInstructor == idInstructor.Value)
            .Select(fcr => new
            {
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre,
                CodigoCompetencia = fcr.FichaCompetencia.Competencia.Codigo,
                NumeroFicha = fcr.FichaCompetencia.Ficha.NumeroFicha,
                DescripcionCompetencia = fcr.FichaCompetencia.Competencia.Descripcion,
                NombreResultado = fcr.Resultado.Nombre
            })
            .ToListAsync(ct);

        if (fcrs.Count == 0)
            throw new InvalidOperationException("No tienes resultados asignados en la competencia seleccionada.");

        var primera = fcrs.First();
        var iaCtx = new PrediccionCompetenciaContext(
            instructor.NombreCompleto,
            instructor.Especialidad ?? "General",
            primera.NombreCompetencia,
            primera.CodigoCompetencia,
            primera.NumeroFicha,
            primera.DescripcionCompetencia,
            fcrs.Select(x => x.NombreResultado).Distinct().ToList());

        var aiResult = await ai.PredecirAprendizajeAsync(iaCtx, ct);

        var dto = new PrediccionAprendizajeDTO
        {
            TendenciaGeneral = aiResult.TendenciaGeneral,
            DescripcionTendencia = aiResult.DescripcionTendencia,
            Tematicas = aiResult.Tematicas.Select(t => new TematicaDTO
            {
                Numero = t.Numero, Icono = t.Icono, Titulo = t.Titulo,
                Descripcion = t.Descripcion, Relevancia = t.Relevancia
            }).ToList(),
            RutaAprendizaje = aiResult.RutaAprendizaje.Select(p => new PasoRutaDTO
            {
                Num = p.Num, Titulo = p.Titulo, Descripcion = p.Descripcion
            }).ToList(),
            Kpis = aiResult.Kpis.Select(k => new KpiPrediccionDTO
            {
                Etiqueta = k.Etiqueta, Valor = k.Valor, Icono = k.Icono, Color = k.Color
            }).ToList(),
            GeneradoEn = DateTime.Now
        };

        cache.Set(cacheKey, dto, TimeSpan.FromMinutes(60));
        return dto;
    }

    public async Task EnviarSolicitudAsync(EnviarSolicitudCommand command, CancellationToken ct = default)
    {
        var existe = await context.SolicitudAsignacion
            .AnyAsync(s => s.IdInstructor == command.IdInstructor
                           && s.IdFichaCompetencia == command.IdFichaCompetencia
                           && s.Estado == "Pendiente", ct);
        if (existe)
            throw new InvalidOperationException("Ya existe una solicitud pendiente para esta asignación.");

        context.SolicitudAsignacion.Add(new SolicitudAsignacion
        {
            IdInstructor = command.IdInstructor,
            IdFichaCompetencia = command.IdFichaCompetencia,
            RazonamientoIA = command.Razonamiento,
            PorcentajeCompatibilidad = command.PorcentajeCompatibilidad,
            FechaSolicitud = DateTime.Now,
            Estado = "Pendiente"
        });
        await context.SaveChangesAsync(ct);
    }

    public Task<List<SolicitudAsignacionDTO>> ObtenerSolicitudesAsync(CancellationToken ct = default) =>
        context.SolicitudAsignacion
            .OrderByDescending(s => s.FechaSolicitud)
            .Select(s => new SolicitudAsignacionDTO
            {
                IdSolicitud = s.IdSolicitud,
                NombreInstructor = s.Instructor.Usuario.Persona.Nombres + " " + s.Instructor.Usuario.Persona.Apellidos,
                NombreCompetencia = s.FichaCompetencia.Competencia.Nombre,
                NumeroFicha = s.FichaCompetencia.Ficha.NumeroFicha,
                PorcentajeCompatibilidad = s.PorcentajeCompatibilidad,
                RazonamientoIA = s.RazonamientoIA,
                Estado = s.Estado,
                FechaSolicitud = s.FechaSolicitud,
                ObservacionAdmin = s.ObservacionAdmin
            })
            .ToListAsync(ct);

    public async Task ResponderSolicitudAsync(ResponderSolicitudCommand command, CancellationToken ct = default)
    {
        var solicitud = await context.SolicitudAsignacion
            .FirstOrDefaultAsync(s => s.IdSolicitud == command.IdSolicitud, ct)
            ?? throw new InvalidOperationException("Solicitud no encontrada.");

        solicitud.Estado = command.Estado;
        solicitud.FechaRespuesta = DateTime.Now;
        solicitud.ObservacionAdmin = command.ObservacionAdmin;
        await context.SaveChangesAsync(ct);
    }
}
