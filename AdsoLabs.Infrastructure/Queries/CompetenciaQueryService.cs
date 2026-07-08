using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Competencia;
using AdsoLabs.Application.DTOs.Instructor;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries;

public class CompetenciaQueryService(AdsoDbContext context) : ICompetenciaQueryService
{
    public async Task<List<CompetenciaDTO>> ObtenerTodosAsync()
        => await context.Competencia.Select(c => new CompetenciaDTO
        {
            IdCompetencia     = c.IdCompetencia,
            CodigoCompetencia = c.Codigo,
            Estado            = c.Estado,
            HorasAsignadas    = c.HorasAsignadas,
            NombreCompetencia = c.Nombre,
            TipoCompetencia   = c.Tipo,
            FaseFormativa     = FaseFormativaHelper.NombreFase(c.FaseFormativa),
        }).ToListAsync();

    public async Task<FichaCompetenciaDTO> ObtenerDatosAsync(string nombreCompetencia)
    {
        var competencias = await context.FichaCompetencia
            .Include(f => f.Ficha)
            .Include(c => c.Competencia)
            .Include(fc => fc.ResultadosProgramados)
                .ThenInclude(r => r.Instructor)
                    .ThenInclude(i => i!.Usuario)
                        .ThenInclude(u => u.Persona)
            .Where(fc => fc.Competencia.Nombre == nombreCompetencia)
            .ToListAsync();

        var codigoCompetencia = await context.Competencia
            .FirstOrDefaultAsync(c => c.Nombre == nombreCompetencia);

        return new FichaCompetenciaDTO
        {
            NombreCompetencia = nombreCompetencia,
            CodigoCompetencia = codigoCompetencia?.Codigo ?? string.Empty,
            TotalCompetencias = competencias.Count,
            CompetenciasActivas = competencias.Count(fc =>
                fc.Estado == EstadosCompetencia.EnCurso ||
                fc.Estado == EstadosCompetencia.Programada),
            CompetenciasClausuradas = competencias.Count(fc => fc.Estado == EstadosCompetencia.Vista),
            Fichas = competencias.Select(fc => new FichaCompetenciaItemDTO
            {
                NumeroFicha       = fc.Ficha.NumeroFicha,
                Nombre            = fc.Ficha.Nombre,
                Estado            = fc.Ficha.Estado,
                EstadoCompetencia = fc.Estado,          // estado de la competencia EN esta ficha
                Descripcion       = fc.Ficha.Descripcion,
                NombreInstructor  = fc.ResultadosProgramados
                    .Where(r => r.Instructor != null)
                    .Select(r => $"{r.Instructor!.Usuario.Persona.Nombres} {r.Instructor.Usuario.Persona.Apellidos}")
                    .FirstOrDefault() ?? "Sin instructor"
            }).ToList(),
            // Instructores únicos, con la lista de fichas en las que participan en esta competencia.
            // Se agrupa por instructor para evitar duplicados y recopilar todas sus fichas.
            Instructores = competencias
                .SelectMany(fc => fc.ResultadosProgramados
                    .Where(r => r.Instructor != null)
                    .Select(r => new
                    {
                        NumeroFicha     = fc.Ficha.NumeroFicha,
                        TipoCompetencia = fc.Competencia.Tipo,
                        r.Instructor
                    }))
                .GroupBy(x => x.Instructor!.IdInstructor)
                .Select(g =>
                {
                    var first = g.First();
                    return new InstructorListadoDTO
                    {
                        IdInstructor = g.Key,
                        Nombres      = $"{first.Instructor!.Usuario.Persona.Nombres} {first.Instructor.Usuario.Persona.Apellidos}",
                        Activo       = first.Instructor!.Activo,
                        Competencias = new List<string>
                        {
                            first.TipoCompetencia == "Tecnica"
                                ? "Instructor Técnico"
                                : "Instructor Transversal"
                        },
                        Fichas = g.Select(x => x.NumeroFicha).Distinct().ToList()
                    };
                })
                .ToList()
        };
    }
}
