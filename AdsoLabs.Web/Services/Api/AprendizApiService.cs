using AdsoLabs.Application.DTOs.Aprendiz;
using Microsoft.AspNetCore.Components.Forms;

namespace AdsoLabs.Web.Services.Api;

public class AprendizApiService(HttpClient httpClient)
{
    public async Task<List<AprendizListadoDTO>> ObtenerAprendicesAsync()
    {
        var response = await httpClient.GetAsync("api/obtener-aprendices");
        return await response.Content.ReadFromJsonAsync<List<AprendizListadoDTO>>() ?? [];
    }

    public async Task<ResumenAprendizDto> ObtenerResumenAsync(
        string? ficha = null, string? documento = null, string? nombre = null)
    {
        var url = $"api/obtener-resumen-aprendices?ficha={ficha}&documento={documento}&nombre={nombre}";
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<ResumenAprendizDto>() ?? new();
    }

    public async Task<List<AprendizListadoDTO>> FiltrarAprendicesAsync(
        string? numeroFicha, string? numeroDocumento, string? nombreAprendiz)
    {
        var url = $"api/filtrar-aprendices?numeroFicha={numeroFicha}&numeroDocumento={numeroDocumento}&nombreAprendiz={nombreAprendiz}";
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadFromJsonAsync<List<AprendizListadoDTO>>() ?? [];
    }

    public async Task<bool> CambiarEstadoAsync(int idAprendiz, int idFicha, string nuevoEstado)
    {
        using var body = new StringContent(
            $"\"{nuevoEstado}\"",
            System.Text.Encoding.UTF8,
            "application/json");
        var response = await httpClient.PutAsync($"api/{idAprendiz}/estado?idFicha={idFicha}", body);
        return response.IsSuccessStatusCode;
    }

    public async Task<ResultadoImportacionDto> ImportarReporteAsync(IBrowserFile archivo, int idUsuario)
    {
        // Bufferear el archivo completo en memoria ANTES de enviarlo.
        // En Blazor Server, OpenReadStream() transmite datos a través del circuito
        // SignalR. Si intentamos leer ese stream mientras HttpClient también lo lee
        // para el POST, ambas operaciones compiten por el mismo canal y el circuito colapsa.
        using var buffer = new MemoryStream();
        await using var browserStream = archivo.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        await browserStream.CopyToAsync(buffer);
        buffer.Position = 0;

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(buffer);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);
        content.Add(fileContent, "archivo", archivo.Name);
        content.Add(new StringContent(idUsuario.ToString()), "idUsuario");

        var response = await httpClient.PostAsync("api/importar-reporte", content);
        if (!response.IsSuccessStatusCode)
            return new ResultadoImportacionDto
            {
                Errores = [$"Error del servidor: {(int)response.StatusCode} {response.ReasonPhrase}"]
            };

        return await response.Content.ReadFromJsonAsync<ResultadoImportacionDto>() ?? new();
    }
}
