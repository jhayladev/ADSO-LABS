using System.Net.Http.Json;
using System.Text.Json;
using AdsoLabs.Application.DTOs.PrediccionIA;

namespace AdsoLabs.Web.Services.Api;

public class PrediccionIAApiService(HttpClient httpClient)
{
    private static async Task<string> LeerMensajeErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("mensaje", out var m))
                return m.GetString() ?? body;
            return body;
        }
        catch
        {
            return $"HTTP {(int)response.StatusCode}";
        }
    }

    // idUsuario ya no se manda: se deriva del token interno (ver InternalTokenHandler).
    public async Task<CompatibilidadResponseDTO> ObtenerCompatibilidadAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/PrediccionIA/compatibilidad");
        if (!response.IsSuccessStatusCode)
            throw new Exception(await LeerMensajeErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<CompatibilidadResponseDTO>())!;
    }

    public async Task<List<CompetenciaAsignadaDTO>> ObtenerCompetenciasAsignadasAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/PrediccionIA/competencias-asignadas");
        if (!response.IsSuccessStatusCode)
            throw new Exception(await LeerMensajeErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<List<CompetenciaAsignadaDTO>>()) ?? [];
    }

    public async Task<PrediccionAprendizajeDTO> ObtenerPrediccionAprendizajeAsync(int idUsuario, int idFichaCompetencia, bool forzarActualizacion = false)
    {
        var response = await httpClient.GetAsync(
            $"api/PrediccionIA/prediccion-aprendizaje?idFichaCompetencia={idFichaCompetencia}&forzarActualizacion={forzarActualizacion}");
        if (!response.IsSuccessStatusCode)
            throw new Exception(await LeerMensajeErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<PrediccionAprendizajeDTO>())!;
    }

    public async Task EnviarSolicitudAsync(int idInstructor, int idFichaCompetencia, string razonamiento, int porcentaje)
    {
        var cmd = new EnviarSolicitudCommand(idInstructor, idFichaCompetencia, razonamiento, porcentaje);
        var response = await httpClient.PostAsJsonAsync("api/PrediccionIA/solicitud", cmd);
        if (!response.IsSuccessStatusCode)
            throw new Exception(await LeerMensajeErrorAsync(response));
    }

    public async Task<List<SolicitudAsignacionDTO>> ObtenerSolicitudesAsync()
    {
        var response = await httpClient.GetAsync("api/PrediccionIA/solicitudes");
        if (!response.IsSuccessStatusCode)
            throw new Exception(await LeerMensajeErrorAsync(response));
        return (await response.Content.ReadFromJsonAsync<List<SolicitudAsignacionDTO>>()) ?? [];
    }

    public async Task ResponderSolicitudAsync(int idSolicitud, string estado, string? observacion)
    {
        var cmd = new ResponderSolicitudCommand(idSolicitud, estado, observacion);
        var response = await httpClient.PutAsJsonAsync($"api/PrediccionIA/solicitud/{idSolicitud}/responder", cmd);
        if (!response.IsSuccessStatusCode)
            throw new Exception(await LeerMensajeErrorAsync(response));
    }
}
