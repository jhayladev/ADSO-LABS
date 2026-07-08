namespace AdsoLabs.Application.DTOs.Monitoria;

public class InscripcionMonitoriaDTO
{
    public int IdInscripcion { get; set; }
    public int IdAprendiz { get; set; }
    public string NombreAprendiz { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public DateTime FechaInscripcion { get; set; }
}
