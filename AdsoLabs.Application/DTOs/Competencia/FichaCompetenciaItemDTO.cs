namespace AdsoLabs.Application.DTOs.Competencia;

public class FichaCompetenciaItemDTO
{
    public string  NumeroFicha        { get; set; } = string.Empty;
    public string  Nombre             { get; set; } = string.Empty;
    /// <summary>Estado de la Ficha: Activa | Finalizada | Suspendida</summary>
    public string  Estado             { get; set; } = string.Empty;
    /// <summary>Estado de la competencia dentro de esta ficha: Pendiente | Programada | En Curso | Vista</summary>
    public string  EstadoCompetencia  { get; set; } = string.Empty;
    public string? Descripcion        { get; set; }
    public string? NombreInstructor   { get; set; }
}
