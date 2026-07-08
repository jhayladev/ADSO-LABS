using AdsoLabs.Application.DTOs;
using AdsoLabs.Application.DTOs.Instructor;
using AdsoLabs.Application.AppService.Instructor;
using AdsoLabs.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdsoLabs.API.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize(Roles = "Administrador,Instructor")]
    public class InstructorController(InstructorAppService instructorAppService) : ControllerBase
    {

        // Un aprendiz con rol de Monitor necesita este directorio para elegir el
        // instructor destinatario de sus informes/asistencias de monitoría
        // (Gestión de Monitorías en Profile.razor). Solo expone nombre/especialidad,
        // no datos sensibles.
        [HttpGet("obtener-instructores")]
        [Authorize(Roles = "Administrador,Instructor,Aprendiz")]
        public async Task<IActionResult> GetInstructoresAsync()
        {
            return Ok(await instructorAppService.ObtenerInstructoresAsync());
        }

        [HttpGet("obtener-resumen-instructores")]
        public async Task<IActionResult> GetResumenInstructoresAsync(
            [FromQuery] string? competencia,
            [FromQuery] string? instructor)
        {
            return Ok(await instructorAppService.ObtenerResumenInstructoresAsync(competencia, instructor));
        }

        [HttpGet("obtener-detalles-instructor")]
        public async Task<IActionResult> GetDetallesInstructorAsync([FromQuery] int idInstructor)
        {
            var resultado = await instructorAppService.ObtenerDetallesInstructorAsync(idInstructor);
            return resultado == null ? NotFound() : Ok(resultado);
        }

        [HttpGet("obtener-instructores-por-competencia")]
        public async Task<IActionResult> GetInstructoresPorCompetenciaAsync([FromQuery] string nombreCompetencia)
        {
            return Ok(await instructorAppService.ObtenerInstructoresPorCompetenciaAsync(nombreCompetencia));
        }

        [HttpGet("filtrar-instructores")]
        public async Task<IActionResult> FiltrarInstructoresAsync(
            [FromQuery] string? competencia,
            [FromQuery] string? instructor)
        {
            return Ok(await instructorAppService.FiltrarInstructorAsync(competencia, instructor));
        }

        [HttpPost("agregar-instructor")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AgregarInstructorAsync([FromBody] InstructorAgregarDTO instructor)
        {
            var resultado = await instructorAppService.AgregarInstructorAsync(instructor);
            return resultado switch
            {
                ResultadoAgregar.Existe => BadRequest(new MensajeEstadoDTO { esExitoso = false, Mensaje = "El instructor ya existe" }),
                ResultadoAgregar.Exitoso => Ok(new MensajeEstadoDTO { esExitoso = true, Mensaje = "Instructor agregado exitosamente" }),
                _ => BadRequest(new MensajeEstadoDTO { esExitoso = false, Mensaje = "Error al agregar el instructor" })
            };
        }

        [HttpPut("actualizar-contacto-instructor")]
        [Authorize(Roles = "Administrador,Instructor")]
        public async Task<IActionResult> ActualizarContactoInstructorAsync([FromBody] ActualizarContactoInstructorDTO dto)
        {
            var idUsuarioLogueado = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Administrador") && idUsuarioLogueado != dto.IdUsuario)
                return Forbid();

            var ok = await instructorAppService.ActualizarContactoAsync(dto);
            return ok
                ? Ok(new MensajeEstadoDTO { esExitoso = true, Mensaje = "Datos de contacto actualizados." })
                : BadRequest(new MensajeEstadoDTO { esExitoso = false, Mensaje = "El correo ya está en uso por otro usuario." });
        }

        [HttpGet("obtener-avisos-instructor")]
        public async Task<IActionResult> GetAvisosInstructorAsync([FromQuery] int idInstructor)
        {
            return Ok(await instructorAppService.ObtenerAvisosInstructorAsync(idInstructor));
        }

        [HttpGet("obtener-horario-instructor")]
        public async Task<IActionResult> GetHorarioInstructorAsync([FromQuery] int idInstructor)
        {
            return Ok(await instructorAppService.ObtenerHorarioInstructorAsync(idInstructor));
        }

        [HttpPut("editar-instructor/{idInstructor}")]
        public async Task<IActionResult> EditarInstructorAsync(int idInstructor)
        {
            var resultado = await instructorAppService.EditarInstructorAsync(idInstructor);
            return resultado
                ? Ok(new MensajeEstadoDTO { esExitoso = true, Mensaje = "Estado actualizado exitosamente" })
                : BadRequest(new MensajeEstadoDTO { esExitoso = false, Mensaje = "Instructor no encontrado" });
        }

    }
}
