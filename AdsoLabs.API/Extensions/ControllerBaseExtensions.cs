using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Extensions;

public static class ControllerBaseExtensions
{
    public static int ObtenerIdUsuarioAutenticado(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("El token no trae el claim NameIdentifier.");

        return int.Parse(claim.Value);
    }
}
