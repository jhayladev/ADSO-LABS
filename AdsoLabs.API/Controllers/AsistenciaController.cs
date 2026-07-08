using AdsoLabs.Application.AppService.Asistencia;
using AdsoLabs.Application.DTOs.Asistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[Route("api/")]
[ApiController]
[Authorize(Roles = "Administrador,Instructor")]
public class AsistenciaController(AsistenciaAppService asistenciaAppService) : ControllerBase
{
    /// <summary>
    /// Resultados oficialmente programados (con fechas + horario + instructor) de una ficha.
    /// Solo muestra resultados en estado Programado o Completado.
    /// </summary>
    [HttpGet("obtener-resultados-programados-ficha")]
    public async Task<IActionResult> ObtenerResultadosProgramadosAsync(
        [FromQuery] string numeroFicha)
        => Ok(await asistenciaAppService.ObtenerResultadosProgramadosAsync(numeroFicha));

    /// <summary>
    /// Datos de un resultado programado específico (para cabecera de pantalla de sesiones).
    /// </summary>
    [HttpGet("obtener-resultado-programado")]
    public async Task<IActionResult> ObtenerResultadoProgramadoAsync(
        [FromQuery] int idFichaCompetenciaResultado)
        => Ok(await asistenciaAppService.ObtenerResultadoProgramadoAsync(idFichaCompetenciaResultado));

    /// <summary>
    /// Historial de sesiones reales de un resultado programado específico.
    /// </summary>
    [HttpGet("obtener-sesiones-resultado")]
    public async Task<IActionResult> ObtenerSesionesPorResultadoAsync(
        [FromQuery] int idFichaCompetenciaResultado)
        => Ok(await asistenciaAppService.ObtenerSesionesPorResultadoAsync(idFichaCompetenciaResultado));

    /// <summary>
    /// Datos básicos de una sesión (fecha, horario, estado) para cabecera de la pantalla de registro.
    /// </summary>
    [HttpGet("obtener-info-sesion")]
    public async Task<IActionResult> ObtenerInfoSesionAsync(
        [FromQuery] int idSesion)
    {
        var sesion = await asistenciaAppService.ObtenerInfoSesionAsync(idSesion);
        return sesion is not null ? Ok(sesion) : NotFound();
    }

    /// <summary>
    /// Aprendices EN FORMACION con su estado de asistencia para la sesión indicada.
    /// Estado = null si aún no tiene registro.
    /// </summary>
    [HttpGet("obtener-aprendices-sesion")]
    public async Task<IActionResult> ObtenerAprendicesSesionAsync(
        [FromQuery] int idSesion)
        => Ok(await asistenciaAppService.ObtenerAprendicesSesionAsync(idSesion));

    /// <summary>
    /// Crea una nueva sesión real para el resultado programado indicado.
    /// Devuelve IdCreado con el IdSesion generado.
    /// </summary>
    [HttpPost("crear-sesion")]
    public async Task<IActionResult> CrearSesionAsync([FromBody] CrearSesionDTO dto)
    {
        var resultado = await asistenciaAppService.CrearSesionAsync(dto);
        return resultado.esExitoso ? Ok(resultado) : BadRequest(resultado);
    }

    /// <summary>
    /// Registra o actualiza la asistencia de los aprendices en la sesión indicada.
    /// </summary>
    [HttpPost("guardar-asistencia")]
    public async Task<IActionResult> GuardarAsistenciaAsync([FromBody] GuardarAsistenciaDTO dto)
    {
        var resultado = await asistenciaAppService.GuardarAsistenciaAsync(dto);
        return resultado.esExitoso ? Ok(resultado) : BadRequest(resultado);
    }
}
