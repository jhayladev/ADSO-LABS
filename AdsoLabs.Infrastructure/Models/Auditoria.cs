namespace AdsoLabs.Infrastructure.Models;

public class Auditoria
{
    public long IdAuditoria { get; set; }
    public int? IdUsuario { get; set; }
    public string Accion { get; set; } = null!; // INSERT | UPDATE | DELETE
    public string TablaAfectada { get; set; } = null!;
    public string IdRegistroAfectado { get; set; } = null!;
    public string? DatosViejos { get; set; }
    public string? DatosNuevos { get; set; }
    public DateTime Fecha { get; set; }
    public string? DireccionIp { get; set; }
}
