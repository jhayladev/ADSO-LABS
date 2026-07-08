using AdsoLabs.Application.DTOs.Auth;
using AdsoLabs.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers
{

    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "Administrador")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailServices _email;

        public EmailController(IEmailServices email)
        {
            _email = email;
        }

        [HttpPost("probar")]
        public async Task<IActionResult> Probar([FromBody] ProbarEmailDto peticion)
        {
            try
            {
                await _email.EnviarAsync(
                    peticion.Correo,
                    "Prueba de correo - ADSO Labs",
                    "<h3>Si ves este correo, el servicio de email funciona correctamente.</h3>");

                return Ok(new { mensaje = "Correo enviado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al enviar el correo.", detalle = ex.Message });
            }
        }
    }
}