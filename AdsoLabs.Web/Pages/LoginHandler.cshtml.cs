using AdsoLabs.Application.DTOs.Auth;
using AdsoLabs.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace AdsoLabs.Web.Pages;

[IgnoreAntiforgeryToken]
public class LoginHandlerModel : PageModel
{

    private readonly SesionService _sesionService;

    public LoginHandlerModel(SesionService sesionService)
    {
        _sesionService = sesionService;
    }

    public async Task<IActionResult> OnPostAsync(UsuarioSesionDTO usuario, int primerLogin, string returnUrl)
    {

        usuario.PrimerLogin = primerLogin == 1;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombres),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.NombreRol),
            new Claim("PrimerLogin", usuario.PrimerLogin.ToString()),
            new Claim("IdInstructor", usuario.IdInstructor.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (usuario.PrimerLogin)
            return Redirect("/passwordchange");

        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(Uri.UnescapeDataString(returnUrl));

        if (usuario.NombreRol == "Administrador" || usuario.NombreRol == "Instructor")
            return Redirect("/home");

        if (_sesionService.EsPrimerLogin)
        {
            return Redirect("/passwordchange");
        }

        if (_sesionService.EsAdministrador || _sesionService.EsInstructor)
        {
            return Redirect("/home");
        }

        return Redirect("/perfil");

    }

}