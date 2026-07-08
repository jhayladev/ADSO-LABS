using AdsoLabs.Application.DTOs.Reportes;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IReportesQueryService
{
    /// <summary>
    /// Reporte E — Resumen global de fichas.
    /// Instructor: solo sus fichas. Administrador: todas las fichas del sistema.
    /// </summary>
    Task<List<ReporteGlobalFichaDTO>> ObtenerResumenGlobalAsync(int idUsuario);

    /// <summary>
    /// Reporte B — Asistencia por ficha.
    /// Devuelve null si el usuario no tiene acceso a la ficha solicitada.
    /// </summary>
    Task<ReporteAsistenciaFichaDTO?> ObtenerAsistenciaFichaAsync(string numeroFicha, int idUsuario);

    /// <summary>
    /// Reporte C — Progreso de competencias por ficha.
    /// Devuelve null si el usuario no tiene acceso a la ficha solicitada.
    /// </summary>
    Task<ReporteProgresoCompetenciasDTO?> ObtenerProgresoCompetenciasAsync(string numeroFicha, int idUsuario);

    /// <summary>
    /// Reporte D — Juicios evaluativos por ficha.
    /// Devuelve null si el usuario no tiene acceso a la ficha solicitada.
    /// </summary>
    Task<ReporteJuiciosDTO?> ObtenerJuiciosAsync(string numeroFicha, int idUsuario);

    /// <summary>
    /// Reporte F — Carga académica de un instructor específico.
    /// Instructor: solo puede consultar sus propios datos (idInstructor ignorado).
    /// Administrador: puede consultar cualquier instructor por idInstructor.
    /// </summary>
    Task<ReporteInstructorDTO?> ObtenerReporteInstructorAsync(int idInstructor, int idUsuario);

    /// <summary>
    /// Lista de instructores para el selector (solo Administrador).
    /// </summary>
    Task<List<InstructorSelectorDTO>> ObtenerSelectorInstructoresAsync();

    /// <summary>
    /// Reporte G — Estado de una competencia en todas las fichas accesibles.
    /// </summary>
    Task<ReporteCompetenciaDTO?> ObtenerReporteCompetenciaAsync(int idCompetencia, int idUsuario);

    /// <summary>
    /// Reporte H — Aprendices patrocinados con datos de empresa, etapa y asistencia.
    /// Instructor: solo patrocinados de sus fichas. Administrador: todos.
    /// </summary>
    Task<List<ReportePatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario);
}
