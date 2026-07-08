using AdsoLabs.Core.Entities;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface ITipoDocumentoRepository
{
    Task<TipoDocumento?> ObtenerPorNombreAsync(string nombre);
}
