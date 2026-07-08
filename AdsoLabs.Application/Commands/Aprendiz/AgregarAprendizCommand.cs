namespace AdsoLabs.Application.Commands.Aprendiz;

public record AgregarAprendizCommand(
    string TipoDocumento,
    string NumeroDocumento,
    string Nombre,
    string Apellido,
    string Telefono,
    DateTime FechaNacimiento,
    string Direccion,
    string Municipio,
    string NumeroFicha,
    byte Estrato,
    string CondicionEspecial,
    string TipoPoblacion,
    string ContactoEmergenciaNombre,
    string ContactoEmergenciaTelefono
);
