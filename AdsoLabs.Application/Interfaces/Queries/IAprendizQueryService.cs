using AdsoLabs.Application.DTOs.Aprendiz;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IAprendizQueryService
{
    Task<List<AprendizListadoDTO>> ObtenerTodosAsync();
    Task<List<AprendizListadoDTO>> ObtenerPorFichaAsync(string numeroFicha);
    Task<AprendizDetalleDTO?> ObtenerDetallesAsync(int idAprendiz);
    Task<List<AprendizListadoDTO>> FiltrarAsync(string? numeroFicha, string? numeroDocumento, string? nombreAprendiz);
    Task<ResumenAprendizDto> ObtenerResumenAsync(string? numeroFicha, string? numeroDocumento, string? nombreAprendiz);
}
