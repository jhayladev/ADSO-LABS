namespace AdsoLabs.Infrastructure.Models;

public class HistorialEstadoAprendiz
{
    public int IdHistorial { get; set; }
    public int IdFicha { get; set; }
    public int IdAprendiz { get; set; }
    public string EstadoAnterior { get; set; } = null!;
    public string EstadoNuevo { get; set; } = null!;
    public int? IdUsuarioCambio { get; set; }
    public DateTime FechaCambio { get; set; }

    // Navegación
    public FichaAprendiz FichaAprendiz { get; set; } = null!;
}
