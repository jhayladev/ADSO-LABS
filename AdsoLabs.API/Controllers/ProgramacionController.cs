using AdsoLabs.Application.AppService.Programacion;
using AdsoLabs.Application.DTOs.Fichas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[Route("api/")]
[ApiController]
[Authorize(Roles = "Administrador,Instructor")]
public class ProgramacionController(ProgramacionAppService programacionAppService) : ControllerBase
{
    /// <summary>
    /// Devuelve los resultados de aprendizaje de una competencia dentro del contexto
    /// de una ficha, con su estado de programación actual.
    /// </summary>
    [HttpGet("obtener-resultados-ficha-competencia")]
    public async Task<IActionResult> ObtenerResultadosAsync(
        [FromQuery] string numeroFicha,
        [FromQuery] int    idCompetencia)
        => Ok(await programacionAppService.ObtenerResultadosPorFichaYCompetenciaAsync(
                numeroFicha, idCompetencia));

    [HttpGet("obtener-instructores-activos")]
    public async Task<IActionResult> ObtenerInstructoresActivosAsync()
        => Ok(await programacionAppService.ObtenerInstructoresActivosAsync());

    /// <summary>
    /// Crea o actualiza la ventana de programación de una competencia en una ficha
    /// (FechaInicio, FechaFin, TotalHoras). Debe llamarse antes de programar resultados.
    /// </summary>
    [HttpPost("configurar-competencia-ficha")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ConfigurarFichaCompetenciaAsync(
        [FromBody] ConfigurarFichaCompetenciaDTO dto)
    {
        var resultado = await programacionAppService.ConfigurarFichaCompetenciaAsync(dto);
        return resultado.esExitoso ? Ok(resultado) : BadRequest(resultado);
    }

    /// <summary>
    /// Programa un resultado de aprendizaje dentro de la ficha dada.
    /// El backend resuelve el IdFichaCompetencia a partir de NumeroFicha + IdCompetencia.
    /// </summary>
    [HttpPost("programar-resultado")]
    public async Task<IActionResult> ProgramarResultadoAsync([FromBody] ProgramarResultadoDTO dto)
    {
        var resultado = await programacionAppService.ProgramarResultadoAsync(dto);
        return resultado.esExitoso ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("eliminar-programacion-resultado/{idFichaCompetenciaResultado:int}")]
    public async Task<IActionResult> EliminarProgramacionResultadoAsync(int idFichaCompetenciaResultado)
    {
        var resultado = await programacionAppService.EliminarProgramacionResultadoAsync(idFichaCompetenciaResultado);
        return resultado.esExitoso ? Ok(resultado) : BadRequest(resultado);
    }
}
