namespace AdsoLabs.Infrastructure.Models;

public class TipoDocumento
{
    public int IdTipoDocumento { get; set; }
    public string Nombre { get; set; } = null!;

    // Navegación
    public ICollection<Persona> Personas { get; set; } = [];
}
