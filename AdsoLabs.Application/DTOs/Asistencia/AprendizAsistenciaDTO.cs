namespace AdsoLabs.Application.DTOs.Asistencia;

public class AprendizAsistenciaDTO
{
    public int     IdAprendiz          { get; set; }
    public string  NombreCompleto      { get; set; } = "";
    public string  NumeroDocumento     { get; set; } = "";
    public string? Estado              { get; set; }  // null = sin registro aún
    public string? MotivoInasistencia  { get; set; }
}
