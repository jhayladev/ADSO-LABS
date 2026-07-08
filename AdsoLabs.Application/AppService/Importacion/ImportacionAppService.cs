using AdsoLabs.Application.DTOs.Importacion;
using AdsoLabs.Application.Interfaces.Repositories;

namespace AdsoLabs.Application.AppService.Importacion;

public class ImportacionAppService(IImportacionRepository repo)
{
    public Task<List<ImportacionResumenDTO>> ObtenerHistorialAsync()
        => repo.ObtenerHistorialAsync();

    public Task<List<ImportacionErrorDTO>> ObtenerErroresAsync(int idImportacion)
        => repo.ObtenerErroresAsync(idImportacion);
}
