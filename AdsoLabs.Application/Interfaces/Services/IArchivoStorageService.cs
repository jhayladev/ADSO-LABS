namespace AdsoLabs.Application.Interfaces.Services;

public record ArchivoStorageResultado(
    string NombreAlmacenado,
    string RutaStorage,
    long TamanioBytes);

public interface IArchivoStorageService
{
    /// <summary>Guarda el archivo en el almacenamiento y retorna los metadatos del resultado.</summary>
    Task<ArchivoStorageResultado> GuardarAsync(
        Stream contenido,
        string nombreOriginal,
        string subcarpeta,
        CancellationToken ct = default);

    /// <summary>Elimina el archivo físico del almacenamiento.</summary>
    Task EliminarAsync(string rutaStorage, CancellationToken ct = default);

    /// <summary>Retorna un stream del archivo almacenado, o null si no existe.</summary>
    Task<Stream?> ObtenerStreamAsync(string rutaStorage, CancellationToken ct = default);

    /// <summary>Retorna el MIME type para la extensión dada (con punto, ej. ".pdf").</summary>
    string ObtenerMimeType(string extension);
}
