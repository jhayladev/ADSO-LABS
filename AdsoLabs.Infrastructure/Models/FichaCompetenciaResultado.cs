namespace AdsoLabs.Infrastructure.Models;

public class FichaCompetenciaResultado
{
    public int IdFichaCompetenciaResultado { get; set; }
    public int IdFichaCompetencia { get; set; }
    public int IdResultado { get; set; }
    public int? IdInstructor { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int? HorasProgramadas { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Programado | Completado

    public FichaCompetencia FichaCompetencia { get; set; } = null!;
    public ResultadoAprendizaje Resultado { get; set; } = null!;
    public InstructorPerfil? Instructor { get; set; }
}