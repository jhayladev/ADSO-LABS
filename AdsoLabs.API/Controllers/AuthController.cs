using AdsoLabs.Application.AppService.Auth;
using AdsoLabs.Application.Common.Validators;
using AdsoLabs.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers
{


    [ApiController]
    [Route("api/")]
    [AllowAnonymous]
    public class AuthController(AuthAppService authAppService) : ControllerBase
    {

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginPeticionDto peticion)
        {
            var resultado = await authAppService.ValidarCredencialesAsync(peticion);

            if (!resultado.esExitoso)
                return Unauthorized(resultado);

            return Ok(resultado);

        }

        [HttpPost("cambiar-password-primer-login")]
        public async Task<IActionResult> CambiarPasswordPrimerLogin([FromBody] CambiarPasswordPrimerLoginDto peticion)
        {
            if (!peticion.ConsentimientoAceptado)
                return BadRequest(new CambiarPasswordResultadoDto() { esExitoso = false, Mensaje = "Debes aceptar el Aviso de Tratamiento de Datos Personales para continuar." });

            var (esValido, error) = PasswordPolicyValidator.Validar(peticion.NuevaPassword);
            if (!esValido)
                return BadRequest(new CambiarPasswordResultadoDto() { esExitoso = false, Mensaje = error! });

            var resultado = await authAppService.CambiarPasswordPrimerLoginAsync(peticion.IdUsuario, peticion.NuevaPassword, peticion.ConsentimientoAceptado);

            if (!resultado)
                return BadRequest(new CambiarPasswordResultadoDto() { esExitoso = false, Mensaje = "No se pudo cambiar la contraseña." });

            return Ok(new CambiarPasswordResultadoDto() { esExitoso = true, Mensaje = "Contraseña establecida correctamente." });
        }
    }
}
