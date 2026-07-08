using AdsoLabs.Application.Commands.Aprendiz;
using AdsoLabs.Core.Entities;
using AdsoLabs.Core.Enums;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IAprendizRepository
{
    Task<ResultadoAgregar> AgregarAsync(AgregarAprendizCommand command);
    Task<bool>    EditarEstadoAsync(int idAprendiz, int idFicha, string estado, int? idUsuarioCambio);
    Task<string?> ObtenerEstadoActualAsync(int idAprendiz, int idFicha);
    Task AsignarUsuarioAsync(int idAprendiz, int idUsuario);
    Task<AprendizPerfil?> ObtenerPorIdPersonaAsync(int idPersona);
    Task<string?> ObtenerEstadoMasRecienteAsync(int idAprendiz);
}
