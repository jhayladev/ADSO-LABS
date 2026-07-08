namespace AdsoLabs.Infrastructure.Models;

public class Ficha
{
    public int IdFicha { get; set; }
    public string NumeroFicha { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Jornada { get; set; }            // Mañana | Tarde
    public string? Modalidad { get; set; }          // Presencial | Virtual
    public string? Descripcion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string Estado { get; set; } = "Activa"; // Activa | Finalizada | Suspendida
    public DateTime FechaCreacion { get; set; }

    // Navegación
    public ICollection<FichaCompetencia> FichasCompetencias { get; set; } = [];
    public ICollection<ProyectoFormativo> ProyectosFormativos { get; set; } = [];
    public ICollection<Patrocinio> Patrocinios { get; set; } = [];
    public ICollection<AprendizPerfil> Aprendices { get; set; } = [];     // skip navigation
    public ICollection<FichaAprendiz> FichaAprendices { get; set; } = []; // explicit join
}
