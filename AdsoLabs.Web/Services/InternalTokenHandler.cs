using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AdsoLabs.Web.Services;

/// <summary>
/// Firma un JWT interno de corta duracion a partir de la cookie ya validada
/// del usuario actual y lo adjunta como Authorization: Bearer en cada
/// llamada saliente hacia AdsoLabs.API. El navegador nunca ve este token;
/// solo viaja servidor-a-servidor.
/// </summary>
public class InternalTokenHandler(IHttpContextAccessor httpContextAccessor, IConfiguration config) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var usuario = httpContextAccessor.HttpContext?.User;
        if (usuario?.Identity?.IsAuthenticated == true)
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", MintarToken(usuario));
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private string MintarToken(ClaimsPrincipal usuario)
    {
        var key = config["InternalAuth:Key"]
            ?? throw new InvalidOperationException("Falta configurar InternalAuth:Key (ver appsettings.Development.json).");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""),
            new Claim(ClaimTypes.Role, usuario.FindFirst(ClaimTypes.Role)?.Value ?? ""),
            new Claim("IdInstructor", usuario.FindFirst("IdInstructor")?.Value ?? "0")
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["InternalAuth:Issuer"],
            audience: config["InternalAuth:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(2),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
