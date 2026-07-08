namespace AdsoLabs.Infrastructure.Models;

public class PlanConcertado
{
    public int IdPlan { get; set; }
    public int IdCompetencia { get; set; }
    public int? IdInstructor { get; set; }
    public int? IdDocumento { get; set; }   // null cuando el plan no tiene documento adjunto (ej. seed)
    public string DescripcionGeneral { get; set; } =
        "Plan de evaluacion concertado con los aprendices de acuerdo con los resultados de aprendizaje de la competencia.";
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
    public string Estado { get; set; } = "Borrador"; // Borrador | Publicado | Cerrado
    public DateTime? FechaCierre { get; set; }

    // Navegación
    public Competencia Competencia { get; set; } = null!;
    public InstructorPerfil? Instructor { get; set; } = null!;
    public Archivo Documento { get; set; } = null!;
    public ICollection<ResultadoAprendizaje> ResultadosAprendizaje { get; set; } = [];
}

public class ResultadoAprendizaje
{
    public int IdResultado { get; set; }
    public int IdPlan { get; set; }
    public string Codigo { get; set; } = null!;   // código oficial SENA (ej: 593109)
    public string Nombre { get; set; } = null!;

    // Navegación
    public PlanConcertado Plan { get; set; } = null!;
    public ICollection<JuicioResultado> JuiciosResultado { get; set; } = [];
    public ICollection<FichaCompetenciaResultado> ProgramacionesFicha { get; set; } = [];
}

public class JuicioResultado
{
    public int IdJuicio { get; set; }
    public int IdResultado { get; set; }
    public int IdAprendiz { get; set; }
    public string Juicio { get; set; } = "Por Evaluar"; // Aprobado | No Aprobado | Por Evaluar
    public string? RegistradoPor { get; set; }          // nombre del funcionario en Sofia Plus
    public DateTime? FechaJuicio { get; set; }
    public DateTime FechaRegistro { get; set; }

    // Navegación
    public ResultadoAprendizaje Resultado { get; set; } = null!;
    public AprendizPerfil Aprendiz { get; set; } = null!;
}
