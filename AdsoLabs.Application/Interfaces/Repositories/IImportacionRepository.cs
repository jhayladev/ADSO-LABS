using AdsoLabs.Application.DTOs.Importacion;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IImportacionRepository
{
    Task<List<ImportacionResumenDTO>> ObtenerHistorialAsync();
    Task<List<ImportacionErrorDTO>>   ObtenerErroresAsync(int idImportacion);
}
