namespace AdsoLabs.Application.DTOs.Instructor;

public class InstructorDetalleDTO
{
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }
    public string? NumeroContrato { get; set; }
    public string? Especialidad { get; set; }
    public string? TipoVinculacion { get; set; }
    public DateOnly? FechaVinculacion { get; set; }
    public bool Activo { get; set; }
    public int IdUsuario { get; set; }
    public List<FichaResumenInstructorDTO> FichasAsignadas { get; set; } = [];
}
