using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Reportes;
using AdsoLabs.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Administrador,Instructor")]
public class ReportesController(
    ReportesAppService reportesAppService,
    InformeExcelGenerator excelGenerator) : ControllerBase
{
    // ── E: Resumen global ────────────────────────────────────────────────────

    [HttpGet("global")]
    public async Task<IActionResult> ObtenerResumenGlobal()
    {
        var resultado = await reportesAppService.ObtenerResumenGlobalAsync(this.ObtenerIdUsuarioAutenticado());
        return Ok(resultado);
    }

    // ── B: Asistencia por ficha ──────────────────────────────────────────────

    [HttpGet("asistencia/{numeroFicha}")]
    public async Task<IActionResult> ObtenerAsistenciaFicha(string numeroFicha)
    {
        var resultado = await reportesAppService.ObtenerAsistenciaFichaAsync(numeroFicha, this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? Forbid() : Ok(resultado);
    }

    // ── C: Progreso de competencias por ficha ────────────────────────────────

    [HttpGet("competencias/{numeroFicha}")]
    public async Task<IActionResult> ObtenerProgresoCompetencias(string numeroFicha)
    {
        var resultado = await reportesAppService.ObtenerProgresoCompetenciasAsync(numeroFicha, this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? Forbid() : Ok(resultado);
    }

    // ── D: Juicios evaluativos por ficha ─────────────────────────────────────

    [HttpGet("juicios/{numeroFicha}")]
    public async Task<IActionResult> ObtenerJuicios(string numeroFicha)
    {
        var resultado = await reportesAppService.ObtenerJuiciosAsync(numeroFicha, this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? Forbid() : Ok(resultado);
    }

    // ── F: Por instructor ────────────────────────────────────────────────────

    [HttpGet("instructor/{idInstructor:int}")]
    public async Task<IActionResult> ObtenerReporteInstructor(int idInstructor)
    {
        var resultado = await reportesAppService.ObtenerReporteInstructorAsync(idInstructor, this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpGet("instructor/selector")]
    public async Task<IActionResult> ObtenerSelectorInstructores()
    {
        var resultado = await reportesAppService.ObtenerSelectorInstructoresAsync();
        return Ok(resultado);
    }

    // ── G: Por competencia ───────────────────────────────────────────────────

    [HttpGet("competencia/{idCompetencia:int}")]
    public async Task<IActionResult> ObtenerReporteCompetencia(int idCompetencia)
    {
        var resultado = await reportesAppService.ObtenerReporteCompetenciaAsync(idCompetencia, this.ObtenerIdUsuarioAutenticado());
        return resultado is null ? NotFound() : Ok(resultado);
    }

    // ── H: Patrocinio ────────────────────────────────────────────────────────

    [HttpGet("patrocinio")]
    public async Task<IActionResult> ObtenerPatrocinios()
    {
        var resultado = await reportesAppService.ObtenerPatrociniosAsync(this.ObtenerIdUsuarioAutenticado());
        return Ok(resultado);
    }

    // ── Exportación Excel ────────────────────────────────────────────────────

    [HttpGet("asistencia/{numeroFicha}/excel")]
    public async Task<IActionResult> ExportarAsistenciaExcel(string numeroFicha)
    {
        var datos = await reportesAppService.ObtenerAsistenciaFichaAsync(numeroFicha, this.ObtenerIdUsuarioAutenticado());
        if (datos is null) return Forbid();

        var bytes = excelGenerator.GenerarAsistencia(datos);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Asistencia_{numeroFicha}.xlsx");
    }

    [HttpGet("competencias/{numeroFicha}/excel")]
    public async Task<IActionResult> ExportarCompetenciasExcel(string numeroFicha)
    {
        var datos = await reportesAppService.ObtenerProgresoCompetenciasAsync(numeroFicha, this.ObtenerIdUsuarioAutenticado());
        if (datos is null) return Forbid();

        var bytes = excelGenerator.GenerarProgresoCompetencias(datos);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Competencias_{numeroFicha}.xlsx");
    }

    [HttpGet("global/excel")]
    public async Task<IActionResult> ExportarGlobalExcel()
    {
        var datos = await reportesAppService.ObtenerResumenGlobalAsync(this.ObtenerIdUsuarioAutenticado());
        var bytes = excelGenerator.GenerarResumenGlobal(datos);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ResumenFichas.xlsx");
    }
}
