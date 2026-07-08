using AdsoLabs.Web.Services.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace AdsoLabs.Web.Pages
{
    public class RecuperacionHandlerModel : PageModel
    {

        private readonly AuthApiService _authService;

        public RecuperacionHandlerModel(AuthApiService authService)
        {

            _authService = authService;

        }

        // RecuperacionHandler.cshtml.cs
        public async Task<IActionResult> OnGetAsync(string token)
        {
            var valido = await _authService.ValidarTokenAsync(token);
            if (!valido)
                return Redirect("/");

            // Crea una sesi�n temporal solo para cambiar contrase�a
            var claims = new List<Claim>
            {
                new Claim("CambioPassword", "true"),
                new Claim("TokenRecuperacion", token)
            };
            var identity = new ClaimsIdentity(claims, "Cookies");
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return Redirect("/passwordchange");
        }
    }
}
