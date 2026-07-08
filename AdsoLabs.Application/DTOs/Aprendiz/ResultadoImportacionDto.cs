namespace AdsoLabs.Application.DTOs.Aprendiz;

public class ResultadoImportacionDto
{
    public int IdImportacion { get; set; }
    public int TotalFilas { get; set; }
    public int FilasOk { get; set; }
    public int FilasError { get; set; }
    public List<string> Errores { get; set; } = [];
}
