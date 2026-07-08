namespace AdsoLabs.Application.DTOs.Importacion;

public class ResultadoImportacionDto
{
    public int          IdImportacion         { get; set; }
    public int          TotalFilas            { get; set; }
    public int          FilasOk               { get; set; }
    public int          FilasError            { get; set; }
    public int          AprendicesCreados     { get; set; }
    public int          AprendicesActualizados { get; set; }
    public int          JuiciosInsertados     { get; set; }
    public int          JuiciosActualizados   { get; set; }
    public List<string> FichasAfectadas       { get; set; } = [];
    public List<string> Errores               { get; set; } = [];
}
