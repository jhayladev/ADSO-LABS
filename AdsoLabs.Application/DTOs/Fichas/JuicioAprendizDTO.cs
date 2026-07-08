namespace AdsoLabs.Application.DTOs.Fichas;

/// <summary>
/// Estado del juicio evaluativo de un aprendiz para un Resultado de Aprendizaje específico.
/// Usado en la pantalla de asignación de competencias para mostrar el avance por RA.
/// </summary>
public class JuicioAprendizDTO
{
    public int    IdAprendiz     { get; set; }
    public string NombreCompleto { get; set; } = "";
    /// <summary>"APROBADO" o "POR EVALUAR" (ausencia de JuicioResultado = POR EVALUAR)</summary>
    public string Juicio         { get; set; } = "POR EVALUAR";
}
