using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Patrocinio;
using AdsoLabs.Application.DTOs.Patrocinio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[Route("api/")]
[ApiController]
[Authorize(Roles = "Administrador,Instructor")]
public class PatrocinioController(PatrocinioAppService appService) : ControllerBase
{
    [HttpGet("obtener-patrocinios")]
    public async Task<IActionResult> ObtenerPatrocinios()
        => Ok(await appService.ObtenerPatrociniosAsync(this.ObtenerIdUsuarioAutenticado()));

    [HttpGet("obtener-patrocinio/{idPatrocinio:int}")]
    public async Task<IActionResult> ObtenerPatrocinio(int idPatrocinio)
    {
        var resultado = await appService.ObtenerPatrocinioAsync(idPatrocinio);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpGet("obtener-asistencias-patrocinio/{idPatrocinio:int}")]
    public async Task<IActionResult> ObtenerAsistencias(int idPatrocinio)
        => Ok(await appService.ObtenerAsistenciasAsync(idPatrocinio));

    [HttpPost("agregar-patrocinio")]
    public async Task<IActionResult> AgregarPatrocinio([FromBody] AgregarPatrocinioCommand command)
    {
        try
        {
            var id = await appService.AgregarPatrocinioAsync(command);
            return Ok(new { IdPatrocinio = id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
    }

    [HttpPut("actualizar-patrocinio")]
    public async Task<IActionResult> ActualizarPatrocinio([FromBody] ActualizarPatrocinioCommand command)
    {
        try
        {
            await appService.ActualizarPatrocinioAsync(command);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
    }

    [HttpPut("desactivar-patrocinio/{idPatrocinio:int}")]
    public async Task<IActionResult> DesactivarPatrocinio(int idPatrocinio)
    {
        try
        {
            await appService.EliminarPatrocinioAsync(idPatrocinio);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Mensaje = ex.Message });
        }
    }

    [HttpPost("registrar-asistencias-patrocinio")]
    public async Task<IActionResult> RegistrarAsistencias([FromBody] RegistrarAsistenciasCommand command)
    {
        try
        {
            await appService.RegistrarAsistenciasAsync(command);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
    }

    [HttpGet("resumen-asistencia-patrocinio")]
    public async Task<IActionResult> ObtenerResumenAsistencia(
        [FromQuery] int idAprendiz,
        [FromQuery] int idFicha)
        => Ok(await appService.ObtenerResumenAsistenciaAsync(idAprendiz, idFicha));

    [HttpPut("asignar-horario-patrocinio")]
    public async Task<IActionResult> AsignarHorario([FromBody] AsignarHorarioCommand command)
    {
        try
        {
            await appService.AsignarHorarioAsync(command);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Mensaje = ex.Message });
        }
    }

    [HttpDelete("eliminar-horario-patrocinio/{idPatrocinio:int}")]
    public async Task<IActionResult> EliminarHorario(int idPatrocinio)
    {
        try
        {
            await appService.EliminarHorarioAsync(idPatrocinio);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Mensaje = ex.Message });
        }
    }
}
