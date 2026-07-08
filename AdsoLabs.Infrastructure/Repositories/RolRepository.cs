using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class RolRepository(AdsoDbContext context) : IRolRepository
{
    public async Task<int> ObtenerIdPorNombreAsync(string nombre)
        => await context.Rol
            .Where(r => r.Nombre == nombre)
            .Select(r => r.IdRol)
            .FirstAsync();
}
