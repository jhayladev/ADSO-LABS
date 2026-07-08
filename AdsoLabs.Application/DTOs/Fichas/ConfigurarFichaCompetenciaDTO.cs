namespace AdsoLabs.Application.DTOs.Fichas;

public class ConfigurarFichaCompetenciaDTO
{
    public string   NumeroFicha   { get; set; } = "";
    public int      IdCompetencia { get; set; }
    public DateOnly FechaInicio   { get; set; }
    public DateOnly FechaFin      { get; set; }
    public int      TotalHoras    { get; set; }
}
