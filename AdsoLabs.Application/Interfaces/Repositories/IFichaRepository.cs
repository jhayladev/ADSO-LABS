using AdsoLabs.Core.Entities;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IFichaRepository
{
    Task<List<Ficha>> ObtenerFichasActivasAsync(int idUsuario = 0);
}