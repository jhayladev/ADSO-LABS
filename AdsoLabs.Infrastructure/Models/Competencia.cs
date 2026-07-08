namespace AdsoLabs.Infrastructure.Models;

public class Competencia
{
    public int IdCompetencia { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public int HorasAsignadas { get; set; }
    public string Tipo { get; set; } = "Tecnica"; // Tecnica | Transversal | Clave | Induccion | Practica
    public string Estado { get; set; } = "Activa";

    // Fase del proyecto formativo en la que típicamente se dicta esta competencia dentro
    // de una ficha ADSO (Induccion|Analisis|Planeacion|Ejecucion|Evaluacion|Transversal).
    // Null = sin clasificar (se trata como Transversal: no bloquea ni es bloqueada).
    public string? FaseFormativa { get; set; }

    // Navegación
    public ICollection<FichaCompetencia> FichasCompetencias { get; set; } = [];
    public ICollection<PlanConcertado> PlanesConcertados { get; set; } = [];
    public ICollection<InstrumentoEvaluacion> InstrumentosEvaluacion { get; set; } = [];
    public ICollection<DesarrolloCurricular> DesarrollosCurriculares { get; set; } = [];
}
