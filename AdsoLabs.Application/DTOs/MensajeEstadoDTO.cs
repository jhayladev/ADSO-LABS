namespace AdsoLabs.Application.DTOs;

public class MensajeEstadoDTO
{
    public bool    esExitoso { get; set; }
    public string  Mensaje   { get; set; } = string.Empty;
    /// <summary>ID del recurso creado (opcional). Usado p.ej. para devolver IdSesion tras CrearSesion.</summary>
    public int?    IdCreado  { get; set; }
}
