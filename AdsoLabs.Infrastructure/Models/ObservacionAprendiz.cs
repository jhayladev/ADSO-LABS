namespace AdsoLabs.Infrastructure.Models;

public class ObservacionAprendiz
{
    public int    IdObservacion { get; set; }
    public int    IdAprendiz   { get; set; }
    public string Descripcion  { get; set; } = null!;
    public DateTime Fecha      { get; set; }
    public int    IdUsuario    { get; set; }

    // Navegación
    public AprendizPerfil Aprendiz { get; set; } = null!;
    public Usuario        Usuario  { get; set; } = null!;
}
