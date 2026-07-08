namespace AdsoLabs.Application.DTOs.Fichas;

public class CompetenciaFichaDTO
{
    public int IdFichaCompetencia { get; set; }
    public int IdCompetencia      { get; set; }
    public string Nombre { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Estado { get; set; } = "";
    /// <summary>Fase del proyecto formativo (Inducción|Análisis|Planeación|Ejecución|Evaluación|Transversal).</summary>
    public string FaseFormativa { get; set; } = "Transversal";
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public List<ResultadoKanbanDTO> ResultadosProgramados { get; set; } = [];
    public int HorasEjecutadas { get; set; }
    public int HorasAsignadas { get; set; }
    public int? TotalHoras { get; set; }
    /// <summary>
    /// true cuando el instructor autenticado tiene al menos un resultado asignado
    /// dentro de esta FichaCompetencia. Siempre false para Administrador.
    /// </summary>
    public bool EsMiCompetencia { get; set; }

    public string FechasProgramadas =>
        FechaInicio.HasValue && FechaFin.HasValue
            ? $"{FechaInicio:dd/MM/yyyy} - {FechaFin:dd/MM/yyyy}"
            : Estado == "Vista" ? "Finalizada" : "Sin programar";
}

/// <summary>Resultado de aprendizaje programado, para visualización en el Kanban de cronograma.</summary>
public sealed record ResultadoKanbanDTO(string Nombre, TimeOnly? HoraInicio, TimeOnly? HoraFin);
