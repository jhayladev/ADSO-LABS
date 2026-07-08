using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

/// <summary>
/// Endpoints del dashboard del usuario autenticado (idUsuario se deriva
/// del token, no se acepta como parametro del cliente).
/// </summary>
[ApiController]
[Route("api/mi-dashboard")]
[Authorize(Roles = "Administrador,Instructor")]
public class DashboardController(DashboardAppService dashboardAppService) : ControllerBase
{
    /// <summary>
    /// Resumen de cabecera: nombre, rol, última actividad, fichas activas,
    /// horas planeadas, progreso promedio e instructores técnicos.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerResumenAsync()
    {
        var resultado = await dashboardAppService.ObtenerResumenAsync(this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? NotFound() : Ok(resultado);
    }

    /// <summary>
    /// Conteo de competencias por estado (Total, Finalizadas, En Ejecución,
    /// Próximas a cerrar, Pendientes) para las fichas del usuario.
    /// </summary>
    [HttpGet("competencias")]
    public async Task<IActionResult> ObtenerEstadoCompetenciasAsync()
    {
        var resultado = await dashboardAppService.ObtenerEstadoCompetenciasAsync(this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? NotFound() : Ok(resultado);
    }

    /// <summary>
    /// Horas planeadas vs ejecutadas por ficha activa del usuario.
    /// </summary>
    [HttpGet("progreso-fichas")]
    public async Task<IActionResult> ObtenerProgresoFichasAsync()
    {
        return Ok(await dashboardAppService.ObtenerProgresoFichasAsync(this.ObtenerIdUsuarioAutenticado()));
    }

    /// <summary>
    /// Alertas clasificadas: baja asistencia, retraso de horas, al día
    /// y fichas que culminan esta semana.
    /// </summary>
    [HttpGet("alertas")]
    public async Task<IActionResult> ObtenerAlertasAsync()
    {
        return Ok(await dashboardAppService.ObtenerAlertasAsync(this.ObtenerIdUsuarioAutenticado()));
    }
}
