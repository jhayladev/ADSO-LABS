using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Aprendiz;
using AdsoLabs.Application.AppService.Informe;
using AdsoLabs.Application.DTOs.Informe;
using AdsoLabs.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[Route("api/aprendiz")]
[ApiController]
[Authorize]
public class InformeController(
    InformeAppService informeAppService,
    AprendizAppService aprendizAppService,
    IPerfilService perfilService) : ControllerBase
{
    // Un aprendiz solo puede consultar/editar su propia informacion (acudiente,
    // observaciones); Administrador/Instructor pueden consultar la de cualquiera
    // (uso existente desde Informe.razor).
    private async Task<bool> EsAccesoPermitidoAsync(int idAprendiz)
    {
        if (!User.IsInRole("Aprendiz")) return true;

        var perfil = await perfilService.ObtenerPerfilAsync(this.ObtenerIdUsuarioAutenticado());
        return perfil is not null && perfil.IdAprendiz == idAprendiz;
    }

    // ──────────────────────────────────────────────
    //  Datos del aprendiz (para encabezado del informe)
    // ──────────────────────────────────────────────

    [HttpGet("{idAprendiz}/datos")]
    [Authorize(Roles = "Administrador,Instructor")]
    public async Task<IActionResult> ObtenerDatos(int idAprendiz)
    {
        var result = await aprendizAppService.ObtenerDetallesAprendizAsync(idAprendiz);
        return result is null ? NotFound() : Ok(result);
    }

    // ──────────────────────────────────────────────
    //  Acudiente
    // ──────────────────────────────────────────────

    [HttpGet("{idAprendiz}/acudiente")]
    public async Task<IActionResult> ObtenerAcudiente(int idAprendiz)
    {
        if (!await EsAccesoPermitidoAsync(idAprendiz)) return Forbid();
        var result = await informeAppService.ObtenerAcudienteAsync(idAprendiz);
        return result is null ? NoContent() : Ok(result);
    }

    [HttpPost("{idAprendiz}/acudiente")]
    public async Task<IActionResult> GuardarAcudiente(int idAprendiz, [FromBody] GuardarAcudienteDTO dto)
    {
        if (!await EsAccesoPermitidoAsync(idAprendiz)) return Forbid();
        await informeAppService.GuardarAcudienteAsync(idAprendiz, dto);
        return Ok();
    }

    // ──────────────────────────────────────────────
    //  Observaciones
    // ──────────────────────────────────────────────

    [HttpGet("{idAprendiz}/observaciones")]
    public async Task<IActionResult> ObtenerObservaciones(int idAprendiz)
    {
        if (!await EsAccesoPermitidoAsync(idAprendiz)) return Forbid();
        var result = await informeAppService.ObtenerObservacionesAsync(idAprendiz);
        return Ok(result);
    }

    [HttpPost("{idAprendiz}/observacion")]
    [Authorize(Roles = "Administrador,Instructor")]
    public async Task<IActionResult> AgregarObservacion(int idAprendiz, [FromBody] AgregarObservacionDTO dto)
    {
        await informeAppService.AgregarObservacionAsync(idAprendiz, dto);
        return Ok();
    }

    // ──────────────────────────────────────────────
    //  Competencias sin calificar
    // ──────────────────────────────────────────────

    [HttpGet("{idAprendiz}/competencias-sin-calificar")]
    [Authorize(Roles = "Administrador,Instructor")]
    public async Task<IActionResult> ObtenerCompetenciasSinCalificar(
        int idAprendiz, [FromQuery] string numeroFicha)
    {
        var result = await informeAppService.ObtenerCompetenciasSinCalificarAsync(idAprendiz, numeroFicha);
        return Ok(result);
    }
}
