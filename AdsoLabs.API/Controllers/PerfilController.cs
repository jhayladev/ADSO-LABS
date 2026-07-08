using AdsoLabs.API.Extensions;
using AdsoLabs.Application.AppService.Perfil;
using AdsoLabs.Application.DTOs.Perfil;
using Microsoft.AspNetCore.Mvc;

namespace AdsoLabs.API.Controllers;

[Route("api/perfil")]
[ApiController]
public class PerfilController(PerfilAppService perfilAppService) : ControllerBase
{
    [HttpGet("yo")]
    public async Task<IActionResult> ObtenerPerfil()
    {
        var result = await perfilAppService.ObtenerPerfilAsync(this.ObtenerIdUsuarioAutenticado());
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("editar-contacto")]
    public async Task<IActionResult> EditarContacto([FromBody] ActualizarContactoDTO dto)
    {
        var ok = await perfilAppService.ActualizarContactoAsync(this.ObtenerIdUsuarioAutenticado(), dto);
        return ok ? Ok() : NotFound();
    }

    [HttpGet("cronograma")]
    public async Task<IActionResult> ObtenerCronograma()
    {
        var result = await perfilAppService.ObtenerCronogramaAsync(this.ObtenerIdUsuarioAutenticado());
        return result is null ? NotFound() : Ok(result);
    }
}
