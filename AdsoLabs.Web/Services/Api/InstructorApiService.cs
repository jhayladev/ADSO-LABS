using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Instructor;

namespace AdsoLabs.Web.Services.Api;

public class InstructorApiService(HttpClient httpClient)
{
    public async Task<List<InstructorListadoDTO>> ObtenerInstructoresAsync()
    {
        var response = await httpClient.GetAsync("api/obtener-instructores");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<InstructorListadoDTO>>() ?? [];
    }

    public async Task<ResumenInstructorDto> ObtenerResumenInstructoresAsync(
        string? competencia = null, string? instructor = null)
    {
        var url = $"api/obtener-resumen-instructores?competencia={competencia}&instructor={instructor}";
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<ResumenInstructorDto>() ?? new();
    }

    public async Task<InstructorDetalleDTO?> ObtenerDetallesInstructorAsync(int idInstructor)
    {
        var response = await httpClient.GetAsync($"api/obtener-detalles-instructor?idInstructor={idInstructor}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InstructorDetalleDTO>();
    }

    public async Task<List<InstructorListadoDTO>> ObtenerInstructoresPorCompetenciaAsync(string nombreCompetencia)
    {
        var response = await httpClient.GetAsync($"api/obtener-instructores-por-competencia?nombreCompetencia={nombreCompetencia}");
        return await response.Content.ReadFromJsonAsync<List<InstructorListadoDTO>>() ?? [];
    }

    public async Task<List<InstructorListadoDTO>> FiltrarInstructoresAsync(
        string? competencia = null, string? instructor = null)
    {
        var url = $"api/filtrar-instructores?competencia={competencia}&instructor={instructor}";
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<List<InstructorListadoDTO>>() ?? [];
    }

    public async Task<MensajeEstadoDTO> AgregarInstructorAsync(InstructorAgregarDTO instructor)
    {
        var response = await httpClient.PostAsJsonAsync("api/agregar-instructor", instructor);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }

    public async Task<MensajeEstadoDTO> EditarInstructorAsync(int idInstructor)
    {
        var response = await httpClient.PutAsync($"api/editar-instructor/{idInstructor}", null);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }

    public async Task<MensajeEstadoDTO> ActualizarContactoInstructorAsync(ActualizarContactoInstructorDTO dto)
    {
        var response = await httpClient.PutAsJsonAsync("api/actualizar-contacto-instructor", dto);
        return await response.Content.ReadFromJsonAsync<MensajeEstadoDTO>() ?? new();
    }

    public async Task<List<AvisoInstructorDTO>> ObtenerAvisosInstructorAsync(int idInstructor)
    {
        var response = await httpClient.GetAsync($"api/obtener-avisos-instructor?idInstructor={idInstructor}");
        return await response.Content.ReadFromJsonAsync<List<AvisoInstructorDTO>>() ?? [];
    }

    public async Task<List<HorarioInstructorItemDTO>> ObtenerHorarioInstructorAsync(int idInstructor)
    {
        var response = await httpClient.GetAsync($"api/obtener-horario-instructor?idInstructor={idInstructor}");
        return await response.Content.ReadFromJsonAsync<List<HorarioInstructorItemDTO>>() ?? [];
    }
}
