namespace AdsoLabs.Application.DTOs.Instructor;

public class InstructorAgregarDTO
{
    //-- Datos Personales -----------
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; } = string.Empty;
    public DateTime? FechaNacimiento { get; set; }
    public string? Direccion { get; set; } = string.Empty;
    public string? Municipio { get; set; } = string.Empty;

    //-- Datos Usuario ---------------
    public string Correo { get; set; } = string.Empty;

    //-- Datos Instructor ------------
    public string? NumeroContrato { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string? TipoVinculacion { get; set; } = string.Empty;
    public DateOnly? FechaVinculacion { get; set; }
    public bool Activo { get; set; }
}
