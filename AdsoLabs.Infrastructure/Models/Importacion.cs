namespace AdsoLabs.Infrastructure.Models;

public class ImportacionArchivo
{
    public int IdImportacion { get; set; }
    public string TipoEntidad { get; set; } = null!;
    public int IdDocumento { get; set; }
    public int IdUsuario { get; set; }
    public string EstadoProcesamiento { get; set; } = "Pendiente"; // Pendiente | Procesado | Error
    public int? TotalFilas { get; set; }
    public int? FilasOk { get; set; }
    public int? FilasError { get; set; }
    public string? Observacion { get; set; }
    public DateTime FechaSubida { get; set; }
    public DateTime? FechaProcesado { get; set; }

    // Navegación
    public Archivo Documento { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
    public Sesion? SesionDestino { get; set; }
    public ICollection<ImportacionError> Errores { get; set; } = [];
}

public class ImportacionError
{
    public int IdError { get; set; }
    public int IdImportacion { get; set; }
    public int NumeroFila { get; set; }
    public string? DatosFila { get; set; }
    public string MotivoError { get; set; } = null!;

    // Navegación
    public ImportacionArchivo Importacion { get; set; } = null!;
}
