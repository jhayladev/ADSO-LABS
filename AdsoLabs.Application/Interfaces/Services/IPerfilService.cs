using AdsoLabs.Application.DTOs.Perfil;

namespace AdsoLabs.Application.Interfaces.Services;

public interface IPerfilService
{
    Task<PerfilAprendizDTO?> ObtenerPerfilAsync(int idUsuario);
    Task<bool> ActualizarContactoAsync(int idUsuario, ActualizarContactoDTO dto);
    Task<CronogramaAprendizDTO?> ObtenerCronogramaAsync(int idUsuario);
}
