using AdsoLabs.Core.Entities;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IPersonaRepository
{
    Task<Persona?> ObtenerPorNumeroDocumentoAsync(string numeroDocumento);
    Task AgregarAsync(Persona persona);
}
