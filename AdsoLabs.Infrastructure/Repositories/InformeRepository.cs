using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Informe;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using AdsoLabs.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class InformeRepository(AdsoDbContext context) : IInformeRepository
{
    public async Task<AcudienteDTO?> ObtenerAcudienteAsync(int idAprendiz)
    {
        var a = await context.Acudiente
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdAprendiz == idAprendiz);

        if (a is null) return null;

        return new AcudienteDTO
        {
            IdAcudiente     = a.IdAcudiente,
            Nombre          = a.Nombre,
            Parentesco      = a.Parentesco,
            Correo          = a.Correo,
            Telefono        = a.Telefono,
            Genero          = a.Genero,
            TipoDocumento   = a.TipoDocumento,
            NumeroDocumento = a.NumeroDocumento,
            Direccion       = a.Direccion,
            Estrato         = a.Estrato,
            FechaRegistro   = a.FechaRegistro
        };
    }

    public async Task GuardarAcudienteAsync(int idAprendiz, GuardarAcudienteDTO dto)
    {
        var existente = await context.Acudiente
            .FirstOrDefaultAsync(x => x.IdAprendiz == idAprendiz);

        if (existente is null)
        {
            context.Acudiente.Add(new Acudiente
            {
                IdAprendiz      = idAprendiz,
                Nombre          = dto.Nombre,
                Parentesco      = dto.Parentesco,
                Correo          = dto.Correo,
                Telefono        = dto.Telefono,
                Genero          = dto.Genero,
                TipoDocumento   = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                Direccion       = dto.Direccion,
                Estrato         = dto.Estrato
            });
        }
        else
        {
            existente.Nombre          = dto.Nombre;
            existente.Parentesco      = dto.Parentesco;
            existente.Correo          = dto.Correo;
            existente.Telefono        = dto.Telefono;
            existente.Genero          = dto.Genero;
            existente.TipoDocumento   = dto.TipoDocumento;
            existente.NumeroDocumento = dto.NumeroDocumento;
            existente.Direccion       = dto.Direccion;
            existente.Estrato         = dto.Estrato;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<ObservacionAprendizDTO>> ObtenerObservacionesAsync(int idAprendiz)
    {
        return await context.ObservacionAprendiz
            .AsNoTracking()
            .Where(x => x.IdAprendiz == idAprendiz)
            .OrderByDescending(x => x.Fecha)
            .Select(x => new ObservacionAprendizDTO
            {
                IdObservacion = x.IdObservacion,
                Descripcion   = x.Descripcion,
                Fecha         = x.Fecha,
                IdUsuario     = x.IdUsuario,
                NombreUsuario = x.Usuario.Persona.Nombres + " " + x.Usuario.Persona.Apellidos
            })
            .ToListAsync();
    }

    public async Task AgregarObservacionAsync(int idAprendiz, AgregarObservacionDTO dto)
    {
        context.ObservacionAprendiz.Add(new ObservacionAprendiz
        {
            IdAprendiz  = idAprendiz,
            Descripcion = dto.Descripcion,
            IdUsuario   = dto.IdUsuario,
            Fecha       = DateTime.Now
        });
        await context.SaveChangesAsync();
    }

    public async Task<List<CompetenciaSinCalificarDTO>> ObtenerCompetenciasSinCalificarAsync(
        int idAprendiz, string numeroFicha)
    {
        // ── Paso 1: resultados cuya programación ya existe (Programado o Completado)
        //            dentro de la ficha indicada, con nombre del resultado incluido.
        // ─────────────────────────────────────────────────────────────────────────
        var fcrProgramados = await context.FichaCompetenciaResultado
            .AsNoTracking()
            .Where(fcr => (fcr.Estado == "Programado" || fcr.Estado == "Completado")
                       && fcr.FichaCompetencia.Ficha.NumeroFicha == numeroFicha)
            .Select(fcr => new
            {
                fcr.IdResultado,
                NombreResultado   = fcr.Resultado.Nombre,
                IdCompetencia     = fcr.FichaCompetencia.IdCompetencia,
                NombreCompetencia = fcr.FichaCompetencia.Competencia.Nombre
            })
            .ToListAsync();

        if (!fcrProgramados.Any()) return [];

        var resultadoIds = fcrProgramados
            .Select(f => f.IdResultado)
            .Distinct()
            .ToList();

        // ── Paso 2: resultados donde al menos UN aprendiz ya tiene juicio APROBADO.
        //            Eso indica que la fase de evaluación produjo resultados y los
        //            demás aprendices sin juicio deben ser avisados.
        // ─────────────────────────────────────────────────────────────────────────
        var resultadosConJuicios = await context.JuicioResultado
            .AsNoTracking()
            .Where(jr => resultadoIds.Contains(jr.IdResultado)
                      && jr.Juicio == EstadosJuicio.Aprobado)
            .Select(jr => jr.IdResultado)
            .Distinct()
            .ToListAsync();

        if (!resultadosConJuicios.Any()) return [];

        // ── Paso 3: de esos resultados, ¿cuáles NO tiene aún registrado este aprendiz?
        // ─────────────────────────────────────────────────────────────────────────
        var resultadosDelAprendiz = await context.JuicioResultado
            .AsNoTracking()
            .Where(jr => jr.IdAprendiz == idAprendiz
                      && resultadosConJuicios.Contains(jr.IdResultado))
            .Select(jr => jr.IdResultado)
            .ToListAsync();

        var resultadosSinJuicio = resultadosConJuicios
            .Except(resultadosDelAprendiz)
            .ToList();

        if (!resultadosSinJuicio.Any()) return [];

        // ── Paso 4: agrupar por competencia para devolver un aviso por competencia.
        // ─────────────────────────────────────────────────────────────────────────
        return resultadosSinJuicio
            .GroupBy(idRes => fcrProgramados
                .First(f => f.IdResultado == idRes).IdCompetencia)
            .Select(g =>
            {
                var comp = fcrProgramados.First(f => f.IdCompetencia == g.Key);
                return new CompetenciaSinCalificarDTO
                {
                    IdCompetencia        = g.Key,
                    NombreCompetencia    = comp.NombreCompetencia,
                    ResultadosPendientes = g
                        .Select(idRes => fcrProgramados
                            .First(f => f.IdResultado == idRes).NombreResultado)
                        .Distinct()
                        .ToList()
                };
            })
            .ToList();
    }
}
