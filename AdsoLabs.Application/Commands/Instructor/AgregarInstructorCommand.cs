namespace AdsoLabs.Application.Commands.Instructor;

public record AgregarInstructorCommand(
    string TipoDocumento,
    string NumeroDocumento,
    string Nombre,
    string Apellido,
    string? Telefono,
    DateTime? FechaNacimiento,
    string? Direccion,
    string? Municipio,
    string Correo,
    string? NumeroContrato,
    string? Especialidad,
    string? TipoVinculacion,
    DateOnly? FechaVinculacion
);
