namespace AdsoLabs.Core.Entities;

public class AprendizPerfil
{
    public int IdAprendiz { get; set; }
    public int IdPersona { get; set; }
    public int? IdUsuario { get; set; }
    public byte? Estrato { get; set; }
    public string? CondicionEspecial { get; set; }
    public string? TipoPoblacion { get; set; }
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }
    public DateTime FechaRegistro { get; set; }
}
