using AdsoLabs.Application.AppService.Competencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize(Roles = "Administrador,Instructor")]
    public class CompetenciaController(CompetenciaAppService competenciaAppService) : ControllerBase
    {
        [HttpGet("obtener-competencias")]
        public async Task<IActionResult> GetCompetenciasAsync()
        {
            return Ok(await competenciaAppService.ObtenerCompetenciasAsync());
        }

        [HttpGet("obtener-datos-competencia")]
        public async Task<IActionResult> GetDatosCompetenciaAsync([FromQuery] string nombreCompetencia)
        {
            var resultado = await competenciaAppService.ObtenerDatosCompetenciasAsync(nombreCompetencia);
            return Ok(resultado);
        }
    }
}
