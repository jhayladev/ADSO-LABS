namespace AdsoLabs.Application.DTOs.Informe;

public class ObservacionAprendizDTO
{
    public int      IdObservacion { get; set; }
    public string   Descripcion   { get; set; } = string.Empty;
    public DateTime Fecha         { get; set; }
    public int      IdUsuario     { get; set; }
    public string   NombreUsuario { get; set; } = string.Empty;
}
