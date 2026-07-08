using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Asistencia;

namespace AdsoLabs.Web.Services.Api;

public class AsistenciaApiService(HttpClient httpClient)
{
    public async Task<List<ResultadoProgramadoDTO>> ObtenerResultadosProgramadosAsync(string numeroFicha)
    {
        var response = await httpClient.GetAsync(
            $"api/obtener-resultados-programados-ficha?numeroFicha={numeroFicha}");
        return await response.Content.ReadFromJsonAsync<List<ResultadoProgramadoDTO>>() ?? [];
    }

    public async Task<ResultadoProgramadoDTO?> ObtenerResultadoProgramadoAsync(int idFichaCompetenciaResultado)
    {
        var response = await httpClient.GetAsync(
            $"api/obtener-resultado-programado?idFichaCompetenciaResultado={idFichaCompetenciaResultado}");
        return await response.Content.ReadFromJsonAsync<ResultadoProgramadoDTO>();
    }

    public async Task<List<SesionDTO>> ObtenerSesionesPorResultadoAsync(int idFichaCompetenciaResultado)
    {
        var response = await httpClient.GetAsync(
            $"api/obtener-sesiones-resultado?idFichaCompetenciaResultado={idFichaCompetenciaResultado}");
        return await response.Content.ReadFromJsonAsync<List<SesionDTO>>() ?? [];
    }

    public async Task<SesionDTO?> ObtenerInfoSesionAsync(int idSesion)
    {
        var response = await httpClient.GetAsync(
            $"api/obtener-info-sesion?idSesion={idSesion}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SesionDTO>();
    }

    public async Task<List<AprendizAsistenciaDTO>> ObtenerAprendicesSesionAsync(int idSesion)
    {
        var response = await httpClient.GetAsync(
            $"api/obtener-aprendices-sesion?idSesion={idSesion}");
        return await response.Content.ReadFromJsonAsync<List<AprendizAsistenciaDTO>>() ?? [];
    }

    public async Task<MensajeEstadoDTO> CrearSesionAsync(CrearSesionDTO dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/crear-sesion", dto);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }

    public async Task<MensajeEstadoDTO> GuardarAsistenciaAsync(GuardarAsistenciaDTO dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/guardar-asistencia", dto);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }
}
