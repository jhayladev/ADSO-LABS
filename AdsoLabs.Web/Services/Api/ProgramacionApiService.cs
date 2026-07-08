using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Fichas;

namespace AdsoLabs.Web.Services.Api;

public class ProgramacionApiService(HttpClient httpClient)
{
    public async Task<List<ResultadoFichaDTO>> ObtenerResultadosAsync(
        string numeroFicha, int idCompetencia)
    {
        var response = await httpClient.GetAsync(
            $"api/obtener-resultados-ficha-competencia?numeroFicha={numeroFicha}&idCompetencia={idCompetencia}");
        return await response.Content.ReadFromJsonAsync<List<ResultadoFichaDTO>>() ?? [];
    }

    public async Task<List<InstructorSelectDTO>> ObtenerInstructoresActivosAsync()
    {
        var response = await httpClient.GetAsync("api/obtener-instructores-activos");
        return await response.Content.ReadFromJsonAsync<List<InstructorSelectDTO>>() ?? [];
    }

    public async Task<MensajeEstadoDTO> ConfigurarFichaCompetenciaAsync(ConfigurarFichaCompetenciaDTO dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/configurar-competencia-ficha", dto);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }

    public async Task<MensajeEstadoDTO> ProgramarResultadoAsync(ProgramarResultadoDTO dto)
    {
        var response = await httpClient.PostAsJsonAsync("api/programar-resultado", dto);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }

    public async Task<MensajeEstadoDTO> EliminarProgramacionResultadoAsync(int idFichaCompetenciaResultado)
    {
        var response = await httpClient.DeleteAsync(
            $"api/eliminar-programacion-resultado/{idFichaCompetenciaResultado}");
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }
}
