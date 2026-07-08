using AdsoLabs.Application.DTOs.Monitoria;

namespace AdsoLabs.Web.Services.Api;

public class MonitoriaApiService(HttpClient httpClient)
{
    public async Task<EstadoMonitorDTO?> ObtenerEstadoMonitorAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync($"api/obtener-estado-monitor?idUsuario={idUsuario}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<EstadoMonitorDTO>();
    }

    public async Task<List<MonitorDTO>> ObtenerMonitoresAsync()
    {
        var response = await httpClient.GetAsync("api/obtener-monitores");
        return await response.Content.ReadFromJsonAsync<List<MonitorDTO>>() ?? [];
    }

    public async Task<List<SesionMonitoriaDTO>> ObtenerMonitoriasVigentesAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync($"api/obtener-monitorias-vigentes?idUsuario={idUsuario}");
        return await response.Content.ReadFromJsonAsync<List<SesionMonitoriaDTO>>() ?? [];
    }

    public async Task<List<SesionMonitoriaDTO>> ObtenerMisSesionesAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync($"api/obtener-mis-sesiones-monitoria?idUsuario={idUsuario}");
        return await response.Content.ReadFromJsonAsync<List<SesionMonitoriaDTO>>() ?? [];
    }

    public async Task<List<InscripcionMonitoriaDTO>> ObtenerInscritosAsync(int idSesionMonitoria)
    {
        var response = await httpClient.GetAsync($"api/obtener-inscritos-monitoria/{idSesionMonitoria}");
        return await response.Content.ReadFromJsonAsync<List<InscripcionMonitoriaDTO>>() ?? [];
    }

    public async Task<List<AsistenciaMonitoriaDTO>> ObtenerAsistenciasRecibidasAsync(int? idInstructor)
    {
        var query = idInstructor.HasValue ? $"?idInstructor={idInstructor}" : "";
        var response = await httpClient.GetAsync($"api/obtener-asistencias-recibidas-monitoria{query}");
        return await response.Content.ReadFromJsonAsync<List<AsistenciaMonitoriaDTO>>() ?? [];
    }

    public async Task<List<InformeMonitoriaDTO>> ObtenerInformesRecibidosAsync(int? idInstructor)
    {
        var query = idInstructor.HasValue ? $"?idInstructor={idInstructor}" : "";
        var response = await httpClient.GetAsync($"api/obtener-informes-recibidos-monitoria{query}");
        return await response.Content.ReadFromJsonAsync<List<InformeMonitoriaDTO>>() ?? [];
    }

    public async Task<(bool ok, string mensaje)> AsignarMonitorAsync(AsignarMonitorCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/asignar-monitor", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al asignar el monitor.");
    }

    public async Task<(bool ok, string mensaje)> DesactivarMonitorAsync(int idAprendiz)
    {
        var response = await httpClient.PutAsync($"api/desactivar-monitor/{idAprendiz}", null);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al desactivar el monitor.");
    }

    public async Task<(bool ok, string mensaje)> CrearSesionAsync(CrearSesionMonitoriaCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/crear-sesion-monitoria", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al crear la sesión de monitoría.");
    }

    public async Task<(bool ok, string mensaje)> CancelarSesionAsync(int idSesionMonitoria, int idUsuario)
    {
        var response = await httpClient.PutAsync(
            $"api/cancelar-sesion-monitoria/{idSesionMonitoria}?idUsuario={idUsuario}", null);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al cancelar la sesión.");
    }

    public async Task<(bool ok, string mensaje)> InscribirseAsync(InscribirseMonitoriaCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/inscribirse-monitoria", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al inscribirse.");
    }

    public async Task<(bool ok, string mensaje)> DesinscribirseAsync(int idInscripcion, int idUsuario)
    {
        var response = await httpClient.DeleteAsync(
            $"api/desinscribirse-monitoria/{idInscripcion}?idUsuario={idUsuario}");
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al desinscribirse.");
    }

    public async Task<(bool ok, string mensaje)> RegistrarAsistenciaAsync(RegistrarAsistenciaMonitoriaCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/registrar-asistencia-monitoria", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al registrar la asistencia.");
    }

    public async Task<(bool ok, string mensaje)> RegistrarInformeAsync(RegistrarInformeMonitoriaCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/registrar-informe-monitoria", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al registrar el informe.");
    }

    private record MensajeError(string Mensaje);
}
