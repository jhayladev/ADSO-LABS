using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Repositorio;
using AdsoLabs.Application.Interfaces.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdsoLabs.API.Controllers;

[ApiController]
[Route("api/repositorio")]
[Authorize(Roles = "Administrador,Instructor")]
public class RepositorioController(
    RepositorioAppService appService,
    IRepositorioQueryService queryService) : ControllerBase
{
    // ─── LISTADOS ────────────────────────────────────────────────────────────

    [HttpGet("guias")]
    public async Task<IActionResult> ObtenerGuias([FromQuery] string? filtro, CancellationToken ct)
        => Ok(await queryService.ObtenerGuiasAsync(filtro, ct));

    [HttpGet("instrumentos")]
    public async Task<IActionResult> ObtenerInstrumentos([FromQuery] string? filtro, CancellationToken ct)
        => Ok(await queryService.ObtenerInstrumentosAsync(filtro, ct));

    [HttpGet("planeaciones")]
    public async Task<IActionResult> ObtenerPlaneaciones([FromQuery] string? filtro, CancellationToken ct)
        => Ok(await queryService.ObtenerPlaneacionesAsync(filtro, ct));

    [HttpGet("proyectos")]
    public async Task<IActionResult> ObtenerProyectos([FromQuery] string? filtro, CancellationToken ct)
        => Ok(await queryService.ObtenerProyectosAsync(filtro, ct));

    [HttpGet("desarrollo-curricular")]
    public async Task<IActionResult> ObtenerDesarrollos([FromQuery] string? filtro, CancellationToken ct)
        => Ok(await queryService.ObtenerDesarrollosAsync(filtro, ct));

    [HttpGet("planes-concertados")]
    public async Task<IActionResult> ObtenerPlanesConcertados([FromQuery] string? filtro, CancellationToken ct)
        => Ok(await queryService.ObtenerPlanesConcertadosAsync(filtro, ct));

    [HttpGet("mis-descargas")]
    public async Task<IActionResult> MisDescargas(CancellationToken ct)
        => Ok(await queryService.ObtenerMisDescargasAsync(this.ObtenerIdUsuarioAutenticado(), ct));

    // ─── LISTADOS POR FICHA (para sección Repositorio en detalle de ficha) ───

    [HttpGet("por-ficha/{numeroFicha}/guias")]
    public async Task<IActionResult> GuiasPorFicha(string numeroFicha, CancellationToken ct)
        => Ok(await queryService.ObtenerGuiasPorFichaAsync(numeroFicha, ct));

    [HttpGet("por-ficha/{numeroFicha}/planeaciones")]
    public async Task<IActionResult> PlaneacionesPorFicha(string numeroFicha, CancellationToken ct)
        => Ok(await queryService.ObtenerPlaneacionesPorFichaAsync(numeroFicha, ct));

    [HttpGet("por-ficha/{numeroFicha}/proyectos")]
    public async Task<IActionResult> ProyectosPorFicha(string numeroFicha, CancellationToken ct)
        => Ok(await queryService.ObtenerProyectosPorFichaAsync(numeroFicha, ct));

    // ─── SELECTORES PARA MODAL DE SUBIDA ────────────────────────────────────

    [HttpGet("selectores/fichas-competencias")]
    public async Task<IActionResult> FichasCompetencias(CancellationToken ct)
        => Ok(await queryService.ObtenerFichasCompetenciasAsync(ct));

    [HttpGet("selectores/competencias")]
    public async Task<IActionResult> Competencias(CancellationToken ct)
        => Ok(await queryService.ObtenerCompetenciasAsync(ct));

    [HttpGet("selectores/fichas")]
    public async Task<IActionResult> Fichas(CancellationToken ct)
        => Ok(await queryService.ObtenerFichasAsync(ct));

    [HttpGet("selectores/planes-sin-documento")]
    public async Task<IActionResult> PlanesSinDocumento(CancellationToken ct)
        => Ok(await queryService.ObtenerPlanesSinDocumentoAsync(ct));

    // ─── DESCARGA ────────────────────────────────────────────────────────────

    [HttpGet("descargar/{idArchivo:int}")]
    public async Task<IActionResult> Descargar(int idArchivo, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        var resultado = await appService.DescargarArchivoAsync(idArchivo, this.ObtenerIdUsuarioAutenticado(), ip, ct);
        if (resultado is null) return NotFound("Archivo no encontrado.");

        return File(resultado.Value.Stream, resultado.Value.MimeType, resultado.Value.NombreOriginal);
    }

    // ─── SUBIDAS ─────────────────────────────────────────────────────────────

    [HttpPost("guias/subir")]
    public async Task<IActionResult> SubirGuia(
        IFormFile archivo, [FromForm] string titulo,
        [FromForm] string? descripcion, [FromForm] int idFichaCompetencia,
        CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Archivo requerido.");
        await using var stream = archivo.OpenReadStream();
        var resultado = await appService.SubirGuiaAsync(stream, archivo.FileName, titulo, descripcion, idFichaCompetencia, this.ObtenerIdUsuarioAutenticado(), ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado.Error);
    }

    [HttpPost("instrumentos/subir")]
    public async Task<IActionResult> SubirInstrumento(
        IFormFile archivo, [FromForm] string nombre,
        [FromForm] string? descripcion, [FromForm] int idCompetencia,
        CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Archivo requerido.");
        await using var stream = archivo.OpenReadStream();
        var resultado = await appService.SubirInstrumentoAsync(stream, archivo.FileName, nombre, descripcion, idCompetencia, this.ObtenerIdUsuarioAutenticado(), ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado.Error);
    }

    [HttpPost("planeaciones/subir")]
    public async Task<IActionResult> SubirPlaneacion(
        IFormFile archivo, [FromForm] string? descripcion,
        [FromForm] int idFichaCompetencia, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Archivo requerido.");
        await using var stream = archivo.OpenReadStream();
        var resultado = await appService.SubirPlaneacionAsync(stream, archivo.FileName, descripcion, idFichaCompetencia, this.ObtenerIdUsuarioAutenticado(), ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado.Error);
    }

    [HttpPost("proyectos/subir")]
    public async Task<IActionResult> SubirProyecto(
        IFormFile archivo, [FromForm] string titulo,
        [FromForm] string? descripcion, [FromForm] int idFicha,
        CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Archivo requerido.");
        await using var stream = archivo.OpenReadStream();
        var resultado = await appService.SubirProyectoAsync(stream, archivo.FileName, titulo, descripcion, idFicha, this.ObtenerIdUsuarioAutenticado(), ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado.Error);
    }

    [HttpPost("desarrollo-curricular/subir")]
    public async Task<IActionResult> SubirDesarrollo(
        IFormFile archivo, [FromForm] string? descripcion,
        [FromForm] int idCompetencia, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Archivo requerido.");
        await using var stream = archivo.OpenReadStream();
        var resultado = await appService.SubirDesarrolloAsync(stream, archivo.FileName, descripcion, idCompetencia, this.ObtenerIdUsuarioAutenticado(), ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado.Error);
    }

    [HttpPost("planes-concertados/subir")]
    public async Task<IActionResult> SubirPlanConcertado(
        IFormFile archivo, [FromForm] int idPlan,
        CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0) return BadRequest("Archivo requerido.");
        await using var stream = archivo.OpenReadStream();
        var resultado = await appService.VincularDocumentoPlanAsync(stream, archivo.FileName, idPlan, this.ObtenerIdUsuarioAutenticado(), ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado.Error);
    }

    // ─── ELIMINACIÓN ─────────────────────────────────────────────────────────

    [HttpDelete("guias/{id:int}")]
    public async Task<IActionResult> EliminarGuia(int id, CancellationToken ct)
        => await Eliminar("Guia", id, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpDelete("instrumentos/{id:int}")]
    public async Task<IActionResult> EliminarInstrumento(int id, CancellationToken ct)
        => await Eliminar("Instrumento", id, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpDelete("planeaciones/{id:int}")]
    public async Task<IActionResult> EliminarPlaneacion(int id, CancellationToken ct)
        => await Eliminar("Planeacion", id, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpDelete("proyectos/{id:int}")]
    public async Task<IActionResult> EliminarProyecto(int id, CancellationToken ct)
        => await Eliminar("Proyecto", id, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpDelete("desarrollo-curricular/{id:int}")]
    public async Task<IActionResult> EliminarDesarrollo(int id, CancellationToken ct)
        => await Eliminar("Desarrollo", id, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpDelete("planes-concertados/{id:int}")]
    public async Task<IActionResult> EliminarPlan(int id, CancellationToken ct)
        => await Eliminar("Plan", id, this.ObtenerIdUsuarioAutenticado(), ct);

    // ─── CAMBIO DE ESTADO ────────────────────────────────────────────────────

    [HttpPut("guias/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoGuia(int id, [FromBody] string nuevoEstado, CancellationToken ct)
        => await CambiarEstado("Guia", id, nuevoEstado, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpPut("instrumentos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoInstrumento(int id, [FromBody] string nuevoEstado, CancellationToken ct)
        => await CambiarEstado("Instrumento", id, nuevoEstado, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpPut("planeaciones/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoPlaneacion(int id, [FromBody] string nuevoEstado, CancellationToken ct)
        => await CambiarEstado("Planeacion", id, nuevoEstado, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpPut("proyectos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoProyecto(int id, [FromBody] string nuevoEstado, CancellationToken ct)
        => await CambiarEstado("Proyecto", id, nuevoEstado, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpPut("desarrollo-curricular/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoDesarrollo(int id, [FromBody] string nuevoEstado, CancellationToken ct)
        => await CambiarEstado("Desarrollo", id, nuevoEstado, this.ObtenerIdUsuarioAutenticado(), ct);

    [HttpPut("planes-concertados/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoPlan(int id, [FromBody] string nuevoEstado, CancellationToken ct)
        => await CambiarEstado("Plan", id, nuevoEstado, this.ObtenerIdUsuarioAutenticado(), ct);

    // ─── HELPERS ─────────────────────────────────────────────────────────────

    private async Task<IActionResult> Eliminar(string tipoDoc, int id, int idUsuario, CancellationToken ct)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var resultado = await appService.EliminarDocumentoAsync(tipoDoc, id, idUsuario, rol, ct);
        if (!resultado.Exito)
            return resultado.Error!.Contains("permiso") ? Forbid() : NotFound(resultado.Error);
        return NoContent();
    }

    private async Task<IActionResult> CambiarEstado(string tipoDoc, int id, string nuevoEstado, int idUsuario, CancellationToken ct)
    {
        var rol = User.FindFirstValue(ClaimTypes.Role) ?? "";
        var resultado = await appService.CambiarEstadoAsync(tipoDoc, id, nuevoEstado, idUsuario, rol, ct);
        if (!resultado.Exito)
            return resultado.Error!.Contains("permiso") ? Forbid() : BadRequest(resultado.Error);
        return NoContent();
    }
}
