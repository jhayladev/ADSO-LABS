using AdsoLabs.Application.DTOs.Aprendiz;
using AdsoLabs.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdsoLabs.Web.Pages
{
    public class ActivacionHandlerModel : PageModel
    {
        private readonly ActivacionCuentaApiService _activacionService;

        public ActivacionHandlerModel(ActivacionCuentaApiService activacionService)
        {
            _activacionService = activacionService;
        }

        public async Task<IActionResult> OnGetAsync(ActivarUsuarioTokenDTO peticion)
        {
            var resultado = await _activacionService.ValidarTokenActivacionAsync(peticion);

            if (!resultado.esExitoso)
                return Redirect("/activar-cuenta?error=token-invalido");

            // Activa la cuenta en el backend
            await _activacionService.ActivarUsuarioAsync(peticion);

            // Redirige al login � el aprendiz inicia sesi�n y PrimerLogin lo manda a passwordchange
            // Redirige al login con indicador de activación exitosa
            // El banner en el login explica que deben ingresar con el documento como contraseña
            return Redirect("/?activado=true");
        }
    }
}
