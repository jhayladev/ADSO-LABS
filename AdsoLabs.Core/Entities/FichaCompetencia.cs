namespace AdsoLabs.Core.Entities;

public class FichaCompetencia
{
    public int IdFichaCompetencia { get; set; }
    public int IdFicha { get; set; }
    public int IdCompetencia { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int? HorasEjecutadas { get; set; }
    public string Estado { get; set; } = null!;
}
