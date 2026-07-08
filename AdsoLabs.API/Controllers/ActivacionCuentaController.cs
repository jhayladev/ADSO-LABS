using AdsoLabs.Application.AppService.ActivacionCuenta;
using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.Api.Controllers
{
    [ApiController]
    [Route("api/")]
    [AllowAnonymous]
    public class ActivacionCuentaController(ActivacionCuentaAppService activacionAppService) : ControllerBase
    {

        /// <summary>
        /// Paso 1: El aprendiz ingresa su número de documento y correo.
        /// El sistema crea el usuario y envía el token al correo.
        /// </summary>
        [HttpPost("solicitar")]
        public async Task<IActionResult> Solicitar([FromBody] ActivarUsuarioSolicitudDTO peticion)
        {
            var resultado = await activacionAppService.EnviarTokenActivacionAsync(
                peticion.NumeroDocumento, peticion.Correo);

            return resultado switch
            {
                ResultadoActivacion.Exitoso => Ok(new ActivarUsuarioResultadoDTO { esExitoso = true, Mensaje = "Se envió un enlace de activación a tu correo." }),
                ResultadoActivacion.DocumentoNoEncontrado => NotFound(new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "No se encontró un aprendiz con ese número de documento." }),
                ResultadoActivacion.YaActivado => Conflict(new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "Esta cuenta ya fue activada. Usa 'Olvidé mi contraseña' si necesitas acceso." }),
                ResultadoActivacion.CorreoEnUso => Conflict(new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "Ese correo ya está registrado en el sistema." }),
                ResultadoActivacion.EnCooldown => StatusCode(429, new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "Ya se envió un enlace recientemente. Espera 5 minutos antes de intentar de nuevo." }),
                ResultadoActivacion.AprendizInactivo => StatusCode(403, new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "No es posible activar esta cuenta. El aprendiz se encuentra inactivo en el sistema." }),
                _ => StatusCode(500, new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "Error inesperado." })
            };
        }

        /// <summary>
        /// Paso 2 (opcional): Validar que el token sigue vigente antes de
        /// mostrar el formulario de contraseña en el frontend.
        /// </summary>
        [HttpGet("validar")]
        public async Task<IActionResult> Validar([FromQuery] string token)
        {
            bool valido = await activacionAppService.ValidarTokenAsync(token);
            return valido
                ? Ok(new ActivarUsuarioResultadoDTO { esExitoso = true, Mensaje = "El Token es valido" })
                : BadRequest(new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "El enlace es inválido o ya expiró." });
        }

        /// <summary>
        /// Paso 3: El aprendiz establece su contraseña y activa la cuenta.
        /// </summary>
        [HttpPost("activar")]
        public async Task<IActionResult> Activar([FromBody] ActivarUsuarioTokenDTO peticion)
        {
            var resultado = await activacionAppService.ActivarCuentaAsync(peticion.Token);

            return resultado switch
            {
                ResultadoActivacion.Exitoso => Ok(new ActivarUsuarioResultadoDTO { esExitoso = true, Mensaje = "Cuenta activada exitosamente. Ya puedes iniciar sesión." }),
                ResultadoActivacion.TokenInvalido => BadRequest(new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "El enlace es inválido o ya expiró." }),
                _ => StatusCode(500, new ActivarUsuarioResultadoDTO { esExitoso = false, Mensaje = "Error inesperado." })
            };
        }
    }

}
