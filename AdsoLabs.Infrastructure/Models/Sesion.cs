namespace AdsoLabs.Infrastructure.Models;

public class Sesion
{
    public int IdSesion { get; set; }
    public int IdFichaCompetencia { get; set; }
    public int IdFichaCompetenciaResultado { get; set; }   // resultado específico al que pertenece la sesión
    public int IdInstructor { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string Estado { get; set; } = "Abierta"; // Abierta | Finalizada | Cancelada

    // Navegación
    public FichaCompetencia         FichaCompetencia          { get; set; } = null!;
    public FichaCompetenciaResultado FichaCompetenciaResultado { get; set; } = null!;
    public InstructorPerfil          Instructor                { get; set; } = null!;
    public ICollection<Asistencia>   Asistencias               { get; set; } = [];
}
