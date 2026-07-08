namespace AdsoLabs.Infrastructure.Models;

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

    // Navegación
    public TipoDocumento TipoDocumento { get; set; } = null!;
    public Usuario? Usuario { get; set; }           // nullable: aprendiz puede existir sin usuario
    public AprendizPerfil? AprendizPerfil { get; set; } // directo desde Persona
}
