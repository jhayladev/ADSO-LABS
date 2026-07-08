namespace AdsoLabs.Infrastructure.Models;

public class FichaCompetencia
{
    public int IdFichaCompetencia { get; set; }
    public int IdFicha { get; set; }
    public int IdCompetencia { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int? TotalHoras { get; set; }
    public int HorasEjecutadas { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente | En Curso | Programada | Vista

    // Navegación
    public Ficha Ficha { get; set; } = null!;
    public Competencia Competencia { get; set; } = null!;
    public ICollection<Sesion> Sesiones { get; set; } = [];
    public ICollection<GuiaAprendizaje> GuiasAprendizaje { get; set; } = [];
    public ICollection<PlaneacionPedagogica> PlaneacionesPedagogicas { get; set; } = [];
    public ICollection<FichaCompetenciaResultado> ResultadosProgramados { get; set; } = [];
}
