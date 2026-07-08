using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class AprendizQueryService(AdsoDbContext context) : IAprendizQueryService
{
    public async Task<List<AprendizListadoDTO>> ObtenerTodosAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var raw = await context.AprendizPerfil
            .Include(a => a.Asistencias).ThenInclude(asi => asi.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia)
            .Select(a => new
            {
                a.IdAprendiz,
                IdFicha = a.FichaAprendices
                    .OrderByDescending(fa => fa.Ficha.FechaInicio)
                    .Select(fa => fa.IdFicha)
                    .FirstOrDefault(),
                Nombres = string.Concat(a.Persona.Nombres, " ", a.Persona.Apellidos),
                a.Persona.NumeroDocumento,
                Ficha = a.Fichas.Select(f => f.NumeroFicha).ToList(),
                Estado = a.FichaAprendices
                    .OrderByDescending(fa => fa.Ficha.FechaInicio)
                    .Select(fa => fa.Estado)
                    .FirstOrDefault() ?? "",
                TotalSesiones   = a.Asistencias.Count(asi => asi.Sesion.Fecha <= today),
                SesionesAsistidas = a.Asistencias.Count(asi =>
                    asi.Sesion.Fecha <= today &&
                    (asi.Estado == EstadosAsistencia.Presente || asi.Estado == EstadosAsistencia.Justificado)),
                a.Asistencias
            }).ToListAsync();

        return raw.Select(a =>
        {
            var pct = a.TotalSesiones == 0 ? 100m
                : (decimal)a.SesionesAsistidas * 100m / a.TotalSesiones;
            return new AprendizListadoDTO
            {
                IdAprendiz           = a.IdAprendiz,
                IdFicha              = a.IdFicha,
                Nombres              = a.Nombres,
                NumeroDocumento      = a.NumeroDocumento,
                Ficha                = a.Ficha,
                Estado               = a.Estado,
                PorcentajeAsistencia = Math.Round(pct, 1),
                IndicadorAsistencia  = AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(a.Asistencias) ? "Atrasado" : "Al día"
            };
        }).ToList();
    }

    public async Task<List<AprendizListadoDTO>> ObtenerPorFichaAsync(string numeroFicha)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var raw = await context.FichaAprendiz
            .Where(fa => fa.Ficha.NumeroFicha == numeroFicha)
            .Include(fa => fa.Aprendiz).ThenInclude(a => a.Asistencias).ThenInclude(asi => asi.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia)
            .Select(fa => new
            {
                IdAprendiz      = fa.Aprendiz.IdAprendiz,
                IdFicha         = fa.IdFicha,
                Nombres         = string.Concat(fa.Aprendiz.Persona.Nombres, " ", fa.Aprendiz.Persona.Apellidos),
                NumeroDocumento = fa.Aprendiz.Persona.NumeroDocumento,
                Ficha           = fa.Aprendiz.Fichas.Select(f => f.NumeroFicha).ToList(),
                Estado          = fa.Estado,
                TotalSesiones   = fa.Aprendiz.Asistencias.Count(asi => asi.Sesion.Fecha <= today),
                SesionesAsistidas = fa.Aprendiz.Asistencias.Count(asi =>
                    asi.Sesion.Fecha <= today &&
                    (asi.Estado == EstadosAsistencia.Presente || asi.Estado == EstadosAsistencia.Justificado)),
                fa.Aprendiz.Asistencias
            })
            .ToListAsync();

        return raw.Select(fa =>
        {
            var pct = fa.TotalSesiones == 0 ? 100m
                : (decimal)fa.SesionesAsistidas * 100m / fa.TotalSesiones;
            return new AprendizListadoDTO
            {
                IdAprendiz           = fa.IdAprendiz,
                IdFicha              = fa.IdFicha,
                Nombres              = fa.Nombres,
                NumeroDocumento      = fa.NumeroDocumento,
                Ficha                = fa.Ficha,
                Estado               = fa.Estado,
                PorcentajeAsistencia = Math.Round(pct, 1),
                IndicadorAsistencia  = AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(fa.Asistencias) ? "Atrasado" : "Al día"
            };
        }).ToList();
    }

    public async Task<AprendizDetalleDTO?> ObtenerDetallesAsync(int idAprendiz)
    {
        var aprendiz = await context.AprendizPerfil
            .Include(a => a.Persona).ThenInclude(p => p.TipoDocumento)
            .Include(a => a.FichaAprendices).ThenInclude(fa => fa.Ficha)
            .Include(a => a.Asistencias).ThenInclude(asi => asi.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia)
            .FirstOrDefaultAsync(a => a.IdAprendiz == idAprendiz);

        if (aprendiz == null) return null;

        var today = DateOnly.FromDateTime(DateTime.Today);
        var totalSesiones = aprendiz.Asistencias.Count(asi => asi.Sesion.Fecha <= today);
        var asistidas = aprendiz.Asistencias.Count(asi =>
                asi.Sesion.Fecha <= today &&
                (asi.Estado == EstadosAsistencia.Presente || asi.Estado == EstadosAsistencia.Justificado));

        var pct = totalSesiones == 0 ? 100m : (decimal)asistidas * 100m / totalSesiones;

        return new AprendizDetalleDTO
        {
            Apellidos = aprendiz.Persona.Apellidos,
            Nombres = aprendiz.Persona.Nombres,
            CondicionEspecial = aprendiz.CondicionEspecial,
            ContactoEmergenciaNombre = aprendiz.ContactoEmergenciaNombre,
            ContactoEmergenciaTelefono = aprendiz.ContactoEmergenciaTelefono,
            Direccion = aprendiz.Persona.Direccion,
            Estrato = aprendiz.Estrato,
            FechaNacimiento = aprendiz.Persona.FechaNacimiento,
            Municipio = aprendiz.Persona.Municipio,
            NumeroDocumento = aprendiz.Persona.NumeroDocumento,
            Telefono = aprendiz.Persona.Telefono,
            TipoDocumento = aprendiz.Persona.TipoDocumento.Nombre,
            TipoPoblacion = aprendiz.TipoPoblacion,
            Estado = aprendiz.FichaAprendices
                .OrderByDescending(fa => fa.Ficha.FechaInicio)
                .Select(fa => fa.Estado)
                .FirstOrDefault() ?? "",
            Ficha = aprendiz.FichaAprendices.Select(fa => fa.Ficha.NumeroFicha).ToList(),
            PorcentajeAsistencia = Math.Round(pct, 1),
            IndicadorAsistencia  = AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(aprendiz.Asistencias) ? "Atrasado" : "Al día"
        };
    }

    public async Task<List<AprendizListadoDTO>> FiltrarAsync(
    string? numeroFicha,
    string? numeroDocumento,
    string? nombreAprendiz)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        IQueryable<AprendizPerfil> query = context.AprendizPerfil;

        var ignorarFiltroFicha = !string.IsNullOrEmpty(numeroDocumento);

        if (!string.IsNullOrEmpty(numeroFicha) && !ignorarFiltroFicha)
            query = query.Where(a =>
                a.FichaAprendices.Any(fa => fa.Ficha.NumeroFicha == numeroFicha));

        if (!string.IsNullOrEmpty(numeroDocumento))
            query = query.Where(a =>
                a.Persona.NumeroDocumento.Contains(numeroDocumento));

        if (!string.IsNullOrEmpty(nombreAprendiz))
            query = query.Where(a =>
                EF.Functions.Like(
                    a.Persona.Nombres + " " + a.Persona.Apellidos,
                    $"%{nombreAprendiz}%"));

        var raw = await query
            .Include(a => a.Asistencias).ThenInclude(asi => asi.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia)
            .Select(a => new
            {
                IdAprendiz = a.IdAprendiz,
                IdFicha    = (!string.IsNullOrEmpty(numeroFicha) && !ignorarFiltroFicha)
                    ? a.FichaAprendices
                        .Where(fa => fa.Ficha.NumeroFicha == numeroFicha)
                        .Select(fa => fa.IdFicha)
                        .FirstOrDefault()
                    : a.FichaAprendices
                        .OrderByDescending(fa => fa.Ficha.FechaInicio)
                        .Select(fa => fa.IdFicha)
                        .FirstOrDefault(),
                Nombres = a.Persona.Nombres + " " + a.Persona.Apellidos,
                NumeroDocumento = a.Persona.NumeroDocumento,

                Ficha = (!string.IsNullOrEmpty(numeroFicha) && !ignorarFiltroFicha)
                    ? a.FichaAprendices
                        .Where(fa => fa.Ficha.NumeroFicha == numeroFicha)
                        .Select(fa => fa.Ficha.NumeroFicha)
                        .ToList()
                    : a.FichaAprendices
                        .Select(fa => fa.Ficha.NumeroFicha)
                        .ToList(),

                Estado = (!string.IsNullOrEmpty(numeroFicha) && !ignorarFiltroFicha)
                    ? a.FichaAprendices
                        .Where(fa => fa.Ficha.NumeroFicha == numeroFicha)
                        .Select(fa => fa.Estado)
                        .FirstOrDefault() ?? ""
                    : a.FichaAprendices
                        .OrderByDescending(fa => fa.Ficha.FechaInicio)
                        .Select(fa => fa.Estado)
                        .FirstOrDefault() ?? "",

                TotalSesiones   = a.Asistencias.Count(asi => asi.Sesion.Fecha <= today),
                SesionesAsistidas = a.Asistencias.Count(asi =>
                    asi.Sesion.Fecha <= today &&
                    (asi.Estado == EstadosAsistencia.Presente || asi.Estado == EstadosAsistencia.Justificado)),
                a.Asistencias
            })
            .ToListAsync();

        return raw.Select(a =>
        {
            var pct = a.TotalSesiones == 0 ? 100m
                : (decimal)a.SesionesAsistidas * 100m / a.TotalSesiones;
            return new AprendizListadoDTO
            {
                IdAprendiz           = a.IdAprendiz,
                IdFicha              = a.IdFicha,
                Nombres              = a.Nombres,
                NumeroDocumento      = a.NumeroDocumento,
                Ficha                = a.Ficha,
                Estado               = a.Estado,
                PorcentajeAsistencia = Math.Round(pct, 1),
                IndicadorAsistencia  = AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(a.Asistencias) ? "Atrasado" : "Al día"
            };
        }).ToList();
    }

    public async Task<ResumenAprendizDto> ObtenerResumenAsync(string? numeroFicha, string? numeroDocumento, string? nombreAprendiz)
    {
        IQueryable<FichaAprendiz> query = context.FichaAprendiz
            .Include(fa => fa.Aprendiz).ThenInclude(a => a.Persona)
            .Include(fa => fa.Aprendiz).ThenInclude(a => a.Asistencias).ThenInclude(asi => asi.Sesion).ThenInclude(s => s.FichaCompetencia).ThenInclude(fc => fc.Competencia);

        if (!string.IsNullOrWhiteSpace(numeroFicha))
            query = query.Where(fa => fa.Ficha.NumeroFicha == numeroFicha);

        if (!string.IsNullOrWhiteSpace(numeroDocumento))
            query = query.Where(fa => fa.Aprendiz.Persona.NumeroDocumento.Contains(numeroDocumento));

        if (!string.IsNullOrWhiteSpace(nombreAprendiz))
            query = query.Where(fa => (fa.Aprendiz.Persona.Nombres + " " + fa.Aprendiz.Persona.Apellidos).Contains(nombreAprendiz));

        var fichasAprendices = await query.ToListAsync();

        return new ResumenAprendizDto
        {
            TotalAprendices = fichasAprendices.Count,
            TotalAprendicesActivos = fichasAprendices.Count(fa => fa.Estado == EstadosAprendiz.EnFormacion),
            TotalAprendicesEnRiesgo = fichasAprendices.Count(fa =>
                AsistenciaRiesgoCalculator.EsAprendizEnRiesgo(fa.Aprendiz.Asistencias))
        };
    }
}
