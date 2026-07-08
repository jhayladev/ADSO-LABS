namespace AdsoLabs.Infrastructure.Models;

public class InstructorPerfil
{
    public int IdInstructor { get; set; }
    public int IdUsuario { get; set; }
    public string? NumeroContrato { get; set; }
    public string? Especialidad { get; set; }
    public string? TipoVinculacion { get; set; }
    public DateOnly? FechaVinculacion { get; set; }
    public bool Activo { get; set; }

    // Navegación
    public Usuario Usuario { get; set; } = null!;
    public ICollection<Sesion> Sesiones { get; set; } = [];
    public ICollection<GuiaAprendizaje> GuiasAprendizaje { get; set; } = [];
    public ICollection<InstrumentoEvaluacion> InstrumentosEvaluacion { get; set; } = [];
    public ICollection<PlaneacionPedagogica> PlaneacionesPedagogicas { get; set; } = [];
    public ICollection<ProyectoFormativo> ProyectosFormativos { get; set; } = [];
    public ICollection<DesarrolloCurricular> DesarrollosCurriculares { get; set; } = [];
    public ICollection<PlanConcertado> PlanesConcertados { get; set; } = [];
    public ICollection<FichaCompetenciaResultado> ResultadosProgramados { get; set; } = [];
}
