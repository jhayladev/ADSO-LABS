namespace AdsoLabs.Application.DTOs.Aprendiz;

public class AprendizDetalleDTO
{
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public List<string> Ficha { get; set; } = new List<string>();
    public string? Telefono { get; set; }
    public DateOnly? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Municipio { get; set; }
    public byte? Estrato { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? CondicionEspecial { get; set; }
    public string? TipoPoblacion { get; set; }
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public string IndicadorAsistencia { get; set; } = "Al día";
    public decimal PorcentajeAsistencia { get; set; } = 100m;
}
