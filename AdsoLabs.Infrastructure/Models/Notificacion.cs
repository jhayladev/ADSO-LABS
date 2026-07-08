namespace AdsoLabs.Infrastructure.Models;

public class Notificacion
{
    public int      IdNotificacion        { get; set; }
    public int      IdUsuarioDestinatario { get; set; }
    public string   Tipo                  { get; set; } = null!;
    public string   Titulo                { get; set; } = null!;
    public string   Mensaje               { get; set; } = null!;
    public bool     Leida                 { get; set; }
    public DateTime FechaCreacion         { get; set; }
    public DateTime? FechaLectura         { get; set; }
    public int?     ReferenciaId          { get; set; }
    public string?  ReferenciaTipo        { get; set; }

    public Usuario UsuarioDestinatario { get; set; } = null!;
}
