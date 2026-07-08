namespace AdsoLabs.Application.DTOs.Importacion;

public class ImportacionResumenDTO
{
    public int      IdImportacion       { get; set; }
    public string   NombreArchivo       { get; set; } = string.Empty;
    public string   NombreUsuario       { get; set; } = string.Empty;
    public string   Estado              { get; set; } = string.Empty;
    public string   Observacion        { get; set; } = string.Empty;
    public int?     TotalFilas          { get; set; }
    public int?     FilasOk             { get; set; }
    public int?     FilasError          { get; set; }
    public DateTime FechaSubida         { get; set; }
    public DateTime? FechaProcesado     { get; set; }
}
