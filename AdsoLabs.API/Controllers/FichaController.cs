using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Fichas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize(Roles = "Administrador,Instructor")]
    public class FichaController(FichaAppService fichaAppService) : ControllerBase
    {
        [HttpGet("obtener-fichas")]
        public async Task<IActionResult> ObtenerFichasAsync()
        {
            return Ok(await fichaAppService.ObtenerFichasAsync(this.ObtenerIdUsuarioAutenticado()));
        }

        [HttpGet("obtener-detalles-ficha")]
        public async Task<IActionResult> ObtenerDatosAsync([FromQuery] string numeroFicha)
        {
            return Ok(await fichaAppService.ObtenerDatosAsync(numeroFicha));
        }

        [HttpGet("obtener-competencias-ficha")]
        public async Task<IActionResult> ObtenerCompetenciasAsync([FromQuery] string numeroFicha, [FromQuery] int idUsuario = 0)
        {
            // idUsuario aqui no filtra acceso (todas las competencias de la ficha se
            // devuelven igual) - solo marca EsMiCompetencia para el instructor indicado.
            // Puede ser "el que llama" o, en pantallas admin (PerfilInstructor.razor),
            // otro instructor cuya vista se esta consultando. No es un caso SELF.
            return Ok(await fichaAppService.ObtenerFichaCompetenciaAsync(numeroFicha, idUsuario));
        }

        [HttpGet("obtener-aprendices-ficha")]
        public async Task<IActionResult> ObtenerAprendicesAsync([FromQuery] string numeroFicha)
        {
            return Ok(await fichaAppService.ObtenerAprendicesAsync(numeroFicha));
        }
    }
}
