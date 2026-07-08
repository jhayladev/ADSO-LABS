using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Infrastructure.Services;

/// <summary>
/// Almacena archivos en el sistema de archivos local del servidor.
/// basePath se resuelve en Program.cs como Path.Combine(ContentRootPath, "storage").
/// </summary>
public class LocalArchivoStorageService(string basePath) : IArchivoStorageService
{
    public async Task<ArchivoStorageResultado> GuardarAsync(
        Stream contenido,
        string nombreOriginal,
        string subcarpeta,
        CancellationToken ct = default)
    {
        var carpeta = Path.Combine(basePath, subcarpeta);
        Directory.CreateDirectory(carpeta);

        var extension      = Path.GetExtension(nombreOriginal);
        var nombreGuardado = $"{Guid.NewGuid()}{extension}";
        var rutaAbsoluta   = Path.Combine(carpeta, nombreGuardado);
        var rutaRelativa   = Path.Combine(subcarpeta, nombreGuardado).Replace('\\', '/');

        await using var fs = new FileStream(rutaAbsoluta, FileMode.Create, FileAccess.Write, FileShare.None);
        await contenido.CopyToAsync(fs, ct);

        return new ArchivoStorageResultado(nombreGuardado, rutaRelativa, fs.Length);
    }

    public Task EliminarAsync(string rutaStorage, CancellationToken ct = default)
    {
        var rutaAbsoluta = Path.Combine(basePath, rutaStorage.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(rutaAbsoluta))
            File.Delete(rutaAbsoluta);
        return Task.CompletedTask;
    }

    public Task<Stream?> ObtenerStreamAsync(string rutaStorage, CancellationToken ct = default)
    {
        var rutaAbsoluta = Path.Combine(basePath, rutaStorage.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(rutaAbsoluta)) return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(rutaAbsoluta, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public string ObtenerMimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf"  => "application/pdf",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".doc"  => "application/msword",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".xls"  => "application/vnd.ms-excel",
        ".png"  => "image/png",
        ".jpg"  => "image/jpeg",
        ".jpeg" => "image/jpeg",
        ".zip"  => "application/zip",
        _       => "application/octet-stream"
    };
}
