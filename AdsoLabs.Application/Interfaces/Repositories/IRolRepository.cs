namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IRolRepository
{
    Task<int> ObtenerIdPorNombreAsync(string nombre);
}
