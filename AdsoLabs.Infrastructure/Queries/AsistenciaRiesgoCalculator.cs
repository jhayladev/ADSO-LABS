using AdsoLabs.Application.Common.Constants;
using AdsoLabs.Infrastructure.Models;

namespace AdsoLabs.Infrastructure.Queries;

/// <summary>
/// Regla de negocio "En Riesgo" (AGENTS.md §7.6): un aprendiz esta en riesgo si,
/// para alguna de sus competencias, tiene 3+ inasistencias consecutivas o sus
/// horas ausentes superan el 20% de las horas totales de esa competencia.
/// Requiere que las Asistencia vengan con Sesion.FichaCompetencia.Competencia
/// ya cargados (Include/ThenInclude) - no dispara queries propias.
/// </summary>
public static class AsistenciaRiesgoCalculator
{
    private const int UmbralConsecutivas = 3;
    private const decimal UmbralPorcentajeHoras = 0.20m;

    public static bool EsAprendizEnRiesgo(IEnumerable<Asistencia> asistencias)
    {
        var porCompetencia = asistencias.GroupBy(a => a.Sesion.IdFichaCompetencia);

        foreach (var grupo in porCompetencia)
        {
            var ordenadas = grupo
                .OrderBy(a => a.Sesion.Fecha)
                .ThenBy(a => a.Sesion.HoraInicio)
                .ToList();

            int consecutivas = 0;
            foreach (var a in ordenadas)
            {
                consecutivas = a.Estado == EstadosAsistencia.Ausente ? consecutivas + 1 : 0;
                if (consecutivas >= UmbralConsecutivas) return true;
            }

            var fc = grupo.First().Sesion.FichaCompetencia;
            int totalHoras = fc.TotalHoras ?? fc.Competencia.HorasAsignadas;
            if (totalHoras <= 0) continue;

            double horasAusentes = ordenadas
                .Where(a => a.Estado == EstadosAsistencia.Ausente)
                .Sum(a => (a.Sesion.HoraFin - a.Sesion.HoraInicio).TotalHours);

            if ((decimal)horasAusentes / totalHoras >= UmbralPorcentajeHoras) return true;
        }

        return false;
    }
}
