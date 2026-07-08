using AdsoLabs.Application.DTOs.Fichas;

namespace AdsoLabs.Web.Services.Api;

public class FichaApiService(HttpClient httpClient)
{
    // idUsuario ya no se manda: la API deriva el usuario del token interno
    // (ver InternalTokenHandler). El parametro se mantiene para no tocar
    // todos los llamadores existentes.
    public async Task<List<FichaDTO>> ObtenerFichasAsync(int idUsuario = 0)
    {
        var response = await httpClient.GetAsync("api/obtener-fichas");
        return await response.Content.ReadFromJsonAsync<List<FichaDTO>>() ?? [];
    }

    public async Task<FichaDetallesDTO?> ObtenerDatosAsync(string numeroFicha)
    {
        var response = await httpClient.GetAsync($"api/obtener-detalles-ficha?numeroFicha={numeroFicha}");
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content)) return null;
        return await response.Content.ReadFromJsonAsync<FichaDetallesDTO>();
    }

    public async Task<List<CompetenciaFichaDTO>> ObtenerFichaCompetenciaAsync(string numeroFicha, int idUsuario = 0)
    {
        var url = idUsuario > 0
            ? $"api/obtener-competencias-ficha?numeroFicha={numeroFicha}&idUsuario={idUsuario}"
            : $"api/obtener-competencias-ficha?numeroFicha={numeroFicha}";
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<List<CompetenciaFichaDTO>>() ?? [];
    }

    public async Task<List<AprendizFichaDTO>> ObtenerAprendicesAsync(string numeroFicha)
    {
        var response = await httpClient.GetAsync($"api/obtener-aprendices-ficha?numeroFicha={numeroFicha}");
        return await response.Content.ReadFromJsonAsync<List<AprendizFichaDTO>>() ?? [];
    }
}
