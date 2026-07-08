using AdsoLabs.Application.DTOs.Auth;
using AdsoLabs.Application.AppService.RecuperacionPassword;
using AdsoLabs.Application.Common.Validators;
using AdsoLabs.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers
{

    [ApiController]
    [Route("api/")]
    [AllowAnonymous]
    public class PasswordController(RecuperacionPasswordAppService recuperacionAppService) : ControllerBase
    {

        [HttpPost("enviar-token")]
        public async Task<IActionResult> EnviarToken([FromBody] EnviarTokenDto peticion)
        {
            var resultado = await recuperacionAppService.EnviarTokenAsync(peticion.Correo);

            return resultado switch
            {
                ResultadoEnvioToken.EsPrimerLogin => BadRequest(new EnviarTokenResultadoDto() { Mensaje = "Tu cuenta es nueva, debes iniciar sesión y cambiar tu contraseña desde ahí." }),
                ResultadoEnvioToken.EnCooldown => BadRequest(new EnviarTokenResultadoDto() {  Mensaje = "Ya enviamos un correo recientemente. Espera unos minutos." }),
                _ => Ok(new EnviarTokenResultadoDto() { Mensaje = "Si el correo institucional está registrado, recibirás un enlace de recuperacion." })
            };
        }

        [HttpPost("validar-token")]
        public async Task<IActionResult> ValidarToken([FromQuery] string token)
        {
            var resultado = await recuperacionAppService.ValidarTokenAsync(token);

            if (resultado)
                return Ok();
            else
                return Unauthorized(resultado);

        }

        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto peticion)
        {
            var (esValido, error) = PasswordPolicyValidator.Validar(peticion.NuevaPassword);
            if (!esValido)
                return BadRequest(new CambiarPasswordResultadoDto() { esExitoso = false, Mensaje = error! });

            var resultado = await recuperacionAppService.CambiarPasswordAsync(peticion.Token, peticion.NuevaPassword);

            if (!resultado)
                return BadRequest(new CambiarPasswordResultadoDto() { esExitoso = false, Mensaje = "El enlace es inválido o ya expiró." });

            return Ok(new CambiarPasswordResultadoDto() { esExitoso = true, Mensaje = "Contraseña actualizada correctamente." });
        }

    }

}
