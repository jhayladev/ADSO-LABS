// AdsoLabs.Web/Services/SesionService.cs
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AdsoLabs.Web.Services;

public class SesionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SesionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Usuario => 
        _httpContextAccessor.HttpContext?.User;
    public bool EstaAutenticado => 
        Usuario?.Identity?.IsAuthenticated ?? false;
    public int IdUsuario
    {
        get
        {
            var value = Usuario?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }
    public string Nombres => 
        Usuario?.FindFirst(ClaimTypes.Name)?.Value ?? "";
    public string Correo => 
        Usuario?.FindFirst(ClaimTypes.Email)?.Value ?? "";
    public string Rol => 
        Usuario?.FindFirst(ClaimTypes.Role)?.Value ?? "";
    public string Ficha { get; set; }
    /// <summary>
    /// Competencia pre-seleccionada al navegar desde Fichas → AsignacionCompetencias.
    /// Se resetea a 0 inmediatamente después de ser leída.
    /// </summary>
    public int IdCompetenciaPreseleccionada { get; set; } = 0;
    public bool EsAdministrador => 
        Rol == "Administrador";
    public bool EsInstructor => 
        Rol == "Instructor";
    public bool EsAprendiz => 
        Rol == "Aprendiz";
    public int IdInstructor
    {
        get
        {
            var value = Usuario?.FindFirst("IdInstructor")?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }
    }
    public bool EsPrimerLogin =>
        bool.Parse(Usuario?.FindFirst("PrimerLogin")?.Value ?? "false");
    public bool PuedeCambiarPassword =>
    bool.Parse(Usuario?.FindFirst("CambioPassword")?.Value ?? "false");
    public string TokenRecuperacion =>
        Usuario?.FindFirst("TokenRecuperacion")?.Value ?? "";

}
