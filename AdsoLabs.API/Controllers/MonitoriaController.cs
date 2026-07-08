using AdsoLabs.Application.AppService.Monitoria;
using AdsoLabs.Application.DTOs.Monitoria;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

// Nota: la API no tiene autenticación/autorización configurada a nivel de middleware
// (ver AdsoLabs.API/Program.cs). Por eso, igual que el resto de controllers del proyecto
// (PatrocinioController, AprendizController, etc.), este controller no usa [Authorize];
// el control de acceso por rol se aplica en las páginas Blazor ([Authorize] en los .razor).
[Route("api/")]
[ApiController]
public class MonitoriaController(MonitoriaAppService appService) : ControllerBase
{
    [HttpPost("asignar-monitor")]
    public async Task<IActionResult> AsignarMonitor([FromBody] AsignarMonitorCommand command)
    {
        try
        {
            var id = await appService.AsignarMonitorAsync(command);
            return Ok(new { IdMonitor = id });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpPut("desactivar-monitor/{idAprendiz:int}")]
    public async Task<IActionResult> DesactivarMonitor(int idAprendiz)
    {
        try
        {
            await appService.DesactivarMonitorAsync(idAprendiz);
            return Ok();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpGet("obtener-monitores")]
    public async Task<IActionResult> ObtenerMonitores()
        => Ok(await appService.ObtenerMonitoresAsync());

    [HttpGet("obtener-estado-monitor")]
    public async Task<IActionResult> ObtenerEstadoMonitor([FromQuery] int idUsuario)
        => Ok(await appService.ObtenerEstadoMonitorAsync(idUsuario));

    [HttpGet("obtener-monitorias-vigentes")]
    public async Task<IActionResult> ObtenerMonitoriasVigentes([FromQuery] int idUsuario)
        => Ok(await appService.ObtenerMonitoriasVigentesAsync(idUsuario));

    [HttpGet("obtener-mis-sesiones-monitoria")]
    public async Task<IActionResult> ObtenerMisSesiones([FromQuery] int idUsuario)
        => Ok(await appService.ObtenerMisSesionesAsync(idUsuario));

    [HttpPost("crear-sesion-monitoria")]
    public async Task<IActionResult> CrearSesion([FromBody] CrearSesionMonitoriaCommand command)
    {
        try
        {
            var id = await appService.CrearSesionAsync(command);
            return Ok(new { IdSesionMonitoria = id });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpPut("cancelar-sesion-monitoria/{idSesionMonitoria:int}")]
    public async Task<IActionResult> CancelarSesion(int idSesionMonitoria, [FromQuery] int idUsuario)
    {
        try
        {
            await appService.CancelarSesionAsync(idSesionMonitoria, idUsuario);
            return Ok();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpGet("obtener-inscritos-monitoria/{idSesionMonitoria:int}")]
    public async Task<IActionResult> ObtenerInscritos(int idSesionMonitoria)
        => Ok(await appService.ObtenerInscritosAsync(idSesionMonitoria));

    [HttpPost("inscribirse-monitoria")]
    public async Task<IActionResult> Inscribirse([FromBody] InscribirseMonitoriaCommand command)
    {
        try
        {
            var id = await appService.InscribirseAsync(command);
            return Ok(new { IdInscripcion = id });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpDelete("desinscribirse-monitoria/{idInscripcion:int}")]
    public async Task<IActionResult> Desinscribirse(int idInscripcion, [FromQuery] int idUsuario)
    {
        try
        {
            await appService.DesinscribirseAsync(idInscripcion, idUsuario);
            return Ok();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpPost("registrar-asistencia-monitoria")]
    public async Task<IActionResult> RegistrarAsistencia([FromBody] RegistrarAsistenciaMonitoriaCommand command)
    {
        try
        {
            await appService.RegistrarAsistenciaAsync(command);
            return Ok();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpPost("registrar-informe-monitoria")]
    public async Task<IActionResult> RegistrarInforme([FromBody] RegistrarInformeMonitoriaCommand command)
    {
        try
        {
            await appService.RegistrarInformeAsync(command);
            return Ok();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { Mensaje = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { Mensaje = ex.Message }); }
    }

    [HttpGet("obtener-asistencias-recibidas-monitoria")]
    public async Task<IActionResult> ObtenerAsistenciasRecibidas([FromQuery] int? idInstructor)
        => Ok(await appService.ObtenerAsistenciasRecibidasAsync(idInstructor));

    [HttpGet("obtener-informes-recibidos-monitoria")]
    public async Task<IActionResult> ObtenerInformesRecibidos([FromQuery] int? idInstructor)
        => Ok(await appService.ObtenerInformesRecibidosAsync(idInstructor));
}
