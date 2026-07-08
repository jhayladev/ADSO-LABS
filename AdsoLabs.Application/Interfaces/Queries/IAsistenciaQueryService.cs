using AdsoLabs.Application.DTOs.Asistencia;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IAsistenciaQueryService
{
    /// <summary>
    /// Todos los resultados oficialmente programados (con fechas + horario + instructor)
    /// que pertenecen a la ficha indicada.
    /// </summary>
    Task<List<ResultadoProgramadoDTO>> ObtenerResultadosProgramadosAsync(string numeroFicha);

    /// <summary>
    /// Datos de un resultado programado específico (para cabecera de la pantalla de sesiones).
    /// </summary>
    Task<ResultadoProgramadoDTO?> ObtenerResultadoProgramadoAsync(int idFichaCompetenciaResultado);

    /// <summary>
    /// Historial de sesiones reales creadas para un resultado programado específico.
    /// Incluye cuántos aprendices ya tienen asistencia registrada.
    /// </summary>
    Task<List<SesionDTO>> ObtenerSesionesPorResultadoAsync(int idFichaCompetenciaResultado);

    /// <summary>
    /// Datos básicos de una sesión (fecha, horario, estado) para la cabecera
    /// de la pantalla de registro de asistencia.
    /// </summary>
    Task<SesionDTO?> ObtenerInfoSesionAsync(int idSesion);

    /// <summary>
    /// Aprendices EN FORMACION de la ficha con su estado de asistencia para la sesión indicada.
    /// Estado = null si aún no tiene registro.
    /// </summary>
    Task<List<AprendizAsistenciaDTO>> ObtenerAprendicesSesionAsync(int idSesion);
}
