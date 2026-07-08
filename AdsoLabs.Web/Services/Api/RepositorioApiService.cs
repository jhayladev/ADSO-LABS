using AdsoLabs.Application.DTOs.Repositorio;
using System.Net.Http.Json;

namespace AdsoLabs.Web.Services.Api;

public class RepositorioApiService(HttpClient http)
{
    // ─── LISTADOS ────────────────────────────────────────────────────────────

    public Task<List<DocumentoListadoDTO>> ObtenerGuiasAsync(string? filtro = null)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/guias{ToQuery(filtro)}");

    public Task<List<DocumentoListadoDTO>> ObtenerInstrumentosAsync(string? filtro = null)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/instrumentos{ToQuery(filtro)}");

    public Task<List<DocumentoListadoDTO>> ObtenerPlaneacionesAsync(string? filtro = null)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/planeaciones{ToQuery(filtro)}");

    public Task<List<DocumentoListadoDTO>> ObtenerProyectosAsync(string? filtro = null)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/proyectos{ToQuery(filtro)}");

    public Task<List<DocumentoListadoDTO>> ObtenerDesarrollosAsync(string? filtro = null)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/desarrollo-curricular{ToQuery(filtro)}");

    public Task<List<DocumentoListadoDTO>> ObtenerPlanesConcertadosAsync(string? filtro = null)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/planes-concertados{ToQuery(filtro)}");

    // ── Por ficha ────────────────────────────────────────────────────────────

    public Task<List<DocumentoListadoDTO>> ObtenerGuiasPorFichaAsync(string numeroFicha)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/por-ficha/{Uri.EscapeDataString(numeroFicha)}/guias");

    public Task<List<DocumentoListadoDTO>> ObtenerPlaneacionesPorFichaAsync(string numeroFicha)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/por-ficha/{Uri.EscapeDataString(numeroFicha)}/planeaciones");

    public Task<List<DocumentoListadoDTO>> ObtenerProyectosPorFichaAsync(string numeroFicha)
        => GetListAsync<DocumentoListadoDTO>($"api/repositorio/por-ficha/{Uri.EscapeDataString(numeroFicha)}/proyectos");

    // idUsuario ya no se manda: se deriva del token interno (ver InternalTokenHandler).
    public Task<List<DescargaDTO>> ObtenerMisDescargasAsync(int idUsuario)
        => GetListAsync<DescargaDTO>("api/repositorio/mis-descargas");

    // ─── SELECTORES ──────────────────────────────────────────────────────────

    public Task<List<FichaCompetenciaSelectDTO>> ObtenerFichasCompetenciasAsync()
        => GetListAsync<FichaCompetenciaSelectDTO>("api/repositorio/selectores/fichas-competencias");

    public Task<List<CompetenciaSelectDTO>> ObtenerCompetenciasAsync()
        => GetListAsync<CompetenciaSelectDTO>("api/repositorio/selectores/competencias");

    public Task<List<FichaSelectDTO>> ObtenerFichasAsync()
        => GetListAsync<FichaSelectDTO>("api/repositorio/selectores/fichas");

    public Task<List<PlanParaSubirDTO>> ObtenerPlanesSinDocumentoAsync()
        => GetListAsync<PlanParaSubirDTO>("api/repositorio/selectores/planes-sin-documento");

    // ─── SUBIDAS ─────────────────────────────────────────────────────────────

    // idUsuario ya no se manda a la API en ninguno de los metodos de abajo:
    // se deriva del token interno (ver InternalTokenHandler). Los parametros
    // se mantienen para no tocar todos los llamadores existentes.

    public async Task<bool> SubirGuiaAsync(Stream archivo, string nombre, string titulo, string? descripcion, int idFichaCompetencia, int idUsuario)
    {
        using var content = BuildMultipart(archivo, nombre);
        content.Add(new StringContent(titulo), "titulo");
        if (descripcion is not null) content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(idFichaCompetencia.ToString()), "idFichaCompetencia");
        var resp = await http.PostAsync("api/repositorio/guias/subir", content);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> SubirInstrumentoAsync(Stream archivo, string nombre, string nombreDoc, string? descripcion, int idCompetencia, int idUsuario)
    {
        using var content = BuildMultipart(archivo, nombre);
        content.Add(new StringContent(nombreDoc), "nombre");
        if (descripcion is not null) content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(idCompetencia.ToString()), "idCompetencia");
        var resp = await http.PostAsync("api/repositorio/instrumentos/subir", content);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> SubirPlaneacionAsync(Stream archivo, string nombre, string? descripcion, int idFichaCompetencia, int idUsuario)
    {
        using var content = BuildMultipart(archivo, nombre);
        if (descripcion is not null) content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(idFichaCompetencia.ToString()), "idFichaCompetencia");
        var resp = await http.PostAsync("api/repositorio/planeaciones/subir", content);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> SubirProyectoAsync(Stream archivo, string nombre, string titulo, string? descripcion, int idFicha, int idUsuario)
    {
        using var content = BuildMultipart(archivo, nombre);
        content.Add(new StringContent(titulo), "titulo");
        if (descripcion is not null) content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(idFicha.ToString()), "idFicha");
        var resp = await http.PostAsync("api/repositorio/proyectos/subir", content);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> SubirDesarrolloAsync(Stream archivo, string nombre, string? descripcion, int idCompetencia, int idUsuario)
    {
        using var content = BuildMultipart(archivo, nombre);
        if (descripcion is not null) content.Add(new StringContent(descripcion), "descripcion");
        content.Add(new StringContent(idCompetencia.ToString()), "idCompetencia");
        var resp = await http.PostAsync("api/repositorio/desarrollo-curricular/subir", content);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> SubirPlanConcertadoAsync(Stream archivo, string nombre, int idPlan, int idUsuario)
    {
        using var content = BuildMultipart(archivo, nombre);
        content.Add(new StringContent(idPlan.ToString()), "idPlan");
        var resp = await http.PostAsync("api/repositorio/planes-concertados/subir", content);
        return resp.IsSuccessStatusCode;
    }

    // ─── DESCARGA ────────────────────────────────────────────────────────────

    /// <summary>Retorna los bytes del archivo para descarga client-side.</summary>
    public async Task<(byte[]? Bytes, string? ContentType, string? NombreArchivo)> DescargarAsync(int idArchivo, int idUsuario)
    {
        var resp = await http.GetAsync($"api/repositorio/descargar/{idArchivo}");
        if (!resp.IsSuccessStatusCode) return (null, null, null);

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        var ct    = resp.Content.Headers.ContentType?.MediaType;
        var cd    = resp.Content.Headers.ContentDisposition?.FileNameStar
                    ?? resp.Content.Headers.ContentDisposition?.FileName
                    ?? "archivo";
        return (bytes, ct, cd.Trim('"'));
    }

    // ─── ELIMINACIÓN ─────────────────────────────────────────────────────────

    public Task<bool> EliminarAsync(string tipoDoc, int id, int idUsuario) => DeleteAsync(tipoDoc, id, idUsuario);

    // ─── CAMBIO DE ESTADO ────────────────────────────────────────────────────

    public async Task<bool> CambiarEstadoAsync(string tipoDoc, int id, string nuevoEstado, int idUsuario)
    {
        var segmento = TipoDocToSegmento(tipoDoc);
        var resp = await http.PutAsJsonAsync($"api/repositorio/{segmento}/{id}/estado", nuevoEstado);
        return resp.IsSuccessStatusCode;
    }

    // ─── HELPERS PRIVADOS ────────────────────────────────────────────────────

    private async Task<List<T>> GetListAsync<T>(string url)
    {
        var resp = await http.GetAsync(url);
        if (!resp.IsSuccessStatusCode) return [];
        return await resp.Content.ReadFromJsonAsync<List<T>>() ?? [];
    }

    private async Task<bool> DeleteAsync(string tipoDoc, int id, int idUsuario)
    {
        var segmento = TipoDocToSegmento(tipoDoc);
        var resp = await http.DeleteAsync($"api/repositorio/{segmento}/{id}");
        return resp.IsSuccessStatusCode;
    }

    private static MultipartFormDataContent BuildMultipart(Stream archivo, string nombre)
    {
        var content  = new MultipartFormDataContent();
        var filePart = new StreamContent(archivo);
        content.Add(filePart, "archivo", nombre);
        return content;
    }

    private static string ToQuery(string? filtro)
        => string.IsNullOrWhiteSpace(filtro) ? "" : $"?filtro={Uri.EscapeDataString(filtro)}";

    private static string TipoDocToSegmento(string tipoDoc) => tipoDoc switch
    {
        "Guia"         => "guias",
        "Instrumento"  => "instrumentos",
        "Planeacion"   => "planeaciones",
        "Proyecto"     => "proyectos",
        "Desarrollo"   => "desarrollo-curricular",
        "Plan"         => "planes-concertados",
        _              => tipoDoc.ToLowerInvariant()
    };
}
