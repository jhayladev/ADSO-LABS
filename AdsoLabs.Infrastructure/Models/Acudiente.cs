namespace AdsoLabs.Infrastructure.Models;

public class Acudiente
{
    public int    IdAcudiente    { get; set; }
    public int    IdAprendiz     { get; set; }
    public string Nombre         { get; set; } = null!;
    public string Parentesco     { get; set; } = null!;
    public string? Correo        { get; set; }
    public string Telefono       { get; set; } = null!;
    public string? Genero        { get; set; }
    public string? TipoDocumento { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Direccion     { get; set; }
    public byte?  Estrato        { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Navegación
    public AprendizPerfil Aprendiz { get; set; } = null!;
}
