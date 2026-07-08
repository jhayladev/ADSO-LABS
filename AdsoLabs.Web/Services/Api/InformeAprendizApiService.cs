using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Application.DTOs.Informe;
using System.Net.Http.Json;

namespace AdsoLabs.Web.Services.Api;

public class InformeAprendizApiService(HttpClient httpClient)
{
    // ──────────────────────────────────────────────
    //  Datos del aprendiz
    // ──────────────────────────────────────────────

    public async Task<AprendizDetalleDTO?> ObtenerDatosAprendizAsync(int idAprendiz)
    {
        var response = await httpClient.GetAsync($"api/aprendiz/{idAprendiz}/datos");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AprendizDetalleDTO>();
    }

    // ──────────────────────────────────────────────
    //  Acudiente
    // ──────────────────────────────────────────────

    public async Task<AcudienteDTO?> ObtenerAcudienteAsync(int idAprendiz)
    {
        var response = await httpClient.GetAsync($"api/aprendiz/{idAprendiz}/acudiente");
        if (!response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        return await response.Content.ReadFromJsonAsync<AcudienteDTO>();
    }

    public async Task<bool> GuardarAcudienteAsync(int idAprendiz, GuardarAcudienteDTO dto)
    {
        var response = await httpClient.PostAsJsonAsync($"api/aprendiz/{idAprendiz}/acudiente", dto);
        return response.IsSuccessStatusCode;
    }

    // ──────────────────────────────────────────────
    //  Observaciones
    // ──────────────────────────────────────────────

    public async Task<List<ObservacionAprendizDTO>> ObtenerObservacionesAsync(int idAprendiz)
    {
        var response = await httpClient.GetAsync($"api/aprendiz/{idAprendiz}/observaciones");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<ObservacionAprendizDTO>>() ?? [];
    }

    public async Task<bool> AgregarObservacionAsync(int idAprendiz, AgregarObservacionDTO dto)
    {
        var response = await httpClient.PostAsJsonAsync($"api/aprendiz/{idAprendiz}/observacion", dto);
        return response.IsSuccessStatusCode;
    }

    // ──────────────────────────────────────────────
    //  Competencias sin calificar
    // ──────────────────────────────────────────────

    public async Task<List<CompetenciaSinCalificarDTO>> ObtenerCompetenciasSinCalificarAsync(
        int idAprendiz, string numeroFicha)
    {
        var response = await httpClient.GetAsync(
            $"api/aprendiz/{idAprendiz}/competencias-sin-calificar?numeroFicha={Uri.EscapeDataString(numeroFicha)}");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<CompetenciaSinCalificarDTO>>() ?? [];
    }
}
