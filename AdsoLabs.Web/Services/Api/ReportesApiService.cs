using AdsoLabs.Application.DTOs.Reportes;
using System.Net.Http.Json;

namespace AdsoLabs.Web.Services.Api;

public class ReportesApiService(HttpClient httpClient)
{
    /// <summary>
    /// Valida el status HTTP. En caso de error incluye el body de la respuesta
    /// para facilitar el diagnostico del error del servidor.
    /// </summary>
    private static async Task ValidarRespuestaAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        var resumen = body.Length > 600 ? body[..600] + "..." : body;
        throw new HttpRequestException(
            $"Error {(int)response.StatusCode} ({response.ReasonPhrase}) en {response.RequestMessage?.RequestUri?.PathAndQuery}. " +
            $"Detalle del servidor: {resumen}");
    }


    // idUsuario ya no se manda a la API: se deriva del token interno (ver
    // InternalTokenHandler). Los parametros se mantienen para no tocar todos
    // los llamadores existentes.

    // ── E: Resumen global ─────────────────────────────────────────────────────

    public async Task<List<ReporteGlobalFichaDTO>> ObtenerResumenGlobalAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/reportes/global");
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<List<ReporteGlobalFichaDTO>>() ?? [];
    }

    // ── B: Asistencia por ficha ───────────────────────────────────────────────

    public async Task<ReporteAsistenciaFichaDTO?> ObtenerAsistenciaFichaAsync(string numeroFicha, int idUsuario)
    {
        var response = await httpClient.GetAsync(
            $"api/reportes/asistencia/{Uri.EscapeDataString(numeroFicha)}");
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) return null;
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<ReporteAsistenciaFichaDTO>();
    }

    // ── C: Progreso de competencias por ficha ─────────────────────────────────

    public async Task<ReporteProgresoCompetenciasDTO?> ObtenerProgresoCompetenciasAsync(string numeroFicha, int idUsuario)
    {
        var response = await httpClient.GetAsync(
            $"api/reportes/competencias/{Uri.EscapeDataString(numeroFicha)}");
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) return null;
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<ReporteProgresoCompetenciasDTO>();
    }

    // ── D: Juicios evaluativos por ficha ──────────────────────────────────────

    public async Task<ReporteJuiciosDTO?> ObtenerJuiciosAsync(string numeroFicha, int idUsuario)
    {
        var response = await httpClient.GetAsync(
            $"api/reportes/juicios/{Uri.EscapeDataString(numeroFicha)}");
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) return null;
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<ReporteJuiciosDTO>();
    }

    // ── F: Por instructor ─────────────────────────────────────────────────────

    public async Task<ReporteInstructorDTO?> ObtenerReporteInstructorAsync(int idInstructor, int idUsuario)
    {
        var response = await httpClient.GetAsync($"api/reportes/instructor/{idInstructor}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<ReporteInstructorDTO>();
    }

    public async Task<List<InstructorSelectorDTO>> ObtenerSelectorInstructoresAsync()
    {
        var response = await httpClient.GetAsync("api/reportes/instructor/selector");
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<List<InstructorSelectorDTO>>() ?? [];
    }

    // ── G: Por competencia ────────────────────────────────────────────────────

    public async Task<ReporteCompetenciaDTO?> ObtenerReporteCompetenciaAsync(int idCompetencia, int idUsuario)
    {
        var response = await httpClient.GetAsync($"api/reportes/competencia/{idCompetencia}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<ReporteCompetenciaDTO>();
    }

    // ── H: Patrocinio ─────────────────────────────────────────────────────────

    public async Task<List<ReportePatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/reportes/patrocinio");
        await ValidarRespuestaAsync(response);
        return await response.Content.ReadFromJsonAsync<List<ReportePatrocinioDTO>>() ?? [];
    }

    // ── Exportacion Excel ─────────────────────────────────────────────────────
    // Antes se exponian como URL directa (<a href> a la API en otro puerto).
    // Con la API exigiendo el token interno via Authorization: Bearer, el
    // navegador ya no puede pegarle directo — hay que bajar los bytes por el
    // HttpClient autenticado de Web y disparar la descarga con JS interop
    // (mismo patron que RepositorioApiService.DescargarAsync).

    public async Task<(byte[]? bytes, string nombre)> DescargarExcelAsistenciaAsync(string numeroFicha)
    {
        var response = await httpClient.GetAsync($"api/reportes/asistencia/{Uri.EscapeDataString(numeroFicha)}/excel");
        if (!response.IsSuccessStatusCode) return (null, "");
        return (await response.Content.ReadAsByteArrayAsync(), $"Asistencia_{numeroFicha}.xlsx");
    }

    public async Task<(byte[]? bytes, string nombre)> DescargarExcelCompetenciasAsync(string numeroFicha)
    {
        var response = await httpClient.GetAsync($"api/reportes/competencias/{Uri.EscapeDataString(numeroFicha)}/excel");
        if (!response.IsSuccessStatusCode) return (null, "");
        return (await response.Content.ReadAsByteArrayAsync(), $"Competencias_{numeroFicha}.xlsx");
    }

    public async Task<(byte[]? bytes, string nombre)> DescargarExcelGlobalAsync()
    {
        var response = await httpClient.GetAsync("api/reportes/global/excel");
        if (!response.IsSuccessStatusCode) return (null, "");
        return (await response.Content.ReadAsByteArrayAsync(), "ResumenFichas.xlsx");
    }
}
