namespace AdsoLabs.Application.DTOs.Importacion;

public class ImportacionErrorDTO
{
    public int    NumeroFila  { get; set; }
    public string MotivoError { get; set; } = string.Empty;
    public string? DatosFila  { get; set; }
}
