using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.PrediccionIA;
using AdsoLabs.Application.DTOs.PrediccionIA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Instructor")]
public class PrediccionIAController(PrediccionIAAppService appService) : ControllerBase
{
    [HttpGet("compatibilidad")]
    public async Task<IActionResult> ObtenerCompatibilidad(CancellationToken ct)
    {
        try
        {
            var result = await appService.ObtenerCompatibilidadAsync(this.ObtenerIdUsuarioAutenticado(), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { mensaje = ex.Message });
        }
    }

    [HttpGet("competencias-asignadas")]
    public async Task<IActionResult> ObtenerCompetenciasAsignadas(CancellationToken ct)
    {
        try
        {
            var result = await appService.ObtenerCompetenciasAsignadasAsync(this.ObtenerIdUsuarioAutenticado(), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("prediccion-aprendizaje")]
    public async Task<IActionResult> ObtenerPrediccionAprendizaje(
        [FromQuery] int idFichaCompetencia,
        [FromQuery] bool forzarActualizacion,
        CancellationToken ct)
    {
        try
        {
            var result = await appService.ObtenerPrediccionAprendizajeAsync(this.ObtenerIdUsuarioAutenticado(), idFichaCompetencia, forzarActualizacion, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(503, new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error inesperado: {ex.Message}" });
        }
    }

    [HttpPost("solicitud")]
    public async Task<IActionResult> EnviarSolicitud([FromBody] EnviarSolicitudCommand command, CancellationToken ct)
    {
        try
        {
            await appService.EnviarSolicitudAsync(command, ct);
            return Ok(new { mensaje = "Solicitud enviada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpGet("solicitudes")]
    public async Task<IActionResult> ObtenerSolicitudes(CancellationToken ct)
    {
        var result = await appService.ObtenerSolicitudesAsync(ct);
        return Ok(result);
    }

    [HttpPut("solicitud/{idSolicitud}/responder")]
    public async Task<IActionResult> ResponderSolicitud(int idSolicitud, [FromBody] ResponderSolicitudCommand command, CancellationToken ct)
    {
        try
        {
            var cmd = command with { IdSolicitud = idSolicitud };
            await appService.ResponderSolicitudAsync(cmd, ct);
            return Ok(new { mensaje = "Solicitud actualizada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
