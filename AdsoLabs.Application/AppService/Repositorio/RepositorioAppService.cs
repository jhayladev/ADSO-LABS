using AdsoLabs.Application.Commands.Repositorio;
using AdsoLabs.Application.DTOs.Repositorio;
using AdsoLabs.Application.Interfaces.Queries;
using AdsoLabs.Application.Interfaces.Repositories;
using AdsoLabs.Application.Interfaces.Services;

namespace AdsoLabs.Application.AppService.Repositorio;

public class RepositorioAppService(
    IArchivoStorageService storage,
    IRepositorioRepository repo,
    IRepositorioQueryService query)
{
    // ─────────────────────────────────────────────────────────────
    // SUBIDAS
    // ─────────────────────────────────────────────────────────────

    public async Task<ResultadoOperacionRepositorio> SubirGuiaAsync(
        Stream archivoStream, string nombreOriginal,
        string titulo, string? descripcion, int idFichaCompetencia,
        int idUsuario, CancellationToken ct = default)
    {
        var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
        if (idInstructor is null)
            return ResultadoOperacionRepositorio.Fallo("El usuario no tiene perfil de instructor.");

        var storage = await GuardarArchivoAsync(archivoStream, nombreOriginal, "guias", "Guia de Aprendizaje", idUsuario, ct);
        var idGuia = await repo.AgregarGuiaAsync(
            new AgregarGuiaCommand(storage.IdArchivo, idFichaCompetencia, idInstructor.Value, titulo, descripcion), ct);

        return ResultadoOperacionRepositorio.Ok(idGuia);
    }

    public async Task<ResultadoOperacionRepositorio> SubirInstrumentoAsync(
        Stream archivoStream, string nombreOriginal,
        string nombre, string? descripcion, int idCompetencia,
        int idUsuario, CancellationToken ct = default)
    {
        var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
        if (idInstructor is null)
            return ResultadoOperacionRepositorio.Fallo("El usuario no tiene perfil de instructor.");

        var storage = await GuardarArchivoAsync(archivoStream, nombreOriginal, "instrumentos", "Instrumento de Evaluacion", idUsuario, ct);
        var id = await repo.AgregarInstrumentoAsync(
            new AgregarInstrumentoCommand(storage.IdArchivo, idCompetencia, idInstructor.Value, nombre, descripcion), ct);

        return ResultadoOperacionRepositorio.Ok(id);
    }

    public async Task<ResultadoOperacionRepositorio> SubirPlaneacionAsync(
        Stream archivoStream, string nombreOriginal,
        string? descripcion, int idFichaCompetencia,
        int idUsuario, CancellationToken ct = default)
    {
        var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
        if (idInstructor is null)
            return ResultadoOperacionRepositorio.Fallo("El usuario no tiene perfil de instructor.");

        var storage = await GuardarArchivoAsync(archivoStream, nombreOriginal, "planeaciones", "Planeacion Pedagogica", idUsuario, ct);
        var id = await repo.AgregarPlaneacionAsync(
            new AgregarPlaneacionCommand(storage.IdArchivo, idFichaCompetencia, idInstructor.Value, descripcion), ct);

        return ResultadoOperacionRepositorio.Ok(id);
    }

    public async Task<ResultadoOperacionRepositorio> SubirProyectoAsync(
        Stream archivoStream, string nombreOriginal,
        string titulo, string? descripcion, int idFicha,
        int idUsuario, CancellationToken ct = default)
    {
        var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
        if (idInstructor is null)
            return ResultadoOperacionRepositorio.Fallo("El usuario no tiene perfil de instructor.");

        var storage = await GuardarArchivoAsync(archivoStream, nombreOriginal, "proyectos", "Proyecto Formativo", idUsuario, ct);
        var id = await repo.AgregarProyectoAsync(
            new AgregarProyectoCommand(storage.IdArchivo, idFicha, idInstructor.Value, titulo, descripcion), ct);

        return ResultadoOperacionRepositorio.Ok(id);
    }

    public async Task<ResultadoOperacionRepositorio> SubirDesarrolloAsync(
        Stream archivoStream, string nombreOriginal,
        string? descripcion, int idCompetencia,
        int idUsuario, CancellationToken ct = default)
    {
        var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
        if (idInstructor is null)
            return ResultadoOperacionRepositorio.Fallo("El usuario no tiene perfil de instructor.");

        var storage = await GuardarArchivoAsync(archivoStream, nombreOriginal, "desarrollo-curricular", "Desarrollo Curricular", idUsuario, ct);
        var id = await repo.AgregarDesarrolloAsync(
            new AgregarDesarrolloCommand(storage.IdArchivo, idCompetencia, idInstructor.Value, descripcion), ct);

        return ResultadoOperacionRepositorio.Ok(id);
    }

    public async Task<ResultadoOperacionRepositorio> VincularDocumentoPlanAsync(
        Stream archivoStream, string nombreOriginal,
        int idPlan, int idUsuario, CancellationToken ct = default)
    {
        var storage = await GuardarArchivoAsync(archivoStream, nombreOriginal, "planes-concertados", "Plan Concertado", idUsuario, ct);
        await repo.VincularArchivoAPlanAsync(idPlan, storage.IdArchivo, ct);
        return ResultadoOperacionRepositorio.Ok(idPlan);
    }

    // ─────────────────────────────────────────────────────────────
    // DESCARGA
    // ─────────────────────────────────────────────────────────────

    public async Task<(Stream Stream, string MimeType, string NombreOriginal)?> DescargarArchivoAsync(
        int idArchivo, int idUsuario, string ip, CancellationToken ct = default)
    {
        var info = await query.ObtenerArchivoAsync(idArchivo, ct);
        if (info is null) return null;

        var stream = await storage.ObtenerStreamAsync(info.RutaStorage, ct);
        if (stream is null) return null;

        var mimeType = info.MimeType ?? storage.ObtenerMimeType(info.ExtensionArchivo ?? ".bin");

        await repo.RegistrarDescargaAsync(idUsuario, idArchivo, info.NombreOriginal, ip, ct);

        return (stream, mimeType, info.NombreOriginal);
    }

    // ─────────────────────────────────────────────────────────────
    // ELIMINACIÓN
    // ─────────────────────────────────────────────────────────────

    public async Task<ResultadoOperacionRepositorio> EliminarDocumentoAsync(
        string tipoDoc, int idDocumento, int idUsuario, string rol, CancellationToken ct = default)
    {
        var info = await repo.ObtenerInfoDocumentoAsync(tipoDoc, idDocumento, ct);
        if (info is null)
            return ResultadoOperacionRepositorio.Fallo("Documento no encontrado.");

        // Instructores solo pueden eliminar sus propios documentos
        if (rol == "Instructor")
        {
            var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
            if (idInstructor != info.Value.IdInstructor)
                return ResultadoOperacionRepositorio.Fallo("Sin permiso para eliminar este documento.");
        }

        await repo.EliminarDocumentoAsync(tipoDoc, idDocumento, ct);

        // Eliminar archivo físico (no lanzar si falla — el registro ya se marcó inactivo)
        try { await storage.EliminarAsync(info.Value.RutaStorage, ct); }
        catch { /* log pendiente */ }

        return ResultadoOperacionRepositorio.Ok(idDocumento);
    }

    // ─────────────────────────────────────────────────────────────
    // CAMBIO DE ESTADO
    // ─────────────────────────────────────────────────────────────

    public async Task<ResultadoOperacionRepositorio> CambiarEstadoAsync(
        string tipoDoc, int idDocumento, string nuevoEstado, int idUsuario, string rol, CancellationToken ct = default)
    {
        var estadosValidos = new[] { "Borrador", "Publicado", "Cerrado" };
        if (!estadosValidos.Contains(nuevoEstado))
            return ResultadoOperacionRepositorio.Fallo("Estado no válido.");

        var info = await repo.ObtenerInfoDocumentoAsync(tipoDoc, idDocumento, ct);
        if (info is null)
            return ResultadoOperacionRepositorio.Fallo("Documento no encontrado.");

        if (rol == "Instructor")
        {
            var idInstructor = await ResolverInstructorAsync(idUsuario, ct);
            if (idInstructor != info.Value.IdInstructor)
                return ResultadoOperacionRepositorio.Fallo("Sin permiso para modificar este documento.");
        }

        await repo.CambiarEstadoAsync(tipoDoc, idDocumento, nuevoEstado, ct);
        return ResultadoOperacionRepositorio.Ok(idDocumento);
    }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────

    private async Task<int?> ResolverInstructorAsync(int idUsuario, CancellationToken ct)
        => await query.ObtenerIdInstructorPorUsuarioAsync(idUsuario, ct);

    private async Task<(int IdArchivo, string RutaStorage)> GuardarArchivoAsync(
        Stream stream, string nombreOriginal, string subcarpeta, string tipoNombre, int idUsuario, CancellationToken ct)
    {
        var resultado = await storage.GuardarAsync(stream, nombreOriginal, subcarpeta, ct);
        var extension = Path.GetExtension(nombreOriginal);
        var mimeType = storage.ObtenerMimeType(extension);

        var idArchivo = await repo.AgregarArchivoAsync(new AgregarArchivoCommand(
            nombreOriginal,
            resultado.NombreAlmacenado,
            resultado.RutaStorage,
            extension,
            mimeType,
            tipoNombre,
            idUsuario,
            resultado.TamanioBytes), ct);

        return (idArchivo, resultado.RutaStorage);
    }
}
