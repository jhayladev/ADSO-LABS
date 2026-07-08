using AdsoLabs.API.Extensions;
using AdsoLabs.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[Route("api/notificaciones")]
[ApiController]
public class NotificacionController(INotificacionRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ObtenerBandeja()
        => Ok(await repo.ObtenerPorUsuarioAsync(this.ObtenerIdUsuarioAutenticado()));

    [HttpGet("no-leidas")]
    public async Task<IActionResult> ContarNoLeidas()
        => Ok(await repo.ContarNoLeidasAsync(this.ObtenerIdUsuarioAutenticado()));

    [HttpPut("{id:int}/leida")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        await repo.MarcarLeidaAsync(id, this.ObtenerIdUsuarioAutenticado());
        return NoContent();
    }

    [HttpPut("marcar-todas-leidas")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        await repo.MarcarTodasLeidasAsync(this.ObtenerIdUsuarioAutenticado());
        return NoContent();
    }
}
