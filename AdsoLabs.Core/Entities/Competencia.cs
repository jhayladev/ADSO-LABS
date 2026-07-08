namespace AdsoLabs.Core.Entities;

public class Competencia
{
    public int IdCompetencia { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int HorasAsignadas { get; set; }
    public string Tipo { get; set; } = null!;
    public string Estado { get; set; } = null!;
}
