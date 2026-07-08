using AdsoLabs.Application.DTOs.Patrocinio;

namespace AdsoLabs.Application.Interfaces.Queries;

public interface IPatrocinioQueryService
{
    Task<List<PatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario);
    Task<PatrocinioDTO?> ObtenerPatrocinioAsync(int idPatrocinio);
    Task<List<AsistenciaPatrocinioDTO>> ObtenerAsistenciasAsync(int idPatrocinio);
    Task<ResumenAsistenciaDTO>          ObtenerResumenAsistenciaAsync(int idAprendiz, int idFicha);
}
