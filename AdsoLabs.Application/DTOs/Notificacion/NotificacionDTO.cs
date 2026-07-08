namespace AdsoLabs.Application.DTOs.Notificacion;

public class NotificacionDTO
{
    public int      IdNotificacion { get; set; }
    public string   Tipo           { get; set; } = "";
    public string   Titulo         { get; set; } = "";
    public string   Mensaje        { get; set; } = "";
    public bool     Leida          { get; set; }
    public DateTime FechaCreacion  { get; set; }
    public int?     ReferenciaId   { get; set; }
    public string?  ReferenciaTipo { get; set; }
}
