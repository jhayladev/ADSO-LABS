namespace AdsoLabs.Application.DTOs.Fichas;

public class FichaDTO
{
    public int IdFicha { get; set; }
    public string NumeroFicha { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string Estado { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
}
