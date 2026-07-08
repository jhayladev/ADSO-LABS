namespace AdsoLabs.Application.DTOs.Asistencia;

public class GuardarAsistenciaDTO
{
    public int                     IdSesion { get; set; }
    public List<ItemAsistenciaDTO> Items    { get; set; } = [];
}

public class ItemAsistenciaDTO
{
    public int     IdAprendiz         { get; set; }
    public string  Estado             { get; set; } = "";
    public string? MotivoInasistencia { get; set; }
}
