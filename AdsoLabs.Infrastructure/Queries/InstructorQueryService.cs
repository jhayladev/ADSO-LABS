using AdsoLabs.Application.DTOs.Instructor;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AdsoLabs.Infrastructure.Queries;

public class InstructorQueryService(AdsoDbContext context) : IInstructorQueryService
{
    public async Task<List<InstructorListadoDTO>> ObtenerTodosAsync()
        => await context.InstructorPerfil
            .Select(i => new InstructorListadoDTO
            {
                Competencias    = i.ResultadosProgramados
                    .Select(r => r.FichaCompetencia.Competencia.Nombre)
                    .Distinct().ToList(),
                Activo          = i.Activo,
                IdInstructor    = i.IdInstructor,
                Nombres         = string.Concat(i.Usuario.Persona.Nombres, " ", i.Usuario.Persona.Apellidos),
                Especialidad    = i.Especialidad,
                TipoVinculacion = i.TipoVinculacion
            }).ToListAsync();

    public async Task<InstructorDetalleDTO?> ObtenerDetallesAsync(int idInstructor)
    {
        var instructor = await context.InstructorPerfil
            .Include(a => a.Usuario)
                .ThenInclude(u => u.Persona)
                    .ThenInclude(p => p.TipoDocumento)
            .Include(a => a.ResultadosProgramados)
                .ThenInclude(r => r.FichaCompetencia)
                    .ThenInclude(fc => fc.Ficha)
            .FirstOrDefaultAsync(i => i.IdInstructor == idInstructor);

        if (instructor == null) return null;

        // Obtener IDs de fichas únicas del instructor
        var fichasBase = instructor.ResultadosProgramados
            .Select(r => r.FichaCompetencia.Ficha)
            .DistinctBy(f => f.IdFicha)
            .ToList();

        var fichaIds = fichasBase.Select(f => f.IdFicha).ToList();

        // Calcular progreso con la misma fórmula que FichaQueryService:
        // denominador = TotalHoras ?? Competencia.HorasAsignadas (no ?? 0)
        var fichaData = await context.FichaCompetencia
            .Where(fc => fichaIds.Contains(fc.IdFicha))
            .Select(fc => new
            {
                fc.IdFicha,
                HorasTotal = fc.TotalHoras ?? fc.Competencia.HorasAsignadas,
                fc.HorasEjecutadas
            })
            .ToListAsync();

        var progresoPorFicha = fichaData
            .GroupBy(x => x.IdFicha)
            .ToDictionary(g => g.Key, g =>
            {
                var total = g.Sum(x => x.HorasTotal);
                var ejec  = g.Sum(x => x.HorasEjecutadas);
                return total == 0 ? 0 : (int)Math.Round(100.0 * ejec / total);
            });

        var fichasAsignadas = fichasBase
            .Select(f => new FichaResumenInstructorDTO
            {
                NumeroFicha = f.NumeroFicha,
                Programa    = f.Nombre,
                Jornada     = f.Jornada,
                Modalidad   = f.Modalidad,
                Activa      = f.Estado == "Activa",
                Progreso    = progresoPorFicha.TryGetValue(f.IdFicha, out var pct) ? pct : 0
            }).ToList();

        return new InstructorDetalleDTO
        {
            IdUsuario       = instructor.IdUsuario,
            Apellidos       = instructor.Usuario.Persona.Apellidos,
            Nombres         = instructor.Usuario.Persona.Nombres,
            Direccion       = instructor.Usuario.Persona.Direccion,
            Activo          = instructor.Activo,
            FechaNacimiento = instructor.Usuario.Persona.FechaNacimiento,
            Municipio       = instructor.Usuario.Persona.Municipio,
            NumeroDocumento = instructor.Usuario.Persona.NumeroDocumento,
            Telefono        = instructor.Usuario.Persona.Telefono,
            TipoDocumento   = instructor.Usuario.Persona.TipoDocumento.Nombre,
            Correo          = instructor.Usuario.Correo,
            Especialidad    = instructor.Especialidad,
            FechaVinculacion = instructor.FechaVinculacion,
            FichasAsignadas = fichasAsignadas,
            NumeroContrato  = instructor.NumeroContrato,
            TipoVinculacion = instructor.TipoVinculacion
        };
    }

    public async Task<List<InstructorListadoDTO>> FiltrarAsync(string? nombreCompetencia, string? nombreInstructor)
    {
        var query = context.InstructorPerfil
            .Select(i => new InstructorListadoDTO
            {
                Competencias    = i.ResultadosProgramados
                    .Select(r => r.FichaCompetencia.Competencia.Nombre)
                    .Distinct().ToList(),
                Activo          = i.Activo,
                IdInstructor    = i.IdInstructor,
                Nombres         = string.Concat(i.Usuario.Persona.Nombres, " ", i.Usuario.Persona.Apellidos),
                Especialidad    = i.Especialidad,
                TipoVinculacion = i.TipoVinculacion
            }).AsQueryable();

        if (!string.IsNullOrEmpty(nombreCompetencia))
            query = query.Where(u => u.Competencias.Any(c => c == nombreCompetencia));

        if (!string.IsNullOrEmpty(nombreInstructor))
            query = query.Where(u => u.Nombres.Contains(nombreInstructor));

        return await query.ToListAsync();
    }

    public async Task<List<InstructorListadoDTO>> ObtenerPorCompetenciaAsync(string nombreCompetencia)
        => await context.InstructorPerfil
            .Select(i => new InstructorListadoDTO
            {
                Competencias    = i.ResultadosProgramados
                    .Select(r => r.FichaCompetencia.Competencia.Nombre)
                    .Distinct().ToList(),
                Activo          = i.Activo,
                IdInstructor    = i.IdInstructor,
                Nombres         = string.Concat(i.Usuario.Persona.Nombres, " ", i.Usuario.Persona.Apellidos),
                Especialidad    = i.Especialidad,
                TipoVinculacion = i.TipoVinculacion
            })
            .Where(u => u.Competencias.Any(c => c == nombreCompetencia))
            .ToListAsync();

    public async Task<ResumenInstructorDto> ObtenerResumenAsync(string? nombreCompetencia, string? nombreInstructor)
    {
        IQueryable<InstructorPerfil> query = context.InstructorPerfil
            .Include(a => a.ResultadosProgramados)
                .ThenInclude(r => r.FichaCompetencia)
                    .ThenInclude(fc => fc.Competencia);

        if (!string.IsNullOrWhiteSpace(nombreCompetencia))
            query = query.Where(i => i.ResultadosProgramados
                .Any(r => r.FichaCompetencia.Competencia.Nombre == nombreCompetencia));

        if (!string.IsNullOrWhiteSpace(nombreInstructor))
            query = query.Where(i =>
                string.Concat(i.Usuario.Persona.Nombres, " ", i.Usuario.Persona.Apellidos)
                .Contains(nombreInstructor));

        return new ResumenInstructorDto
        {
            TotalInstructores = await query.CountAsync(),
            TotalInstructoresActivos = await query.CountAsync(i => i.Activo),
            TotalInstructoresTecnicos = await query.CountAsync(i =>
                i.ResultadosProgramados
                    .Any(r => r.FichaCompetencia.Competencia.Tipo == "Tecnica"))
        };
    }

    public async Task<List<HorarioInstructorItemDTO>> ObtenerHorarioAsync(int idInstructor)
        => await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdInstructor == idInstructor && fcr.Estado != "Pendiente")
            .OrderBy(fcr => fcr.FechaInicio).ThenBy(fcr => fcr.HoraInicio)
            .Select(fcr => new HorarioInstructorItemDTO
            {
                NumeroFicha       = fcr.FichaCompetencia.Ficha.NumeroFicha,
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre,
                CodigoResultado   = fcr.Resultado.Codigo,
                NombreResultado   = fcr.Resultado.Nombre,
                FechaInicio       = fcr.FechaInicio,
                FechaFin          = fcr.FechaFin,
                HoraInicio        = fcr.HoraInicio,
                HoraFin           = fcr.HoraFin,
                Estado            = fcr.Estado
            })
            .ToListAsync();

    public async Task<List<AvisoInstructorDTO>> ObtenerAvisosAsync(int idInstructor)
    {
        var avisos = new List<AvisoInstructorDTO>();
        var hoy     = DateOnly.FromDateTime(DateTime.Today);
        var en30    = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        // 1. Resultados en estado Programado (pendientes de completar)
        var resultadosPendientes = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdInstructor == idInstructor && fcr.Estado == "Programado")
            .Select(fcr => new
            {
                NombreResultado   = fcr.Resultado.Nombre,
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre,
                NumeroFicha       = fcr.FichaCompetencia.Ficha.NumeroFicha
            })
            .Distinct()
            .ToListAsync();

        foreach (var r in resultadosPendientes)
        {
            avisos.Add(new AvisoInstructorDTO
            {
                Tipo        = "SinCalificar",
                Titulo      = "Resultado pendiente de completar",
                Descripcion = $"Competencia: {r.NombreCompetencia} — Resultado: \"{r.NombreResultado}\" (Ficha {r.NumeroFicha})."
            });
        }

        // 2. Aprendices con 3 o más ausencias en sesiones del instructor
        var ausenciasPorAprendiz = await context.Asistencia
            .Where(a => a.Sesion.IdInstructor == idInstructor && a.Estado == "Ausente")
            .GroupBy(a => new
            {
                a.IdAprendiz,
                NumeroFicha = a.Sesion.FichaCompetencia.Ficha.NumeroFicha
            })
            .Where(g => g.Count() >= 3)
            .Select(g => new { g.Key.IdAprendiz, g.Key.NumeroFicha, Ausencias = g.Count() })
            .ToListAsync();

        if (ausenciasPorAprendiz.Any())
        {
            var aprendizIds = ausenciasPorAprendiz.Select(a => a.IdAprendiz).Distinct().ToList();
            var nombresMap  = await context.AprendizPerfil
                .Where(ap => aprendizIds.Contains(ap.IdAprendiz))
                .Select(ap => new
                {
                    ap.IdAprendiz,
                    Nombre = ap.Usuario.Persona.Nombres + " " + ap.Usuario.Persona.Apellidos
                })
                .ToDictionaryAsync(ap => ap.IdAprendiz, ap => ap.Nombre);

            foreach (var a in ausenciasPorAprendiz)
            {
                var nombre = nombresMap.TryGetValue(a.IdAprendiz, out var n) ? n : $"Aprendiz #{a.IdAprendiz}";
                avisos.Add(new AvisoInstructorDTO
                {
                    Tipo        = "AprendizRiesgo",
                    Titulo      = "Aprendiz con inasistencias",
                    Descripcion = $"{nombre} acumula {a.Ausencias} ausencias en la ficha {a.NumeroFicha}."
                });
            }
        }

        // 3. Fichas con FechaFin en los próximos 30 días
        var fichasVenciendo = await context.FichaCompetenciaResultado
            .Where(fcr => fcr.IdInstructor == idInstructor &&
                          fcr.FichaCompetencia.Ficha.FechaFin.HasValue &&
                          fcr.FichaCompetencia.Ficha.FechaFin.Value >= hoy &&
                          fcr.FichaCompetencia.Ficha.FechaFin.Value <= en30)
            .Select(fcr => new
            {
                NumeroFicha = fcr.FichaCompetencia.Ficha.NumeroFicha,
                FechaFin    = fcr.FichaCompetencia.Ficha.FechaFin!.Value
            })
            .Distinct()
            .ToListAsync();

        foreach (var f in fichasVenciendo)
        {
            avisos.Add(new AvisoInstructorDTO
            {
                Tipo        = "FichaVence",
                Titulo      = "Ficha próxima a cerrar",
                Descripcion = $"La ficha {f.NumeroFicha} finaliza el {f.FechaFin.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}."
            });
        }

        return avisos;
    }
}
