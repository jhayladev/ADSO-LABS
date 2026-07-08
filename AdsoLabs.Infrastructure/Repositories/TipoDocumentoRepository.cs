using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdsoLabs.Infrastructure.Repositories;

public class TipoDocumentoRepository(AdsoDbContext context) : ITipoDocumentoRepository
{
    public async Task<Core.Entities.TipoDocumento?> ObtenerPorNombreAsync(string nombre)
    {
        var t = await context.TipoDocumento.FirstOrDefaultAsync(t => t.Nombre == nombre);
        if (t == null) return null;
        return new Core.Entities.TipoDocumento
        {
            IdTipoDocumento = t.IdTipoDocumento,
            Nombre = t.Nombre
        };
    }
}
