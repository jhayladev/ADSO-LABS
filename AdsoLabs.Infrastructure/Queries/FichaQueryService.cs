using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Application.DTOs.Competencia;
using AdsoLabs.Application.DTOs.Fichas;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Queries
{
    public class FichaQueryService(AdsoDbContext context) : IFichaQueryService
    {
        public async Task<FichaDetallesDTO?> ObtenerDatosAsync(string numeroFicha)
        {

            var ficha = await context.Ficha
                .Include(a => a.FichaAprendices)
                .Include(a => a.FichasCompetencias)
                    .ThenInclude(fc => fc.Competencia)
                .FirstOrDefaultAsync(fc => fc.NumeroFicha == numeroFicha);

            if (ficha == null) return null;


            FichaDetallesDTO detallesFicha = new FichaDetallesDTO()
            {

                NumeroFicha = numeroFicha,
                FechaInicio = ficha.FechaInicio.ToDateTime(new TimeOnly(0, 0, 0)),
                FechaFin = ficha.FechaFin?.ToDateTime(new TimeOnly(0, 0, 0)) ?? DateTime.MaxValue,
                TotalAprendices = ficha.FichaAprendices.Count(fa => fa.Estado == EstadosAprendiz.EnFormacion),
                TotalCompetencias = ficha.FichasCompetencias.Count(),
                TotalCompetenciasVistas = ficha.FichasCompetencias.Count(c => c.Estado == EstadosCompetencia.Vista),
                // TotalHoras → suma de FichaCompetencia.TotalHoras (configurado manualmente).
                // Si aún no está configurado para alguna competencia, cae al catálogo (HorasAsignadas).
                TotalHoras       = ficha.FichasCompetencias.Sum(c => c.TotalHoras ?? c.Competencia.HorasAsignadas),
                // TotalHorasVistas → suma de HorasEjecutadas de todas las FichaCompetencias.
                // HorasEjecutadas acumula las horas de los FCRs Programados y Completados,
                // por lo que refleja el avance real independientemente del estado de la competencia.
                TotalHorasVistas = ficha.FichasCompetencias.Sum(c => c.HorasEjecutadas),
                FaseActual = FaseFormativaHelper.NombreFase(
                    FaseFormativaHelper.CalcularFaseActualFicha(
                        ficha.FichasCompetencias.Select(fc => (fc.Competencia.FaseFormativa, fc.Estado))))

            };

            return detallesFicha;
        }


        public async Task<List<CompetenciaFichaDTO>> ObtenerFichaCompetenciaAsync(string numeroFicha, int idUsuario = 0)
        {
            // Resolver idUsuario → idInstructor una sola vez antes de la proyección LINQ.
            // Si no se pasa idUsuario (admin o acceso genérico) queda 0 y EsMiCompetencia = false.
            // Se usa int (no int?) para que EF Core genere SQL limpio sin comparaciones nullable.
            int filtroInstructor = 0;
            if (idUsuario > 0)
            {
                filtroInstructor = await context.InstructorPerfil
                    .Where(ip => ip.IdUsuario == idUsuario)
                    .Select(ip => ip.IdInstructor)
                    .FirstOrDefaultAsync();
            }

            return await context.FichaCompetencia
                .Where(fc => fc.Ficha.NumeroFicha == numeroFicha)
                .Select(fc => new CompetenciaFichaDTO
                {
                    IdFichaCompetencia = fc.IdFichaCompetencia,
                    IdCompetencia      = fc.IdCompetencia,
                    Nombre             = fc.Competencia.Nombre,
                    Codigo             = fc.Competencia.Codigo,
                    Tipo               = fc.Competencia.Tipo,
                    FaseFormativa      = FaseFormativaHelper.NombreFase(fc.Competencia.FaseFormativa),
                    // Estado derivado de los datos reales para evitar inconsistencias de datos históricos:
                    // "Vista"     → solo SOFIA puede asignarlo (todos los FCR Completado), se respeta tal cual.
                    // "Programada"→ al menos un FCR Programado (tiene prioridad aunque haya Completados).
                    // "En Curso"  → al menos un FCR Completado y ningún FCR Programado activo.
                    // "Pendiente" → sin FCRs Programado ni Completado.
                    Estado = fc.Estado == "Vista"
                        ? "Vista"
                        : fc.ResultadosProgramados.Any(r => r.Estado == "Programado")
                            ? "Programada"
                            : fc.ResultadosProgramados.Any(r => r.Estado == "Completado")
                                ? "En Curso"
                                : "Pendiente",
                    // FechaInicio y FechaFin se toman del campo configurado manualmente en FichaCompetencia.
                    FechaInicio = fc.FechaInicio,
                    FechaFin    = fc.FechaFin,
                    // Lista de resultados con Estado = "Programado" y sus horarios, para el Kanban.
                    ResultadosProgramados = fc.ResultadosProgramados
                        .Where(r => r.Estado == "Programado")
                        .Select(r => new ResultadoKanbanDTO(r.Resultado.Nombre, r.HoraInicio, r.HoraFin))
                        .ToList(),
                    HorasEjecutadas = fc.HorasEjecutadas,
                    HorasAsignadas  = fc.Competencia.HorasAsignadas,
                    TotalHoras      = fc.TotalHoras,
                    // true cuando el instructor tiene al menos un resultado Programado o Completado en esta competencia.
                    EsMiCompetencia = filtroInstructor > 0 &&
                                      fc.ResultadosProgramados.Any(r => r.IdInstructor == filtroInstructor
                                                                      && (r.Estado == "Programado" || r.Estado == "Completado"))
                })
                .OrderBy(fc => fc.Nombre)
                .ToListAsync();
        }

        public async Task<List<AprendizFichaDTO>> ObtenerAprendicesAsync(string numeroFicha)
        {
            var idFicha = await context.Ficha
                .Where(f => f.NumeroFicha == numeroFicha)
                .Select(f => f.IdFicha)
                .FirstAsync();

            // IDs de todos los resultados de aprendizaje asignados a esta ficha
            var resultadosIds = await context.FichaCompetenciaResultado
                .Where(fcr => fcr.FichaCompetencia.IdFicha == idFicha)
                .Select(fcr => fcr.IdResultado)
                .ToListAsync();

            int totalResultados = resultadosIds.Count;

            // Aprendices aprobados por resultado (un solo viaje a la BD)
            var aprobadosPorAprendiz = await context.JuicioResultado
                .Where(jr => resultadosIds.Contains(jr.IdResultado)
                          && jr.Juicio == EstadosJuicio.Aprobado)
                .GroupBy(jr => jr.IdAprendiz)
                .Select(g => new { IdAprendiz = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.IdAprendiz, x => x.Total);

            // Aprendices de la ficha con su patrocinio
            var aprendices = await context.FichaAprendiz
                .Where(fa => fa.IdFicha == idFicha && fa.Estado == EstadosAprendiz.EnFormacion)
                .Select(fa => new
                {
                    fa.IdAprendiz,
                    Nombres         = fa.Aprendiz.Persona.Nombres + " " + fa.Aprendiz.Persona.Apellidos,
                    fa.Aprendiz.Persona.NumeroDocumento,
                    fa.Estado,
                    TienePatrocinio = context.Patrocinio
                        .Any(p => p.IdAprendiz == fa.IdAprendiz && p.IdFicha == idFicha && p.Activo)
                })
                .OrderBy(a => a.Nombres)
                .ToListAsync();

            return aprendices.Select(a => new AprendizFichaDTO
            {
                IdAprendiz      = a.IdAprendiz,
                Nombres         = a.Nombres,
                NumeroDocumento = a.NumeroDocumento,
                EstadoFicha     = a.Estado,
                AlDia           = totalResultados > 0 ||
                                  aprobadosPorAprendiz.GetValueOrDefault(a.IdAprendiz) == totalResultados,
                TienePatrocinio = a.TienePatrocinio
            }).ToList();
        }
    }
}
