using AdsoLabs.Application.Commands.Repositorio;

namespace AdsoLabs.Application.Interfaces.Repositories;

public interface IRepositorioRepository
{
    Task<int> AgregarArchivoAsync(AgregarArchivoCommand cmd, CancellationToken ct = default);
    Task<int> AgregarGuiaAsync(AgregarGuiaCommand cmd, CancellationToken ct = default);
    Task<int> AgregarInstrumentoAsync(AgregarInstrumentoCommand cmd, CancellationToken ct = default);
    Task<int> AgregarPlaneacionAsync(AgregarPlaneacionCommand cmd, CancellationToken ct = default);
    Task<int> AgregarProyectoAsync(AgregarProyectoCommand cmd, CancellationToken ct = default);
    Task<int> AgregarDesarrolloAsync(AgregarDesarrolloCommand cmd, CancellationToken ct = default);

    /// <summary>Actualiza IdDocumento del PlanConcertado con el archivo recién creado.</summary>
    Task VincularArchivoAPlanAsync(int idPlan, int idArchivo, CancellationToken ct = default);

    /// <summary>Soft-delete: marca Archivo.Activo = false y el documento relacionado.</summary>
    Task EliminarDocumentoAsync(string tipoDoc, int idDocumento, CancellationToken ct = default);

    Task CambiarEstadoAsync(string tipoDoc, int idDocumento, string nuevoEstado, CancellationToken ct = default);

    /// <summary>Retorna (IdInstructor, IdArchivo, RutaStorage) del documento, o null si no existe.</summary>
    Task<(int IdInstructor, int IdArchivo, string RutaStorage)?> ObtenerInfoDocumentoAsync(
        string tipoDoc, int idDocumento, CancellationToken ct = default);

    Task RegistrarDescargaAsync(int idUsuario, int idArchivo, string nombreArchivo, string ip,
        CancellationToken ct = default);
}
