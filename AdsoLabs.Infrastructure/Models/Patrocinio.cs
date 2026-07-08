namespace AdsoLabs.Infrastructure.Models;

public class Patrocinio
{
    public int IdPatrocinio { get; set; }
    public int IdAprendiz { get; set; }
    public int IdFicha { get; set; }
    public bool Activo { get; set; }
    public string Etapa { get; set; } = "Lectiva"; // Lectiva | Productiva
    public DateOnly? FechaInicioEtapa { get; set; }
    public DateOnly? FechaFinEtapa { get; set; }
    public TimeOnly? HoraInicio { get; set; }
    public TimeOnly? HoraFin { get; set; }
    public string? NombreEmpresa { get; set; }
    public string? ContactoEmpresa { get; set; }
    public DateTime FechaRegistro { get; set; }

    public AprendizPerfil Aprendiz { get; set; } = null!;
    public Ficha Ficha { get; set; } = null!;
    public ICollection<AsistenciaPatrocinio> Asistencias { get; set; } = [];
}