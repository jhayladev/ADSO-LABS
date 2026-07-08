using AdsoLabs.Application.DTOs.Perfil;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Perfil;

public class PerfilAppService(
    IPerfilService perfilService,
    INotificacionService notificacionService)
{
    public Task<PerfilAprendizDTO?> ObtenerPerfilAsync(int idUsuario)
        => perfilService.ObtenerPerfilAsync(idUsuario);

    public async Task<bool> ActualizarContactoAsync(int idUsuario, ActualizarContactoDTO dto)
    {
        var ok = await perfilService.ActualizarContactoAsync(idUsuario, dto);
        if (ok)
            await notificacionService.NotificarDatosPersonalesActualizadosPorUsuarioAsync(idUsuario);
        return ok;
    }

    public Task<CronogramaAprendizDTO?> ObtenerCronogramaAsync(int idUsuario)
        => perfilService.ObtenerCronogramaAsync(idUsuario);
}
