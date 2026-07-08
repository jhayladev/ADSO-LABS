namespace AdsoLabs.Infrastructure.Models;

public class FichaAprendiz
{
    public int IdFicha { get; set; }
    public int IdAprendiz { get; set; }
    public string Estado { get; set; } = "En Formacion";

    public Ficha Ficha { get; set; } = null!;
    public AprendizPerfil Aprendiz { get; set; } = null!;
}
