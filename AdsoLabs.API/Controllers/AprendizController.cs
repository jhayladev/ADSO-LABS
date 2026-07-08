using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Aprendiz;
using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize(Roles = "Administrador,Instructor")]
    public class AprendizController(AprendizAppService aprendizAppService) : ControllerBase
    {

        [HttpGet("obtener-aprendices")]
        public async Task<IActionResult> GetAprendicesAsync()
        {

            return Ok(await aprendizAppService.ObtenerAprendicesAsync());

        }

        [HttpPost("agregar-aprendiz")]
        public async Task<IActionResult> AgregarAprendizAsync(AprendizAgregarDTO aprendiz)
        {

            var resultado = await aprendizAppService.AgregarAprendizAsync(aprendiz);
            return resultado switch
            {

                ResultadoAgregar.Existe => BadRequest(new MensajeEstadoDTO() { esExitoso = false, Mensaje = "El aprendiz a registrar ya existe" }),
                ResultadoAgregar.Exitoso => Ok(new MensajeEstadoDTO() { esExitoso = true, Mensaje = "Se agrego al aprendiz exitosamente" }),
                _ => BadRequest(new MensajeEstadoDTO() { esExitoso = false, Mensaje = "Ocurrio un error al registrar el aprendiz" }),

            };

        }

        [HttpPut("{idAprendiz}/estado")]
        public async Task<IActionResult> EditarEstado(int idAprendiz, [FromQuery] int idFicha, [FromBody] string estado)
        {
            var idUsuarioCambio = this.ObtenerIdUsuarioAutenticado();
            var resultado = await aprendizAppService.EditarAprendizAsync(idAprendiz, idFicha, estado, idUsuarioCambio);
            return resultado
                ? Ok(new MensajeEstadoDTO() {esExitoso = true, Mensaje = "Estado actualizado con exito"})
                : BadRequest(new {esExitoso = false, Mensaje = "Estado inválido o aprendiz no encontrado"});
        }


        [HttpGet("obtener-aprendices-por-ficha")]
        public async Task<IActionResult> GetAprendicesPorFichaAsync([FromBody] string numeroFicha)
        {

            return Ok(await aprendizAppService.ObtenerAprendicesPorFichaAsync(numeroFicha));

        }

        [HttpGet("obtener-detalles-aprendiz")]
        public async Task<IActionResult> GetDetallesAprendicesAsync([FromBody] int idAprendiz)
        {

            return Ok(await aprendizAppService.ObtenerDetallesAprendizAsync(idAprendiz));


        }

        [HttpGet("obtener-resumen-aprendices")]
        public async Task<IActionResult> GetResumenAprendicesAsync(
        [FromQuery] string? ficha,
        [FromQuery] string? documento,
        [FromQuery] string? nombre)
        {
            return Ok(await aprendizAppService.ObtenerResumenAprendicesAsync(ficha, documento, nombre));
        }

        [HttpGet("filtrar-aprendices")]
        public async Task<IActionResult> FiltrarAprendicesAsync(
            [FromQuery] string? numeroFicha,
            [FromQuery] string? numeroDocumento,
            [FromQuery] string? nombreAprendiz)
        {
            return Ok(await aprendizAppService.FiltrarAprendizAsync(numeroFicha, numeroDocumento, nombreAprendiz));
        }

    }
}
