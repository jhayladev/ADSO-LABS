namespace AdsoLabs.Application.DTOs.Fichas;

public class AprendizFichaDTO
{
    public int    IdAprendiz      { get; set; }
    public string Nombres         { get; set; } = "";
    public string NumeroDocumento { get; set; } = "";
    public string EstadoFicha     { get; set; } = "";   // En Formacion | Trasladado | Cancelado...
    public bool   AlDia           { get; set; }         // true = todos los resultados Aprobados
    public bool   TienePatrocinio { get; set; }
}
