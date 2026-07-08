using AdsoLabs.Application.DTOs.Patrocinio;

namespace AdsoLabs.Web.Services.Api;

public class PatrocinioApiService(HttpClient httpClient)
{
    // idUsuario ya no se manda: se deriva del token interno (ver InternalTokenHandler).
    public async Task<List<PatrocinioDTO>> ObtenerPatrociniosAsync(int idUsuario)
    {
        var response = await httpClient.GetAsync("api/obtener-patrocinios");
        return await response.Content.ReadFromJsonAsync<List<PatrocinioDTO>>() ?? [];
    }

    public async Task<PatrocinioDTO?> ObtenerPatrocinioAsync(int idPatrocinio)
    {
        var response = await httpClient.GetAsync($"api/obtener-patrocinio/{idPatrocinio}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PatrocinioDTO>();
    }

    public async Task<List<AsistenciaPatrocinioDTO>> ObtenerAsistenciasAsync(int idPatrocinio)
    {
        var response = await httpClient.GetAsync($"api/obtener-asistencias-patrocinio/{idPatrocinio}");
        return await response.Content.ReadFromJsonAsync<List<AsistenciaPatrocinioDTO>>() ?? [];
    }

    public async Task<(bool ok, string mensaje)> AgregarPatrocinioAsync(AgregarPatrocinioCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/agregar-patrocinio", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al agregar el patrocinio.");
    }

    public async Task<(bool ok, string mensaje)> ActualizarPatrocinioAsync(ActualizarPatrocinioCommand command)
    {
        var response = await httpClient.PutAsJsonAsync("api/actualizar-patrocinio", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al actualizar el patrocinio.");
    }

    public async Task<(bool ok, string mensaje)> DesactivarPatrocinioAsync(int idPatrocinio)
    {
        var response = await httpClient.PutAsync($"api/desactivar-patrocinio/{idPatrocinio}", null);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al desactivar el patrocinio.");
    }

    public async Task<(bool ok, string mensaje)> RegistrarAsistenciasAsync(RegistrarAsistenciasCommand command)
    {
        var response = await httpClient.PostAsJsonAsync("api/registrar-asistencias-patrocinio", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al registrar asistencias.");
    }

    public async Task<ResumenAsistenciaDTO?> ObtenerResumenAsistenciaAsync(int idAprendiz, int idFicha)
    {
        var response = await httpClient.GetAsync(
            $"api/resumen-asistencia-patrocinio?idAprendiz={idAprendiz}&idFicha={idFicha}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ResumenAsistenciaDTO>();
    }

    public async Task<(bool ok, string mensaje)> AsignarHorarioAsync(AsignarHorarioCommand command)
    {
        var response = await httpClient.PutAsJsonAsync("api/asignar-horario-patrocinio", command);
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al asignar el horario.");
    }

    public async Task<(bool ok, string mensaje)> EliminarHorarioAsync(int idPatrocinio)
    {
        var response = await httpClient.DeleteAsync($"api/eliminar-horario-patrocinio/{idPatrocinio}");
        if (response.IsSuccessStatusCode) return (true, string.Empty);
        var error = await response.Content.ReadFromJsonAsync<MensajeError>();
        return (false, error?.Mensaje ?? "Error al eliminar el horario.");
    }

    private record MensajeError(string Mensaje);
}
