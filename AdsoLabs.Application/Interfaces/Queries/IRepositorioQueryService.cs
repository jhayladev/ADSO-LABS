using AdsoLabs.Application.DTOs.Repositorio;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IRepositorioQueryService
{
    Task<List<DocumentoListadoDTO>> ObtenerGuiasAsync(string? filtro = null, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerInstrumentosAsync(string? filtro = null, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerPlaneacionesAsync(string? filtro = null, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerProyectosAsync(string? filtro = null, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerDesarrollosAsync(string? filtro = null, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerPlanesConcertadosAsync(string? filtro = null, CancellationToken ct = default);

    Task<List<DescargaDTO>> ObtenerMisDescargasAsync(int idUsuario, CancellationToken ct = default);
    Task<ArchivoInfoDTO?> ObtenerArchivoAsync(int idArchivo, CancellationToken ct = default);

    Task<List<PlanParaSubirDTO>> ObtenerPlanesSinDocumentoAsync(CancellationToken ct = default);
    Task<List<FichaCompetenciaSelectDTO>> ObtenerFichasCompetenciasAsync(CancellationToken ct = default);
    Task<List<CompetenciaSelectDTO>> ObtenerCompetenciasAsync(CancellationToken ct = default);
    Task<List<FichaSelectDTO>> ObtenerFichasAsync(CancellationToken ct = default);

    /// <summary>Retorna el IdInstructor del InstructorPerfil para un IdUsuario dado.</summary>
    Task<int?> ObtenerIdInstructorPorUsuarioAsync(int idUsuario, CancellationToken ct = default);

    // ── Consultas filtradas por ficha (para sección Repositorio en detalle de ficha) ──
    Task<List<DocumentoListadoDTO>> ObtenerGuiasPorFichaAsync(string numeroFicha, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerPlaneacionesPorFichaAsync(string numeroFicha, CancellationToken ct = default);
    Task<List<DocumentoListadoDTO>> ObtenerProyectosPorFichaAsync(string numeroFicha, CancellationToken ct = default);
}
