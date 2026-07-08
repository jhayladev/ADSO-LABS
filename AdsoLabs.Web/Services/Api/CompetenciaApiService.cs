using AdsoLabs.Application.DTOs.Competencia;

namespace AdsoLabs.Web.Services.Api;

public class CompetenciaApiService(HttpClient httpClient)
{
    public async Task<List<CompetenciaDTO>> ObtenerCompetenciasAsync()
    {
        var response = await httpClient.GetAsync("api/obtener-competencias");
        return await response.Content.ReadFromJsonAsync<List<CompetenciaDTO>>() ?? [];
    }

    public async Task<FichaCompetenciaDTO> ObtenerDatosCompetenciaAsync(string nombreCompetencia)
    {
        var response = await httpClient.GetAsync($"api/obtener-datos-competencia?nombreCompetencia={nombreCompetencia}");
        return await response.Content.ReadFromJsonAsync<FichaCompetenciaDTO>() ?? new();
    }
}
