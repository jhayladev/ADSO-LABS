using AdsoLabs.Application.DTOs.Importacion;
using System.Net.Http.Json;

namespace AdsoLabs.Web.Services.Api;

/// <summary>
/// Cliente HTTP para todos los endpoints de importación de la API.
///
/// Agrupa tres responsabilidades:
///   1. Importación de juicios SOFIA Plus (endpoint legacy, ahora en Fichas.razor).
///   2. Historial y errores de importaciones registradas en BD.
///   3. Importación masiva de fichas (plantilla Excel propia).
/// </summary>
public class ImportacionApiService(HttpClient httpClient)
{
    // ══════════════════════════════════════════════════════════════════════════
    //  JUICIOS EVALUATIVOS (SOFIA Plus)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Envía el Reporte de Juicios Evaluativos (.xls) a la API y devuelve el resumen.
    /// Solo actualiza estados: no crea fichas ni aprendices (CLAUDE.md §7.5).
    /// </summary>
    // idUsuario ya no se manda: la API lo deriva del token interno (ver InternalTokenHandler).
    public async Task<ResultadoImportacionDto?> ImportarAsync(
        Stream archivo, string nombreArchivo, int idUsuario)
    {
        using var content   = new MultipartFormDataContent();
        var       filePart  = new StreamContent(archivo);
        filePart.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.ms-excel");
        content.Add(filePart, "archivo", nombreArchivo);

        var response = await httpClient.PostAsync("api/importar-reporte", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ResultadoImportacionDto>();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HISTORIAL DE IMPORTACIONES
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>Devuelve el historial de todas las importaciones registradas en BD.</summary>
    public async Task<List<ImportacionResumenDTO>> ObtenerHistorialAsync()
    {
        var response = await httpClient.GetAsync("api/importaciones");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<ImportacionResumenDTO>>() ?? [];
    }

    /// <summary>Devuelve el detalle de errores de una importación específica.</summary>
    public async Task<List<ImportacionErrorDTO>> ObtenerErroresAsync(int idImportacion)
    {
        var response = await httpClient.GetAsync($"api/importaciones/{idImportacion}/errores");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<ImportacionErrorDTO>>() ?? [];
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  IMPORTADOR DE FICHAS (plantilla xlsx propia)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Descarga la plantilla Excel oficial para importación de fichas.
    /// El servidor genera el archivo en memoria; no hay archivo estático en disco.
    /// </summary>
    /// <returns>Bytes del .xlsx o null si la API falla.</returns>
    public async Task<byte[]?> DescargarPlantillaFichasAsync()
    {
        var response = await httpClient.GetAsync("api/plantilla-fichas");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Envía la plantilla de fichas completada (.xlsx) a la API para su importación.
    /// Crea fichas nuevas, actualiza existentes y vincula competencias.
    /// </summary>
    public async Task<ResultadoImportacionFichaDto?> ImportarFichasAsync(
        Stream archivo, string nombreArchivo)
    {
        using var content  = new MultipartFormDataContent();
        var       filePart = new StreamContent(archivo);
        filePart.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(filePart, "archivo", nombreArchivo);

        var response = await httpClient.PostAsync("api/importar-fichas", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ResultadoImportacionFichaDto>();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  IMPORTADOR DE APRENDICES (plantilla xlsx propia)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Descarga la plantilla Excel oficial para importación de aprendices.
    /// El servidor genera el archivo en memoria; no hay archivo estático en disco.
    /// </summary>
    /// <returns>Bytes del .xlsx o null si la API falla.</returns>
    public async Task<byte[]?> DescargarPlantillaAprendicesAsync()
    {
        var response = await httpClient.GetAsync("api/plantilla-aprendices");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    /// <summary>
    /// Envía la plantilla de aprendices completada (.xlsx) a la API para su importación.
    /// El número de ficha se pasa como parámetro de ruta, NO dentro del archivo.
    ///
    /// Crea o actualiza Persona + AprendizPerfil y vincula el aprendiz a la ficha.
    /// Aplica la regla de traslado automático (CLAUDE.md §7.9).
    /// </summary>
    /// <param name="archivo">Stream del archivo .xlsx seleccionado por el usuario.</param>
    /// <param name="nombreArchivo">Nombre original del archivo (para el Content-Disposition).</param>
    /// <param name="numeroFicha">Número de ficha al que se vincularán los aprendices.</param>
    public async Task<ResultadoImportacionAprendizDto?> ImportarAprendicesAsync(
        Stream archivo, string nombreArchivo, string numeroFicha)
    {
        using var content  = new MultipartFormDataContent();
        var       filePart = new StreamContent(archivo);
        filePart.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(filePart, "archivo", nombreArchivo);

        var response = await httpClient.PostAsync(
            $"api/importar-aprendices/{Uri.EscapeDataString(numeroFicha)}", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ResultadoImportacionAprendizDto>();
    }
}
