using AdsoLabs.Application.DTOs.Dashboard;

namespace AdsoLabs.Web.Services.Api;

// idUsuario ya no se manda a la API: se deriva del token interno (ver
// InternalTokenHandler). Los parametros se mantienen para no tocar todos
// los llamadores existentes.
public class DashboardApiService(HttpClient httpClient)
{
    /// <summary>
    /// Resumen de cabecera del dashboard del usuario autenticado.
    /// Devuelve null si la API responde con un error.
    /// </summary>
    public async Task<ResumenDashboardDTO?> ObtenerResumenAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/mi-dashboard");
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"api/mi-dashboard devolvió HTTP {(int)response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<ResumenDashboardDTO>();
    }

    /// <summary>
    /// Conteo de competencias por estado para las fichas del usuario autenticado.
    /// </summary>
    public async Task<EstadoCompetenciasDashboardDTO?> ObtenerEstadoCompetenciasAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/mi-dashboard/competencias");
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"api/mi-dashboard/competencias devolvió HTTP {(int)response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<EstadoCompetenciasDashboardDTO>();
    }

    /// <summary>
    /// Progreso de horas planeadas vs ejecutadas por ficha activa del usuario autenticado.
    /// </summary>
    public async Task<List<ProgresoFichaDashboardDTO>> ObtenerProgresoFichasAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/mi-dashboard/progreso-fichas");
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"api/mi-dashboard/progreso-fichas devolvió HTTP {(int)response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<List<ProgresoFichaDashboardDTO>>() ?? [];
    }

    /// <summary>
    /// Alertas clasificadas del usuario autenticado.
    /// </summary>
    public async Task<AlertasDashboardDTO?> ObtenerAlertasAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/mi-dashboard/alertas");
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"api/mi-dashboard/alertas devolvió HTTP {(int)response.StatusCode}");
        return await response.Content.ReadFromJsonAsync<AlertasDashboardDTO>();
    }
}
