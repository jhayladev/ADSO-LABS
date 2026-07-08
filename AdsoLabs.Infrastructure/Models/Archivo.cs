namespace AdsoLabs.Infrastructure.Models;

public class TipoArchivo
{
    public int IdTipoArchivo { get; set; }
    public string Nombre { get; set; } = null!;

    public ICollection<Archivo> Archivos { get; set; } = [];
}

public class Archivo
{
    public int IdArchivo { get; set; }
    public string NombreOriginal { get; set; } = null!;
    public string NombreAlmacenado { get; set; } = null!;
    public string RutaStorage { get; set; } = null!;
    public string? ExtensionArchivo { get; set; }
    public string? MimeType { get; set; }
    public int IdTipoArchivo { get; set; }
    public int IdUsuarioSubio { get; set; }
    public long? TamanioBytes { get; set; }
    public int Version { get; set; }
    public string? SiteId { get; set; }
    public string? DriveId { get; set; }
    public string? ItemId { get; set; }
    public string? WebUrl { get; set; }
    public DateTime FechaSubida { get; set; }
    public bool Activo { get; set; }

    // Navegación
    public TipoArchivo TipoArchivo { get; set; } = null!;
    public Usuario UsuarioSubio { get; set; } = null!;
    public ICollection<GuiaAprendizaje> GuiasAprendizaje { get; set; } = [];
    public ICollection<InstrumentoEvaluacion> InstrumentosEvaluacion { get; set; } = [];
    public ICollection<PlaneacionPedagogica> PlaneacionesPedagogicas { get; set; } = [];
    public ICollection<ProyectoFormativo> ProyectosFormativos { get; set; } = [];
    public ICollection<DesarrolloCurricular> DesarrollosCurriculares { get; set; } = [];
    public ICollection<PlanConcertado> PlanesConcertados { get; set; } = [];
    public ICollection<ImportacionArchivo> Importaciones { get; set; } = [];
}
