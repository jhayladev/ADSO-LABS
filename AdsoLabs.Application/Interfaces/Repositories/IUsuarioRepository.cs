using AdsoLabs.Core.Entities;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorIdPersonaAsync(int idPersona);
    Task<bool> ExisteCorreoAsync(string correo);
    Task AgregarAsync(Usuario usuario);
    Task ActualizarLoginAsync(int idUsuario, int intentosFallidos, DateTime? bloqueadoHasta, DateTime? fechaInicioSesion);
    Task CambiarPasswordAsync(int idUsuario, byte[] hash, byte[] salt, bool primerLogin, DateTime? consentimientoFecha = null);
    Task ActivarAsync(int idUsuario);
    Task ActualizarCorreoAsync(int idUsuario, string correo);
    Task<bool> ActualizarContactoAsync(int idUsuario, string correo, string? telefono);
    Task<int?> ObtenerIdInstructorPorUsuarioAsync(int idUsuario);
}
