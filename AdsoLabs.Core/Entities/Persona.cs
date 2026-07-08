namespace AdsoLabs.Core.Entities;

public class Persona
{
    public int IdPersona { get; set; }
    public int IdTipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string? Telefono { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }
    public DateTime FechaCreacion { get; set; }
}
