using AdsoLabs.Application.AppService.Importacion;

namespace AdsoLabs.Application.Interfaces;

public interface IImportadorReporteJuicios
{
    Task<ResultadoImportacion> ImportarAsync(
        string rutaExcel,
        int idUsuarioAdmin,
        int idArchivoRegistro,
        int idInstructorDefault);
}