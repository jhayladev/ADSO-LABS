namespace AdsoLabs.Application.Commands.Repositorio;

public record AgregarArchivoCommand(
    string NombreOriginal,
    string NombreAlmacenado,
    string RutaStorage,
    string? Extension,
    string? MimeType,
    string NombreTipoArchivo,
    int IdUsuarioSubio,
    long? TamanioBytes);

public record AgregarGuiaCommand(
    int IdArchivoCreado,
    int IdFichaCompetencia,
    int IdInstructor,
    string Titulo,
    string? Descripcion);

public record AgregarInstrumentoCommand(
    int IdArchivoCreado,
    int IdCompetencia,
    int IdInstructor,
    string Nombre,
    string? Descripcion);

public record AgregarPlaneacionCommand(
    int IdArchivoCreado,
    int IdFichaCompetencia,
    int IdInstructor,
    string? Descripcion);

public record AgregarProyectoCommand(
    int IdArchivoCreado,
    int IdFicha,
    int IdInstructor,
    string Titulo,
    string? Descripcion);

public record AgregarDesarrolloCommand(
    int IdArchivoCreado,
    int IdCompetencia,
    int IdInstructor,
    string? Descripcion);
