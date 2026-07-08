namespace AdsoLabs.Application.DTOs.Informe;

public class AcudienteDTO
{
    public int    IdAcudiente     { get; set; }
    public string Nombre          { get; set; } = string.Empty;
    public string Parentesco      { get; set; } = string.Empty;
    public string? Correo         { get; set; }
    public string Telefono        { get; set; } = string.Empty;
    public string? Genero         { get; set; }
    public string? TipoDocumento  { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Direccion      { get; set; }
    public byte?  Estrato         { get; set; }
    public DateTime FechaRegistro { get; set; }
}
